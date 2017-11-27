using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlaylistManager.Entities;
using System.Threading;
using Hfa.WebApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using hfa.PlaylistBaseLibrary.Providers;
using hfa.WebApi.Common.Filters;
using hfa.Synker.Services.Dal;
using hfa.Synker.Service.Entities.Playlists;
using hfa.Synker.Service.Services.Playlists;
using Newtonsoft.Json;
using hfa.WebApi.Models.Playlists;
using System.Net.Http;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Elastic;
using System.Diagnostics;
using System.Text;
using hfa.Synker.Service.Services.MediaRefs;

namespace Hfa.WebApi.Controllers
{
    //[ApiVersion("1.0")]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class PlaylistsController : BaseController
    {
        private IPlaylistService _playlistService;
        private IMediaRefService _mediaRefService;

        public PlaylistsController(IMediaRefService mediaRefService, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient,
            SynkerDbContext context, IPlaylistService playlistService)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _playlistService = playlistService;
            _mediaRefService = mediaRefService;
        }

        /// <summary>
        /// List Messages
        /// </summary>
        /// <returns></returns>
        [HttpPost("search")]
        [ValidateModel]
        public IActionResult List([FromBody] QueryListBaseModel query, CancellationToken cancellationToken, [FromQuery] bool light = true)
        {
            var response = _dbContext.Playlist
                .Where(x => x.UserId == UserId)
                .OrderByDescending(x => x.Id)
                .Select(pl => light ? PlaylistModel.ToLightModel(pl, Url) : PlaylistModel.ToModel(pl, Url))
                .GetPaged(query.PageNumber, query.PageSize);

            return new OkObjectResult(response);
        }

        [HttpGet("{id}")]
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
        public async Task<IActionResult> Put(string id, [FromBody]PlaylistModel playlist, CancellationToken cancellationToken)
        {
            var idGuid = new Guid(Encoding.UTF8.DecodeBase64(id));

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            playlistEntity.Status = playlist.Status;
            playlistEntity.Freindlyname = playlist.Freindlyname;
            playlistEntity.SynkConfig.Cron = playlist.Cron;
            playlistEntity.SynkConfig.Url = playlist.Url;
            playlistEntity.SynkConfig.SynkEpg = playlist.SynkEpg;
            playlistEntity.SynkConfig.SynkGroup = playlist.SynkGroup;
            playlistEntity.SynkConfig.SynkLogos = playlist.SynkLogos;
            playlistEntity.Content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(playlist.TvgMedias));
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
                var playlistStream = await httpClient.GetStreamAsync(playlistPostModel.PlaylistUrl);
                var providerInstance = (FileProvider)Activator.CreateInstance(providerType, playlistStream);

                var playlist = _dbContext.Playlist.FirstOrDefault(x => x.SynkConfig.Url == playlistPostModel.PlaylistUrl) ?? new Playlist
                {
                    UserId = UserId.Value,
                    Freindlyname = playlistPostModel.PlaylistName,
                    Status = PlaylistStatus.Enabled,
                    SynkConfig = new SynkConfig { Url = playlistPostModel.PlaylistUrl, Provider = playlistPostModel.Provider }
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
        /// <param name="providersOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/match")]
        [ValidateModel]
        public IActionResult Match(string id, [FromServices] IOptions<List<PlaylistProviderOption>> providersOptions,
            CancellationToken cancellationToken)
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

            return Ok(PlaylistModel.ToLightModel(pl, Url));
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
        public async Task<IActionResult> ImportFromUrl(PlaylistPostModel playlistPostModel, [FromServices] IOptions<List<PlaylistProviderOption>> providersOptions,
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
                var playlistStream = await httpClient.GetStreamAsync(playlistPostModel.PlaylistUrl);
                var providerInstance = (FileProvider)Activator.CreateInstance(providerType, playlistStream);
                var pl = await _playlistService.SynkPlaylist(() => new Playlist
                {
                    UserId = UserId.Value,
                    Freindlyname = playlistPostModel.PlaylistName,
                    Status = PlaylistStatus.Enabled,
                    SynkConfig = new SynkConfig { Url = playlistPostModel.PlaylistUrl }
                }, providerInstance, cancellationToken: cancellationToken);

                await _dbContext.Playlist.AddAsync(pl);
                var res = await _dbContext.SaveChangesAsync(cancellationToken);

                stopwatch.Stop();
                _logger.LogInformation($"Elapsed time : {stopwatch.Elapsed.ToString("c")}");
                return Ok(PlaylistModel.ToLightModel(pl, Url));
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
