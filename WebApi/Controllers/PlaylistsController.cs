using hfa.PlaylistBaseLibrary.Providers;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Entities.Playlists;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Services.MediaRefs;
using hfa.Synker.Service.Services.Playlists;
using hfa.Synker.Services.Dal;
using hfa.WebApi.Common.Filters;
using hfa.WebApi.Models.Playlists;
using Hfa.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hfa.WebApi.Controllers
{
    //[ApiVersion("1.0")]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class PlaylistsController : BaseController
    {
        private IPlaylistService _playlistService;
        private IMediaRefService _mediaRefService;
        IMemoryCache _memoryCache;

        public PlaylistsController(IMemoryCache memoryCache, IMediaRefService mediaRefService, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
            IElasticConnectionClient elasticConnectionClient, SynkerDbContext context, IPlaylistService playlistService)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _playlistService = playlistService;
            _mediaRefService = mediaRefService;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// List Messages
        /// </summary>
        /// <returns></returns>
        [HttpPost("search")]
        [ValidateModel]
        public async Task<IActionResult> List([FromBody] QueryListBaseModel query, CancellationToken cancellationToken, [FromQuery] bool light = true)
        {
            var playlists = await _memoryCache.GetOrCreateAsync($"{CacheKeys.PlaylistByUser}_{UserId}_{query.GetHashCode()}", async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(2);
                return await Task.Run(() =>
                {
                    var response = _dbContext.Playlist
                   .Where(x => x.UserId == UserId)
                   .OrderByDescending(x => x.Id)
                   .Select(pl => light ? PlaylistModel.ToLightModel(pl, Url) : PlaylistModel.ToModel(pl, Url))
                   .GetPaged(query.PageNumber, query.PageSize);
                    return response;
                });
            });

            return new OkObjectResult(playlists);
        }

        [HttpGet("{id}")]
        //[ResponseCache(CacheProfileName = "Long", VaryByQueryKeys = new string[] { "id", "light" })]
        public IActionResult Get(string id, CancellationToken cancellationToken, [FromQuery] bool light = true)
        {
            var idGuid = new Guid(Encoding.UTF8.DecodeBase64(id));
            var playlist = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlist == null)
                return NotFound(id);

            return light ? Ok(PlaylistModel.ToLightModel(playlist, Url)) : Ok(PlaylistModel.ToModel(playlist, Url));
        }

        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> PutAsync(string id, [FromBody]PlaylistModel playlist, CancellationToken cancellationToken)
        {
            var idGuid = new Guid(Encoding.UTF8.DecodeBase64(id));

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            playlistEntity.Status = playlist.Status;
            playlistEntity.Freindlyname = playlist.Freindlyname;
            playlistEntity.TvgSites = playlist.TvgSites;
            playlistEntity.SynkConfig.Cron = playlist.Cron;
            playlistEntity.SynkConfig.Url = playlist.Url;
            playlistEntity.SynkConfig.SynkEpg = playlist.SynkEpg;
            playlistEntity.SynkConfig.SynkGroup = playlist.SynkGroup;
            playlistEntity.SynkConfig.SynkLogos = playlist.SynkLogos;
            playlistEntity.UpdateContent(playlist.TvgMedias);

            var updatedCount = await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Updated Count : {updatedCount}");

            return Ok(updatedCount);
        }

        [HttpPut("light/{id}")]
        [ValidateModel]
        public async Task<IActionResult> PutLightAsync(string id, [FromBody]PlaylistModel playlist, CancellationToken cancellationToken)
        {
            var idGuid = new Guid(Encoding.UTF8.DecodeBase64(id));

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            playlistEntity.Status = playlist.Status;
            playlistEntity.Freindlyname = playlist.Freindlyname;
            playlistEntity.TvgSites = playlist.TvgSites.Distinct().ToList();
            playlistEntity.SynkConfig.Cron = playlist.Cron;
            playlistEntity.SynkConfig.Url = playlist.Url;
            playlistEntity.SynkConfig.SynkEpg = playlist.SynkEpg;
            playlistEntity.SynkConfig.SynkGroup = playlist.SynkGroup;
            playlistEntity.SynkConfig.SynkLogos = playlist.SynkLogos;
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var idGuid = new Guid(Encoding.UTF8.DecodeBase64(id));

            var playlist = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlist == null)
                return NotFound(playlist);

            _dbContext.Playlist.Remove(playlist);

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Synk playlist from source url
        /// </summary>
        /// <param name="playlistPostModel"></param>
        /// <param name="providersOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("synk")]
        [ValidateModel]
        public async Task<IActionResult> Synk(PlaylistPostModel playlistPostModel, [FromServices] IOptions<List<PlaylistProviderOption>> providersOptions,
            CancellationToken cancellationToken)
        {
            //Vérifier si la playlist existe-elle avant 
            var stopwatch = Stopwatch.StartNew();
            var optionsProvider = providersOptions.Value.FirstOrDefault(x => x.Name.Equals(playlistPostModel.Provider, StringComparison.InvariantCultureIgnoreCase));
            if (optionsProvider == null)
                return BadRequest($"Not supported Provider : {playlistPostModel.Provider}");

            var providerType = Type.GetType(optionsProvider.Type, false, true);
            if (providerType == null)
                return BadRequest($"Provider type not found : {playlistPostModel.Provider}");

            //Download playlist from url
            using (var httpClient = new HttpClient())
            {
                var playlistStream = await httpClient.GetStreamAsync(playlistPostModel.Url);
                var providerInstance = (FileProvider)Activator.CreateInstance(providerType, playlistStream);

                var playlist = _dbContext.Playlist.FirstOrDefault(x => x.SynkConfig.Url == playlistPostModel.Url) ?? new Playlist
                {
                    UserId = UserId.Value,
                    Freindlyname = playlistPostModel.Freindlyname,
                    Status = PlaylistStatus.Enabled,
                    SynkConfig = new SynkConfig { Url = playlistPostModel.Url, Provider = playlistPostModel.Provider }
                };

                var pl = await _playlistService.SynkPlaylist(() => playlist, providerInstance, cancellationToken: cancellationToken);

                if (playlist.UniqueId == default(Guid))
                    await _dbContext.Playlist.AddAsync(pl);

                var res = await _dbContext.SaveChangesAsync(cancellationToken);

                stopwatch.Stop();
                _logger.LogInformation($"Elapsed time : {stopwatch.Elapsed.ToString("c")}");
                return CreatedAtRoute(nameof(GetFile), new { id = UTF8Encoding.UTF8.EncodeBase64(pl.UniqueId.ToString()) }, null);
            }
        }

        /// <summary>
        /// Match playlist with media ref
        /// </summary>
        /// <param name="playlistPostModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("match/{id}")]
        [ValidateModel]
        public IActionResult Match([FromRoute] string id, CancellationToken cancellationToken)
        {
            var idGuid = new Guid(Encoding.UTF8.DecodeBase64(id));

            var playlist = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlist == null)
                return NotFound(playlist);

            playlist.TvgMedias.AsParallel().ForAll(media =>
            {
                var matched = _mediaRefService.MatchTermByDispaynamesAsync(media.DisplayName, cancellationToken).GetAwaiter().GetResult();

                if (matched != null)
                {
                    media.Group = matched.Cultures.FirstOrDefault();
                    media.Tvg = matched.Tvg;
                }
            });

            return Ok(PlaylistModel.ToModel(playlist, Url));
        }

        /// <summary>
        ///  Match playlist with media ref
        /// </summary>
        /// <param name="id"></param>
        /// <param name="onlyNotMatched"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("matchfiltred/{id}")]
        [ValidateModel]
        public async Task<IActionResult> MatchFiltredByTvgSites([FromRoute] string id, [FromQuery] bool onlyNotMatched = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var idGuid = new Guid(Encoding.UTF8.DecodeBase64(id));

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            var list = playlistEntity.TvgMedias;

            if(onlyNotMatched)
                list = playlistEntity.TvgMedias.Where(x => x.Tvg == null).ToList();
           
            list.AsParallel().ForAll(media =>
               {
                   var matched = _mediaRefService.MatchTermByDispaynamesAndFiltredBySiteNameAsync(media.DisplayName, media.Lang, playlistEntity.TvgSites, cancellationToken).GetAwaiter().GetResult();
                   media.Group = matched?.DefaultSite;
                   media.Tvg = matched?.Tvg;
               });

            playlistEntity.UpdateContent(playlistEntity.TvgMedias);

            var res = await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"{nameof(MatchFiltredByTvgSites)} saved : {res}");
            return Ok(PlaylistModel.ToModel(playlistEntity, Url));
        }

        [ResponseCache(CacheProfileName = "Long")]
        [AllowAnonymous]
        [HttpGet("files/{id:required}", Name = nameof(GetFile))]
        public async Task<IActionResult> GetFile(string id, [FromServices] IOptions<List<PlaylistProviderOption>> providersOptions,
          [FromQuery] string provider = "m3u", CancellationToken cancellationToken = default(CancellationToken))
        {
            var idGuid = new Guid(Encoding.UTF8.DecodeBase64(id));

            var playlist = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlist == null)
                return NotFound(id);

            using (var ms = new MemoryStream())
            using (var sourceProvider = FileProvider.Create(provider, providersOptions.Value, ms))
            using (var pl = new Playlist<TvgMedia>(sourceProvider))
            using (var sourcePl = new Playlist<TvgMedia>(playlist.TvgMedias))
            {
                ms.Seek(0, SeekOrigin.Begin);
                await pl.PushAsync(sourcePl, cancellationToken);
                return File(ms.GetBuffer(), "text/plain");
            }
        }

        /// <summary>
        /// Export from provider to another
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        /// <param name="file"></param>
        /// <param name="providersOptions"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("export/{fromType}/{toType}")]
        public async Task<IActionResult> Export(string fromType, string toType, IFormFile file, [FromServices] IOptions<List<PlaylistProviderOption>> providersOptions,
            CancellationToken cancellationToken)
        {
            try
            {
                var sourceProvider = FileProvider.Create(fromType, providersOptions.Value, file.OpenReadStream());

                using (var stream = new MemoryStream())
                {
                    var targetProvider = FileProvider.Create(toType, providersOptions.Value, stream);

                    using (var sourcePlaylist = new Playlist<TvgMedia>(sourceProvider))
                    {
                        var sourceList = await sourcePlaylist.PullAsync(HttpContext.RequestAborted);
                        using (var targetPlaylist = new Playlist<TvgMedia>(targetProvider))
                        {
                            await targetPlaylist.PushAsync(sourcePlaylist, HttpContext.RequestAborted);
                            return File(stream.GetBuffer(), "text/plain", file.FileName);
                        }
                    }
                }
            }
            catch (InvalidFileProviderException ifileEx)
            {
                return BadRequest(ifileEx.Message);
            }
        }

        #region Import

        /// <summary>
        /// Add new Upload playlist from stream
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="playlistUrl">if playlist not null the file param will ignored</param>
        /// <param name="toType"></param>
        /// <param name="file"></param>
        /// <param name="providersOptions"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("create/{provider}")]
        [AllowAnonymous]
        public async Task<IActionResult> Import(string playlistName, string playlistUrl, string provider, IFormFile file, [FromServices] IOptions<List<PlaylistProviderOption>> providersOptions,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(playlistName))
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (string.IsNullOrEmpty(playlistName))
            {
                playlistName = file.FileName.Replace(Path.GetExtension(file.FileName), string.Empty); //TODO : catch all ArgumentException et les passer en BadRequest
            }
            //Vérifier si la playlist existe-elle avant 

            var optionsProvider = providersOptions.Value.FirstOrDefault(x => x.Name.Equals(provider, StringComparison.InvariantCultureIgnoreCase));
            if (optionsProvider == null)
                return BadRequest($"Not supported Provider : {provider}");

            var providerType = Type.GetType(optionsProvider.Type, false, true);
            if (providerType == null)
                return BadRequest($"Provider type not found : {provider}");

            var playlistStream = file.OpenReadStream();
            if (string.IsNullOrEmpty(playlistUrl))
            {
                //Download playlist from url
                using (var httpClient = new HttpClient())
                {
                    playlistStream = await httpClient.GetStreamAsync(playlistUrl);
                }
            }

            var pl = await SavePlaylist(playlistName, providerType, playlistStream, playlistUrl, cancellationToken);

            var result = PlaylistModel.ToLightModel(pl, Url);
            return Created(result.PublicUrl, result);
        }

        /// <summary>
        /// Add new Upload playlist from url
        /// </summary>
        /// <param name="playlistPostModel"></param>
        /// <param name="providersOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("create")]
        [ValidateModel]
        public async Task<IActionResult> ImportFromUrl([FromBody]PlaylistPostModel playlistPostModel, [FromServices] IOptions<List<PlaylistProviderOption>> providersOptions,
            CancellationToken cancellationToken)
        {
            //Vérifier si la playlist existe-elle avant 
            var stopwatch = Stopwatch.StartNew();
            var optionsProvider = providersOptions.Value.FirstOrDefault(x => x.Name.Equals(playlistPostModel.Provider, StringComparison.InvariantCultureIgnoreCase));
            if (optionsProvider == null)
                return BadRequest($"Not supported Provider : {playlistPostModel.Provider}");

            var providerType = Type.GetType(optionsProvider.Type, false, true);
            if (providerType == null)
                return BadRequest($"Provider type not found : {playlistPostModel.Provider}");

            //Download playlist from url
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.94 Safari/537.36");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "text/plain");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                var playlistStream = await httpClient.GetStreamAsync(playlistPostModel.Url);
                var providerInstance = (FileProvider)Activator.CreateInstance(providerType, playlistStream);
                var pl = await _playlistService.SynkPlaylist(() => new Playlist
                {
                    UserId = UserId.Value,
                    Freindlyname = playlistPostModel.Freindlyname,
                    Status = PlaylistStatus.Enabled,
                    SynkConfig = new SynkConfig { Url = playlistPostModel.Url }
                }, providerInstance, cancellationToken: cancellationToken);

                await _dbContext.Playlist.AddAsync(pl);
                var res = await _dbContext.SaveChangesAsync(cancellationToken);

                stopwatch.Stop();
                _logger.LogInformation($"Elapsed time : {stopwatch.Elapsed.ToString("c")}");

                var model = PlaylistModel.ToLightModel(pl, Url);
                return Created(model.PublicUrl, model);
            }
        }

        private async Task<Playlist> SavePlaylist(string playlistName, Type providerType, Stream playlistStream, string playlistUrl, CancellationToken cancellationToken)
        {
            var providerInstance = (FileProvider)Activator.CreateInstance(providerType, playlistStream);

            using (var playlist = new Playlist<TvgMedia>(providerInstance))
            {
                var sourceList = await playlist.PullAsync(cancellationToken);
                var content = JsonConvert.SerializeObject(sourceList.ToArray());

                var synkcfg = new SynkConfig { Url = playlistUrl };
                var playlistEntity = new Playlist
                {
                    UserId = UserId.Value,
                    Freindlyname = playlistName,
                    Content = UTF8Encoding.UTF8.GetBytes(content),
                    Status = PlaylistStatus.Enabled,
                    SynkConfig = synkcfg
                };
                await _dbContext.Playlist.AddAsync(playlistEntity, cancellationToken);

                var res = await _dbContext.SaveChangesAsync(cancellationToken);
                return playlistEntity;
            }
        }
        #endregion
    }
}
