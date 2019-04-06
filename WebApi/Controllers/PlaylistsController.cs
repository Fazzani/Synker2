using hfa.PlaylistBaseLibrary.Entities;
using hfa.PlaylistBaseLibrary.Providers;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Entities.MediaScraper;
using hfa.Synker.Service.Entities.Playlists;
using hfa.Synker.Service.Services;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Services.Playlists;
using hfa.Synker.Service.Services.Scraper;
using hfa.Synker.Service.Services.Xmltv;
using hfa.Synker.Service.Services.Xtream;
using hfa.Synker.Services.Dal;
using hfa.WebApi.Common;
using hfa.WebApi.Common.Auth;
using hfa.WebApi.Common.Exceptions;
using hfa.WebApi.Common.Filters;
using hfa.WebApi.Models;
using hfa.WebApi.Models.Playlists;
using Hfa.WebApi.Commmon;
using Hfa.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(AuthenticationSchemes = Authentication.AuthSchemes)]
    public class PlaylistsController : BaseController
    {
        private readonly IPlaylistService _playlistService;
        private readonly IMediaScraper _mediaScraper;
        private readonly IMemoryCache _memoryCache;
        private readonly ISitePackService _sitePackService;
        private readonly GlobalOptions _globalOptions;
        private readonly IXtreamService _xtreamService;
        private readonly IProviderFactory _providerFactory;

        private string UserCachePlaylistKey => $"{UserEmail}:{CacheKeys.PlaylistByUser}";

        public PlaylistsController(IMemoryCache memoryCache, IMediaScraper mediaScraper, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory, IOptions<GlobalOptions> globalOptions,
            IElasticConnectionClient elasticConnectionClient, SynkerDbContext context, IPlaylistService playlistService, ISitePackService sitePackService, IXtreamService xtreamService,
            IProviderFactory providerFactory)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _playlistService = playlistService ?? throw new ArgumentNullException(nameof(playlistService));
            _mediaScraper = mediaScraper ?? throw new ArgumentNullException(nameof(mediaScraper));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _sitePackService = sitePackService ?? throw new ArgumentNullException(nameof(sitePackService));
            _globalOptions = globalOptions.Value ?? throw new ArgumentNullException(nameof(globalOptions));
            _xtreamService = xtreamService ?? throw new ArgumentNullException(nameof(xtreamService));
            _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
        }

        /// <summary>
        /// List Messages
        /// </summary>
        /// <returns></returns>
        [HttpPost("search")]
        [ValidateModel]
        [ProducesResponseType(typeof(PagedResult<PlaylistModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        [Authorize(Policy = AuthorizePolicies.READER)]
        public async Task<IActionResult> ListAsync([FromBody] QueryListBaseModel query, [FromQuery] bool light = true, CancellationToken cancellationToken = default)
        {
            //TODO: A virer apres la migration de l'auth
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.Equals(this.UserEmail), cancellationToken);
            if (user == null) return BadRequest($"User {this.UserEmail} not found");

            var plCacheKey = $"{UserCachePlaylistKey}_{query.GetHashCode()}_{light}";
            var playlists = await _memoryCache.GetOrCreateAsync(plCacheKey, async entry =>
            {
                if (_memoryCache.TryGetValue(UserCachePlaylistKey, out List<string> list))
                {
                    list.Add(UserCachePlaylistKey);
                }
                else
                {
                    list = new List<string> { plCacheKey };
                }

                _memoryCache.Set(UserCachePlaylistKey, list);
                entry.SlidingExpiration = TimeSpan.FromHours(2);

                return await Task.Run(() =>
                {
                    var response = _dbContext.Playlist
                   .Where(x => x.UserId == user.Id)
                   .OrderByDescending(x => x.Id)
                   .Select(pl => light ? PlaylistModel.ToLightModel(pl, Url) : PlaylistModel.ToModel(pl, Url))
                   .GetPaged(query.PageNumber, query.PageSize);
                    return response;
                });
            }).ConfigureAwait(false);

            return new OkObjectResult(playlists);
        }

        [HttpGet("{id}")]
        //[ResponseCache(CacheProfileName = "Long", VaryByQueryKeys = new string[] { "id", "light" })]
        [ProducesResponseType(typeof(PlaylistModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = AuthorizePolicies.READER)]
        public async Task<IActionResult> GetAsync(string id, [FromQuery] bool light = true, CancellationToken cancellationToken = default)
        {
            var idGuid = GetInternalPlaylistId(id);
            var playlist = await _dbContext.Playlist.FirstOrDefaultAsync(x => x.UniqueId == idGuid, cancellationToken);
            if (playlist == null)
                return NotFound(id);

            //var tmp = playlist.TvgMedias.Where(x => x.Name.Contains("SPORTS MAX") && x.Group.Equals("beIN SPORTS"));

            //foreach (var item in tmp)
            //{
            //    if (item.Name.Contains('1'))
            //    {
            //        item.Tvg.Id = "max1";
            //        item.Tvg.Name = "max1";
            //        item.Tvg.TvgIdentify = "mena_sports/max1";
            //        item.Tvg.TvgSiteSource = "Bein.net";
            //    }
            //    else if (item.Name.Contains('2'))
            //    {
            //        item.Tvg.Id = "max2";
            //        item.Tvg.Name = "max2";
            //        item.Tvg.TvgIdentify = "mena_sports/max2";
            //        item.Tvg.TvgSiteSource = "Bein.net";
            //    }
            //    else if (item.Name.Contains('3'))
            //    {
            //        item.Tvg.Id = "max3";
            //        item.Tvg.Name = "max3";
            //        item.Tvg.TvgIdentify = "mena_sports/max3";
            //        item.Tvg.TvgSiteSource = "Bein.net";
            //    }
            //    else if (item.Name.Contains('4'))
            //    {
            //        item.Tvg.Id = "max4";
            //        item.Tvg.Name = "max4";
            //        item.Tvg.TvgIdentify = "mena_sports/max4";
            //        item.Tvg.TvgSiteSource = "Bein.net";
            //    }
            //}
            //await _dbContext.SaveChangesAsync(cancellationToken);

            var res = light ? Ok(PlaylistModel.ToLightModel(playlist, Url)) : Ok(PlaylistModel.ToModel(playlist, Url));
            return res;
        }

        [HttpPut("{id}")]
        [ValidateModel]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = AuthorizePolicies.FULLACCESS)]
        public async Task<IActionResult> PutAsync(string id, [FromBody]PlaylistModel playlist, CancellationToken cancellationToken = default)
        {
            var idGuid = GetInternalPlaylistId(id);

            var playlistEntity = await _dbContext.Playlist.FirstOrDefaultAsync(x => x.UniqueId == idGuid, cancellationToken);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            playlistEntity.Status = playlist.Status;
            playlistEntity.Freindlyname = playlist.Freindlyname;
            playlistEntity.TvgSites = playlist.TvgSites;
            playlistEntity.SynkConfig.Url = playlist.Url;
            playlistEntity.SynkConfig.SynkEpg = playlist.SynkEpg;
            playlistEntity.SynkConfig.SynkGroup = playlist.SynkGroup;
            playlistEntity.SynkConfig.SynkLogos = playlist.SynkLogos;
            playlistEntity.TvgMedias = playlist.TvgMedias;

            if (playlist.Tags == null)
            {
                playlist.Tags = new Dictionary<string, string>
                {
                    { PlaylistTags.IsXtream, _xtreamService.IsXtreamPlaylist(playlist.Url).ToString() }
                };
                playlistEntity.Tags = playlist.Tags;
            }
            else
            {
                //todo: à virer tout le bloc else
                if (playlistEntity.ImportProviderTag == null && playlist.Tags.TryAdd(PlaylistTags.ImportProvider, "m3u"))
                {
                    playlistEntity.Tags = playlist.Tags;
                }
            }

            var updatedCount = await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Updated Count : {updatedCount}");
            return Ok(updatedCount);
        }

        [HttpPut("light/{id}")]
        [ValidateModel]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = AuthorizePolicies.FULLACCESS)]
        public async Task<IActionResult> PutLightAsync(string id, [FromBody]PlaylistModel playlist, CancellationToken cancellationToken = default)
        {
            var idGuid = GetInternalPlaylistId(id);

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            playlistEntity.Status = playlist.Status;
            playlistEntity.Freindlyname = playlist.Freindlyname;
            playlistEntity.TvgSites = playlist.TvgSites.Distinct().ToList();
            playlistEntity.SynkConfig.Url = playlist.Url;
            playlistEntity.SynkConfig.SynkEpg = playlist.SynkEpg;
            playlistEntity.SynkConfig.SynkGroup = playlist.SynkGroup;
            playlistEntity.SynkConfig.SynkLogos = playlist.SynkLogos;

            if (playlist.Tags == null)
            {
                playlist.Tags = new Dictionary<string, string>
                {
                    { PlaylistTags.IsXtream, _xtreamService.IsXtreamPlaylist(playlist.Url).ToString() }
                };
                playlistEntity.Tags = playlist.Tags;
            }
            else
            {
                //todo: à virer tout le bloc else
                if (playlistEntity.ImportProviderTag == null && playlist.Tags.TryAdd(PlaylistTags.ImportProvider, "m3u"))
                {
                    playlistEntity.Tags = playlist.Tags;
                }
            }

            await _dbContext.SaveChangesAsync();
            ClearCache();
            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(Playlist), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [Authorize(Policy = AuthorizePolicies.FULLACCESS)]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
        {
            var idGuid = GetInternalPlaylistId(id);

            var playlist = await _dbContext.Playlist.FirstOrDefaultAsync(x => x.UniqueId == idGuid, cancellationToken);
            if (playlist == null)
                return NotFound(playlist);

            _dbContext.Playlist.Remove(playlist);

            await _dbContext.SaveChangesAsync(cancellationToken);
            ClearCache();
            return NoContent();
        }

        /// <summary>
        /// Passe Handlers
        /// </summary>
        /// <param name="tvgMedias"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("handlers")]
        [ProducesResponseType(typeof(List<TvgMedia>), StatusCodes.Status200OK)]
        public IActionResult ExecuteHandlers([FromBody]List<TvgMedia> tvgMedias, CancellationToken cancellationToken = default)
        {
            var result = _playlistService.ExecuteHandlersAsync(tvgMedias, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Synk playlist from source url
        /// override tvgmedias from source
        /// </summary>
        /// <param name="playlistPostModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("synk")]
        [ValidateModel]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Authorize(Policy = AuthorizePolicies.FULLACCESS)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> SynkAsync([FromBody]PlaylistPostModel playlistPostModel, CancellationToken cancellationToken = default)
        {
            //TODO: A virer apres la migration de l'auth
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.Equals(this.UserEmail), cancellationToken);
            if (user == null) return BadRequest($"User {this.UserEmail} not found");

            var playlist = _dbContext.Playlist.AsNoTracking().FirstOrDefault(x => x.SynkConfig.Url == playlistPostModel.Url) ?? new Playlist
            {
                UserId = user.Id,
                Freindlyname = playlistPostModel.Freindlyname,
                Status = playlistPostModel.Status,
                SynkConfig = new SynkConfig { Url = playlistPostModel.Url, Provider = playlistPostModel.Provider }
            };

            var pl = await _playlistService.SynkPlaylistAsync(playlist, true, cancellationToken: cancellationToken);

            return CreatedAtRoute(nameof(SynkAsync), new { id = UTF8Encoding.UTF8.EncodeBase64(pl.UniqueId.ToString()) }, null);
        }

        /// <summary>
        /// Genére un rapport avec les new medias et 
        /// les médias qui n'existes plus
        /// </summary>
        /// <param name="playlistPostModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("diff")]
        [ValidateModel]
        [ProducesResponseType(typeof(IEnumerable<TvgMedia>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DiffAsync([FromBody]PlaylistPostModel playlistPostModel, CancellationToken cancellationToken = default)
        {
            //TODO: Déduire le provider from playlist (isXtream => xtreamProvider, m3u ou tvlist)
            // Load dynmaiquement all providers (singleton)
            PlaylistProvider<Playlist<TvgMedia>, TvgMedia> providerInstance = null;
            try
            {
                providerInstance = _providerFactory.CreateInstance(playlistPostModel.Url, playlistPostModel.Provider);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            try
            {
                //TODO: A virer apres la migration de l'auth
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.Equals(this.UserEmail), cancellationToken);
                if (user == null) return BadRequest($"User {this.UserEmail} not found");

                var playlist = await _dbContext.Playlist
                    .FirstOrDefaultAsync(x => x.SynkConfig.Url == playlistPostModel.Url, cancellationToken) ?? new Playlist
                {
                    UserId = user.Id,
                    Freindlyname = playlistPostModel.Freindlyname,
                    Status = playlistPostModel.Status,
                    SynkConfig = new SynkConfig { Url = playlistPostModel.Url, Provider = playlistPostModel.Provider }
                };

                using (providerInstance)
                {
                    var pl = await _playlistService
                        .DiffWithSourceAsync(() => playlist, providerInstance, cancellationToken: cancellationToken);
                    return Ok(pl);
                }
            }
            catch (HttpRequestException)
            {
                throw new BusinessException($"Playlist url {playlistPostModel.Url} not reachable");
            }
        }

        /// <summary>
        ///  Match tvg playlist by site packs defined on playlist
        /// </summary>
        /// <param name="id"></param>
        /// <param name="onlyNotMatched"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("matchfiltred/{id}")]
        [ValidateModel]
        [ProducesResponseType(typeof(PlaylistModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MatchFiltredByTvgSitesAsync([FromRoute] string id, [FromQuery] bool onlyNotMatched = true,
            CancellationToken cancellationToken = default)
        {
            var idGuid = GetInternalPlaylistId(id);

            var playlistEntity = await _dbContext.Playlist.FirstOrDefaultAsync(x => x.UniqueId == idGuid, cancellationToken);
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
                       media.MediaGroup = new MediaGroup(matched.Country);
                       if (media.Tvg == null)
                       {
                           media.Tvg = new Tvg { Name = matched.Channel_name, TvgIdentify = matched.Id, TvgSiteSource = matched.Site, Id = matched.Xmltv_id };
                       }
                       else
                       {
                           media.Tvg.Name = matched.Channel_name;
                           media.Tvg.Id = matched.Xmltv_id;
                           media.Tvg.TvgIdentify = matched.Id;
                           media.Tvg.TvgSiteSource = matched.Site;
                           if (media.Tvg.TvgSource == null)
                               media.Tvg.TvgSource = new TvgSource();
                           media.Tvg.TvgSource.Site = matched.Site;
                           media.Tvg.TvgSource.Country = matched.Country;
                           media.Tvg.TvgSource.Code = matched.Site_id;
                       }
                   }
               });

            return Ok(PlaylistModel.ToModel(playlistEntity, Url));
        }

        /// <summary>
        ///  Match playlist tvg (site pack directement)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="onlyNotMatched"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("matchtvg/{id}")]
        [ValidateModel]
        [ProducesResponseType(typeof(PlaylistModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MatchTvgAsync([FromRoute] string id, [FromQuery] bool onlyNotMatched = true,
            CancellationToken cancellationToken = default)
        {
            var idGuid = GetInternalPlaylistId(id);

            var playlistEntity = await _dbContext.Playlist.FirstOrDefaultAsync(x => x.UniqueId == idGuid, cancellationToken);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            playlistEntity.TvgMedias
                .Where(m => m.MediaType == MediaType.LiveTv && (!onlyNotMatched || m.Tvg == null || string.IsNullOrEmpty(m.Tvg.Id)))
                .AsParallel()
                .WithCancellation(cancellationToken)
                .ForAll(media =>
                  {
                      var matched = _sitePackService.MatchMediaNameAndBySiteAsync(media.DisplayName, media.Tvg?.TvgSource.Site, cancellationToken).GetAwaiter().GetResult();
                      if (matched != null)
                      {
                          media.Tvg.Id = matched.Xmltv_id;
                          media.Tvg.Name = matched.Channel_name;
                          media.Tvg.TvgIdentify = matched.Id;
                          media.Tvg.TvgSiteSource = matched.Site;
                      }
                  });

            //Matching movies
            var list = playlistEntity.TvgMedias.Where(m => m.MediaType == MediaType.Video).ToList();
            if (onlyNotMatched)
                list = list.Where(x => x.Tvg == null || string.IsNullOrEmpty(x.Tvg.Logo)).ToList();

            list
                .Where(x => x.Tvg?.TvgSource != null)
                .AsParallel()
                .WithCancellation(cancellationToken)
                .ForAll(media =>
                {
                    var matched = _mediaScraper.SearchAsync(media.DisplayName, _globalOptions.TmdbAPI, _globalOptions.TmdbPosterBaseUrl, cancellationToken).GetAwaiter().GetResult();
                    if (matched != null)
                    {
                        media.Tvg.Logo = matched.FirstOrDefault()?.PosterPath;
                    }
                });

            return Ok(PlaylistModel.ToModel(playlistEntity, Url));
        }

        /// <summary>
        /// Match videos
        /// </summary>
        /// <param name="id"></param>
        /// <param name="onlyNotMatched"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("matchvideos/{id}")]
        [ValidateModel]
        [ProducesResponseType(typeof(PlaylistModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MatchVideosByPlaylistAsync([FromRoute] string id, [FromQuery] bool onlyNotMatched = true,
            CancellationToken cancellationToken = default)
        {
            var idGuid = GetInternalPlaylistId(id);

            var playlistEntity = await _dbContext.Playlist.FirstOrDefaultAsync(x => x.UniqueId == idGuid, cancellationToken);
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
                        media.Tvg.Logo = matched.FirstOrDefault()?.PosterPath;
                    }
                });

            return Ok(PlaylistModel.ToModel(playlistEntity, Url));
        }

        /// <summary>
        /// Media info for video
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("matchvideo/{name}")]
        [ValidateModel]
        [ProducesResponseType(typeof(MediaInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MatchVideoAsync([FromRoute] string name, CancellationToken cancellationToken = default)
        {
            var matched = await _mediaScraper.SearchAsync(name, _globalOptions.TmdbAPI, _globalOptions.TmdbPosterBaseUrl, cancellationToken);

            if (matched != null)
            {
                return Ok(matched.FirstOrDefault());
            }

            return NotFound();
        }

        [HttpPost]
        [Route("matchvideos")]
        [ValidateModel]
        [ProducesResponseType(typeof(IEnumerable<TvgMedia>), StatusCodes.Status200OK)]
        public IActionResult MatchVideos([FromBody]List<TvgMedia> tvgmedias, CancellationToken cancellationToken = default)
        {
            tvgmedias.AsParallel().WithCancellation(cancellationToken).ForAll(media =>
           {
               var matched = _mediaScraper.SearchAsync(media.DisplayName, _globalOptions.TmdbAPI, _globalOptions.TmdbPosterBaseUrl, cancellationToken).GetAwaiter().GetResult();

               if (matched?.Any() == true)
               {
                   media.Tvg.Logo = matched.FirstOrDefault()?.PosterPath;
               }
           });
            return Ok(tvgmedias);
        }

        /// <summary>
        ///  Match playlist tvg (site pack directement)
        /// </summary>
        /// <param name="media"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("matchtvg/media")]
        [ValidateModel]
        [ProducesResponseType(typeof(SitePackChannel), StatusCodes.Status200OK)]
        public async Task<IActionResult> MatchTvgByMedia([FromBody] TvgMedia media, CancellationToken cancellationToken = default)
        {
            var sitePack = await _sitePackService.MatchMediaNameAndBySiteAsync(media.DisplayName, media.Tvg.TvgSource.Site, cancellationToken);
            return Ok(sitePack);
        }

        /// <summary>
        /// Get playlist file
        /// </summary>
        /// <param name="publicId"></param>
        /// <param name="provider"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [ResponseCache(CacheProfileName = "Default")]
        [AllowAnonymous]
        [HttpGet("files/{publicId:required}", Name = nameof(GetFile))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFile(string publicId, [FromQuery] string provider = "m3u", CancellationToken cancellationToken = default)
        {
            var idGuid = GetInternalPlaylistId(publicId);

            var playlist = await _dbContext.Playlist.FirstOrDefaultAsync(x => x.UniqueId == idGuid, cancellationToken);

            if (playlist == null)
                return NotFound(publicId);

            PlaylistProvider<Playlist<TvgMedia>, TvgMedia> sourceProvider = null;
            try
            {
                sourceProvider = _providerFactory.CreateInstance(playlist.SynkConfig.Uri, provider);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            using (sourceProvider)
            using (var pl = new Playlist<TvgMedia>(sourceProvider))
            using (var sourcePl = new Playlist<TvgMedia>(playlist.TvgMedias.Where(x => x.Enabled && !x.MediaGroup.Disabled)))
            {
                return await pl.PushAsync(sourcePl, cancellationToken).ContinueWith(t =>
                 {
                     return File(sourceProvider.PlaylistStream.ToArray(), "text/plain");
                 });
            }
        }

        #region Import

        /// <summary>
        /// Add new Upload playlist from stream
        /// </summary>
        /// <param name="playlistName"></param>
        /// <param name="playlistUrl">if playlist not null the file param will ignored</param>
        /// <param name="provider"></param>
        /// <param name="file"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("create/{provider}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Import(string playlistName, string playlistUrl, string provider, IFormFile file, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(playlistName))
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (string.IsNullOrEmpty(playlistName))
            {
                playlistName = file.FileName.Replace(Path.GetExtension(file.FileName), string.Empty); //TODO : catch all ArgumentException et les passer en BadRequest
            }
            //Vérifier si la playlist existe bien avant 

            PlaylistProvider<Playlist<TvgMedia>, TvgMedia> providerInstance = null;
            try
            {
                providerInstance = _providerFactory.CreateInstance(playlistUrl, provider);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            var playlistStream = file.OpenReadStream();
            if (string.IsNullOrEmpty(playlistUrl))
            {
                //TODO : Save file and get Url
                throw new ApplicationException("Import playlist from file not supported yet !!");
            }

            using (providerInstance)
            {
                //TODO: A virer apres la migration de l'auth
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.Equals(this.UserEmail), cancellationToken);
                if (user == null) return BadRequest($"User {this.UserEmail} not found");

                var pl = await _playlistService.SynkPlaylistAsync(() => new Playlist
                {
                    UserId = user.Id,
                    Freindlyname = playlistName,
                    Status = PlaylistStatus.Enabled,
                    SynkConfig = null,
                    Tags = new Dictionary<string, string> { { PlaylistTags.ImportProvider, provider } }
                }, providerInstance, _xtreamService.IsXtreamPlaylist(playlistUrl), true, cancellationToken);

                ClearCache();

                var result = PlaylistModel.ToLightModel(pl, Url);
                return Created(result.PublicUrl, result);
            }
        }

        /// <summary>
        /// Add new Upload playlist from url
        /// </summary>
        /// <param name="playlistPostModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("create")]
        [ValidateModel]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> ImportFromUrl([FromBody]PlaylistPostModel playlistPostModel, CancellationToken cancellationToken)
        {
            //Vérifier si la playlist existe-elle avant 
            var stopwatch = Stopwatch.StartNew();

            PlaylistProvider<Playlist<TvgMedia>, TvgMedia> providerInstance = null;
            try
            {
                providerInstance = _providerFactory.CreateInstance(playlistPostModel.Url, playlistPostModel.Provider);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            using (providerInstance)
            {
                //TODO: A virer apres la migration de l'auth
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.Equals(this.UserEmail), cancellationToken);
                if (user == null) return BadRequest($"User {this.UserEmail} not found");

                var pl = await _playlistService.SynkPlaylistAsync(() => new Playlist
                {
                    UserId = user.Id,
                    Freindlyname = playlistPostModel.Freindlyname,
                    Status = PlaylistStatus.Enabled,
                    SynkConfig = new SynkConfig { Url = playlistPostModel.Url },
                    Tags = new Dictionary<string, string> { { PlaylistTags.ImportProvider, playlistPostModel.Provider } }
                }, providerInstance, _xtreamService.IsXtreamPlaylist(playlistPostModel.Url), cancellationToken: cancellationToken);

                ClearCache();

                stopwatch.Stop();
                _logger.LogInformation($"Elapsed time : {stopwatch.Elapsed.ToString("c")}");

                var model = PlaylistModel.ToLightModel(pl, Url);
                return Created(model.PublicUrl, model);
            }
        }

        #endregion

        [HttpGet("{id}/groups")]
        [ProducesResponseType(typeof(PlaylistModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGroupsAsync(string id, CancellationToken cancellationToken = default)
        {
            var idGuid = GetInternalPlaylistId(id);
            var playlist = await _dbContext.Playlist.FirstOrDefaultAsync(x => x.UniqueId == idGuid, cancellationToken);
            if (playlist == null)
                return NotFound(id);

            return Ok(playlist.TvgMedias.Select(x => x.MediaGroup).Distinct(GroupComparerByName.Factory));
        }

        [HttpGet("{id}/groups/children")]
        [ProducesResponseType(typeof(PlaylistModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetChildrenGroupsAsync(string id, string group, CancellationToken cancellationToken = default)
        {
            var idGuid = GetInternalPlaylistId(id);
            var playlist = await _dbContext.Playlist.FirstOrDefaultAsync(x => x.UniqueId == idGuid, cancellationToken);
            if (playlist == null)
                return NotFound(id);

            return Ok(playlist.TvgMedias
                .Where(x => x.MediaGroup.Name.Equals(group, StringComparison.InvariantCultureIgnoreCase)));
        }

        private void ClearCache()
        {
            if (_memoryCache.TryGetValue(UserCachePlaylistKey, out List<string> list))
            {
                foreach (var item in list)
                {
                    _memoryCache.Remove(item);
                }
                list.Remove(UserCachePlaylistKey);
            }
        }

    }
}
