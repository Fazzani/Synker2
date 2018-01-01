﻿using hfa.PlaylistBaseLibrary.Providers;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Entities.Playlists;
using hfa.Synker.Service.Services;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Services.MediaRefs;
using hfa.Synker.Service.Services.Playlists;
using hfa.Synker.Service.Services.Scraper;
using hfa.Synker.Services.Dal;
using hfa.WebApi.Common;
using hfa.WebApi.Common.Filters;
using hfa.WebApi.Models.Playlists;
using Hfa.WebApi.Commmon;
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
    [Route("api/v1/[controller]")]
    [Authorize]
    public class PlaylistsController : BaseController
    {
        private IPlaylistService _playlistService;
        private IMediaScraper _mediaScraper;
        IMemoryCache _memoryCache;
        private ISitePackService _sitePackService;
        private GlobalOptions _globalOptions;

        private string UserPlaylistKey => $"{UserId}:{CacheKeys.PlaylistByUser}";

        Guid GetInternalId(string id) => new Guid(Encoding.UTF8.DecodeBase64(id));

        public PlaylistsController(IMemoryCache memoryCache, IMediaScraper mediaScraper, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory, IOptions<GlobalOptions> globalOptions,
            IElasticConnectionClient elasticConnectionClient, SynkerDbContext context, IPlaylistService playlistService, ISitePackService sitePackService)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _playlistService = playlistService;
            _mediaScraper = mediaScraper;
            _memoryCache = memoryCache;
            _sitePackService = sitePackService;
            _globalOptions = globalOptions.Value;
        }

        /// <summary>
        /// List Messages
        /// </summary>
        /// <returns></returns>
        [HttpPost("search")]
        [ValidateModel]
        public async Task<IActionResult> List([FromBody] QueryListBaseModel query, CancellationToken cancellationToken, [FromQuery] bool light = true)
        {
            var plCacheKey = $"{UserPlaylistKey}_{query.GetHashCode()}";
            var playlists = await _memoryCache.GetOrCreateAsync(plCacheKey, async entry =>
            {
                if (_memoryCache.TryGetValue(UserPlaylistKey, out List<string> list))
                {
                    list.Add(UserPlaylistKey);
                }
                else
                {
                    list = new List<string> { plCacheKey };
                }

                _memoryCache.Set(UserPlaylistKey, list);

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
            var idGuid = GetInternalId(id);
            var playlist = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlist == null)
                return NotFound(id);

            return light ? Ok(PlaylistModel.ToLightModel(playlist, Url)) : Ok(PlaylistModel.ToModel(playlist, Url));
        }

        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> PutAsync(string id, [FromBody]PlaylistModel playlist, CancellationToken cancellationToken)
        {
            var idGuid = GetInternalId(id);

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
            var idGuid = GetInternalId(id);

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
            var idGuid = GetInternalId(id);

            var playlist = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlist == null)
                return NotFound(playlist);

            _dbContext.Playlist.Remove(playlist);

            await _dbContext.SaveChangesAsync();
            ClearCache();
            return NoContent();
        }

        /// <summary>
        /// Passe Handlers
        /// </summary>
        /// <param name="playlistPostModel"></param>
        /// <param name="providersOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("handlers")]
        public IActionResult ExecuteHandlers([FromBody]List<TvgMedia> tvgMedias, CancellationToken cancellationToken)
        {
            var result = _playlistService.ExecuteHandlersAsync(tvgMedias, cancellationToken);
            return Ok(result);
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
                    Status = playlistPostModel.Status,
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
        /// Genére un rapport avec les new medias et 
        /// les médias qui n'existes plus
        /// </summary>
        /// <param name="playlistPostModel"></param>
        /// <param name="providersOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("diff")]
        [ValidateModel]
        public async Task<IActionResult> DiffAsync([FromBody]PlaylistPostModel playlistPostModel, [FromServices] IOptions<List<PlaylistProviderOption>> providersOptions,
            CancellationToken cancellationToken)
        {
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
                    Status = playlistPostModel.Status,
                    SynkConfig = new SynkConfig { Url = playlistPostModel.Url, Provider = playlistPostModel.Provider }
                };

                var pl = await _playlistService.DiffWithSource(() => playlist, providerInstance, cancellationToken: cancellationToken);
                return Ok(pl);
            }
        }

        ///// <summary>
        ///// Match playlist with media ref
        ///// </summary>
        ///// <param name="playlistPostModel"></param>
        ///// <param name="cancellationToken"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("match/{id}")]
        //[ValidateModel]
        //public IActionResult Match([FromRoute] string id, CancellationToken cancellationToken)
        //{
        //    var idGuid = GetInternalId(id);

        //    var playlist = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
        //    if (playlist == null)
        //        return NotFound(playlist);

        //    playlist.TvgMedias.Where(m => m.MediaType == MediaType.LiveTv).AsParallel().ForAll(media =>
        //        {
        //            var matched = _sitePackService.MatchMediaNameAndBySiteAsync(media.DisplayName,media.Tvg.TvgSource.Country, cancellationToken).GetAwaiter().GetResult();

        //            if (matched != null)
        //            {
        //                media.Group = matched.Cultures.FirstOrDefault();
        //                media.Tvg = matched.Tvg;
        //            }
        //        });

        //    return Ok(PlaylistModel.ToModel(playlist, Url));
        //}

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
            var idGuid = GetInternalId(id);

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            var list = playlistEntity.TvgMedias.Where(m => m.MediaType == MediaType.LiveTv).ToList();

            if (onlyNotMatched)
                list = list.Where(x => x.Tvg == null || string.IsNullOrEmpty(x.Tvg.Id)).ToList();

            //TODO : Match by culture and Country code

            list.AsParallel().ForAll(media =>
               {
                   var matched = _sitePackService.MatchTermByDispaynamesAndFiltredBySiteNameAsync(media.DisplayName, media.Lang, playlistEntity.TvgSites, cancellationToken).GetAwaiter().GetResult();
                   if (matched != null)
                   {
                       media.Group = matched.Country;
                       if (media.Tvg == null)
                       {
                           media.Tvg = new Tvg { Name = matched.Channel_name, TvgIdentify = matched.id, TvgSiteSource = matched.Site, Id = matched.Xmltv_id };
                       }
                       else
                       {
                           media.Tvg.Name = matched.Channel_name;
                           media.Tvg.Id = matched.Xmltv_id;
                           media.Tvg.TvgIdentify = matched.id;
                           media.Tvg.TvgSiteSource = matched.Site;
                           if (media.Tvg.TvgSource == null)
                               media.Tvg.TvgSource = new hfa.PlaylistBaseLibrary.Entities.TvgSource();
                           media.Tvg.TvgSource.Site = matched.Site;
                           media.Tvg.TvgSource.Country = matched.Country;
                           media.Tvg.TvgSource.Code = matched.Site_id;
                       }
                   }
               });

            //playlistEntity.UpdateContent(playlistEntity.TvgMedias);

            //var res = await _dbContext.SaveChangesAsync(cancellationToken);
            //_logger.LogInformation($"{nameof(MatchFiltredByTvgSites)} saved : {res}");
            return Ok(PlaylistModel.ToModel(playlistEntity, Url));
        }

        /// <summary>
        ///  Match playlist tvg (site pack directement)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("matchtvg/{id}")]
        [ValidateModel]
        public async Task<IActionResult> MatchTvg([FromRoute] string id, [FromQuery] bool onlyNotMatched = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var idGuid = GetInternalId(id);

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            playlistEntity.TvgMedias
                .Where(m => m.MediaType == MediaType.LiveTv && (!onlyNotMatched || m.Tvg == null || string.IsNullOrEmpty(m.Tvg.Id)))
                .AsParallel()
                .WithCancellation(cancellationToken)
                .ForAll(media =>
                  {
                      var matched = _sitePackService.MatchMediaNameAndBySiteAsync(media.DisplayName, media.Tvg.TvgSource.Site, cancellationToken).GetAwaiter().GetResult();
                      if (matched != null)
                      {
                          media.Tvg.Id = matched.id;
                          media.Tvg.Name = matched.Xmltv_id;
                      }
                  });

            //Matching movies
            var list = playlistEntity.TvgMedias.Where(m => m.MediaType == MediaType.Video).ToList();
            if (onlyNotMatched)
                list = list.Where(x => x.Tvg == null || string.IsNullOrEmpty(x.Tvg.Logo)).ToList();

            list
                .Where(x => x.Tvg != null && x.Tvg.TvgSource != null)
                .AsParallel()
                .WithCancellation(cancellationToken)
                .ForAll(media =>
                {
                    var matched = _mediaScraper.SearchAsync(media.DisplayName, _globalOptions.TmdbAPI, _globalOptions.TmdbPosterBaseUrl, cancellationToken).GetAwaiter().GetResult();
                    if (matched != null)
                    {
                        media.Tvg.Logo = matched.FirstOrDefault().PosterPath;
                    }
                });

            return Ok(PlaylistModel.ToModel(playlistEntity, Url));
        }

        [HttpPost]
        [Route("matchvideos/{id}")]
        [ValidateModel]
        public async Task<IActionResult> MatchVideos([FromRoute] string id, [FromQuery] bool onlyNotMatched = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var idGuid = GetInternalId(id);

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            int limitRequest = 40;

            playlistEntity.TvgMedias
                .Where(m => m.MediaType == MediaType.Video)
                .AsParallel()
                .WithCancellation(cancellationToken)
                .ForAll(media =>
                {
                    while (limitRequest <= 0)
                    {
                        _logger.LogInformation($"Scrapper Media limit TMDB (40 request/10s) was reached {media.DisplayName}");
                        Thread.Sleep(10001);
                        if (limitRequest <= 0)
                            Interlocked.Add(ref limitRequest, 40);
                    }

                    Interlocked.Decrement(ref limitRequest);
                    _logger.LogInformation($"Scrapping media {limitRequest}: {media.DisplayName}");

                    var matched = _mediaScraper.SearchAsync(media.DisplayName, _globalOptions.TmdbAPI, _globalOptions.TmdbPosterBaseUrl, cancellationToken).GetAwaiter().GetResult();
                    if (matched != null)
                    {
                        media.Tvg.Logo = matched.FirstOrDefault().PosterPath;
                    }
                });
           
            return Ok(PlaylistModel.ToModel(playlistEntity, Url));
        }

        [HttpPost]
        [Route("matchvideo/{name}")]
        [ValidateModel]
        public async Task<IActionResult> MatchVideo([FromRoute] string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            var matched = await _mediaScraper.SearchAsync(name, _globalOptions.TmdbAPI, _globalOptions.TmdbPosterBaseUrl, cancellationToken);

            if (matched != null)
            {
                return Ok(matched.FirstOrDefault());
            }

            return NotFound();
        }

        /// <summary>
        ///  Match playlist tvg (site pack directement)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("matchtvg/media")]
        [ValidateModel]
        public async Task<IActionResult> MatchTvgByMedia([FromBody] TvgMedia media, CancellationToken cancellationToken = default(CancellationToken))
        {
            var sitePack = await _sitePackService.MatchMediaNameAndBySiteAsync(media.DisplayName, media.Tvg.TvgSource.Site, cancellationToken);
            return Ok(sitePack);
        }

        [ResponseCache(CacheProfileName = "Long")]
        [AllowAnonymous]
        [HttpGet("files/{id:required}", Name = nameof(GetFile))]
        public async Task<IActionResult> GetFile(string id, [FromServices] IOptions<List<PlaylistProviderOption>> providersOptions,
          [FromQuery] string provider = "m3u", CancellationToken cancellationToken = default(CancellationToken))
        {
            var idGuid = GetInternalId(id);

            var playlist = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlist == null)
                return NotFound(id);

            using (var ms = new MemoryStream())
            using (var sourceProvider = FileProvider.Create(provider, providersOptions.Value, ms))
            using (var pl = new Playlist<TvgMedia>(sourceProvider))
            using (var sourcePl = new Playlist<TvgMedia>(playlist.TvgMedias.Where(x=>x.Enabled)))
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

                            ClearCache();

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
            ClearCache();

            var result = PlaylistModel.ToLightModel(pl, Url);
            return Created(result.PublicUrl, result);
        }

        private void ClearCache()
        {
            if (_memoryCache.TryGetValue(UserPlaylistKey, out List<string> list))
            {
                foreach (var item in list)
                {
                    _memoryCache.Remove(item);
                }
                list.Remove(UserPlaylistKey);
            }
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

                ClearCache();
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
