using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using Nest;
using hfa.Synker.Services.Dal;
using Microsoft.AspNetCore.Authorization;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Entities.MediasRef;
using hfa.WebApi.Models;
using hfa.Synker.Service.Services.MediaRefs;
using hfa.SyncLibrary.Global;
using Hfa.WebApi.Common;
using hfa.WebApi.Common.Filters;
using Microsoft.Extensions.Caching.Memory;
using hfa.WebApi.Models.Elastic;
using Hfa.WebApi.Commmon;
using hfa.Synker.Service.Services;
using hfa.Synker.Service.Services.Xmltv;
using System.IO;
using PastebinAPI;
using hfa.WebApi.Services;
using hfa.Synker.Service.Entities.Auth;
using hfa.synker.entities;
using Microsoft.EntityFrameworkCore;
using System.Net;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class SitePackController : BaseController
    {
        const string PREFIX_WEBGRAB_FILENAME = "WebGrab";
        const string URL_SITEPACK_PREFIX = "https://raw.githubusercontent.com/SilentButeo2/webgrabplus-siteinipack/master/siteini.pack/";
        const string Docker_WEBGRABBER_IMAGE_NAME = "synker/webgraboneshoturl:latest";
        private readonly IMemoryCache _memoryCache;
        private readonly ISitePackService _sitePackService;
        private readonly IPasteBinService _pasteBinService;

        public SitePackController(IMemoryCache memoryCache, ISitePackService sitePackService, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
            IElasticConnectionClient elasticConnectionClient, SynkerDbContext context, IPasteBinService pasteBinService)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _sitePackService = sitePackService ?? throw new ArgumentNullException(nameof(sitePackService));
            _pasteBinService = pasteBinService ?? throw new ArgumentNullException(nameof(pasteBinService));
        }

        [HttpPost]
        [Route("_search")]
        public async Task<IActionResult> SearchAsync([FromBody]dynamic request, CancellationToken cancellationToken)
        {
            return await SearchAsync<SitePackChannel, SitePackModel>(request.ToString(), nameof(SitePackChannel).ToLowerInvariant(), cancellationToken);
        }

        [HttpPost]
        [Route("_searchstring")]
        public async Task<IActionResult> SearchStringAsync([FromBody]SimpleQueryElastic request, CancellationToken cancellationToken)
        {
            return await SearchQueryStringAsync<SitePackChannel, SitePackModel>(request, cancellationToken);
        }

        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.Value.GetAsync(new DocumentPath<SitePackChannel>(id), null, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Source);
        }

        [HttpPost]
        [ValidateModel]
        [Route("save")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Save([FromBody]List<SitePackChannel> sitepacks, CancellationToken cancellationToken)
        {
            sitepacks.ForEach(x =>
            {
                if (x.DisplayNames == null || !x.DisplayNames.Any())
                    x.DisplayNames = new List<string> { x.Channel_name };
                x.DisplayNames = x.DisplayNames.Distinct().ToList();
            });

            var response = await _sitePackService.SaveAsync(sitepacks, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Items);
        }

        [HttpPut("{id}")]
        [ValidateModel]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Put(string id, [FromBody]SitePackChannel value, CancellationToken cancellationToken)
        {
            var response = await _sitePackService.SaveAsync(new List<SitePackChannel> { value }, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Items);
        }

        [HttpPost(nameof(DeleteMany))]
        public async Task<IActionResult> DeleteMany([FromBody] string[] ids, CancellationToken cancellationToken)
        {
            var response = await _sitePackService.DeleteManyAsync(ids, cancellationToken);
            return new OkObjectResult(response);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery]string id, CancellationToken cancellationToken)
        {
            var response = await _sitePackService.DeleteManyAsync(new[] { id }, cancellationToken);
            return new OkObjectResult(response);
        }

        [ResponseCache(CacheProfileName = "Long")]
        [HttpGet]
        [Route("tvgsites")]
        public async Task<IActionResult> TvgSitesAsync(CancellationToken cancellationToken)
        {
            var tvgSites = await _memoryCache.GetOrCreateAsync(CacheKeys.SitesKey, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(12);
                var response = await _sitePackService.ListSitePackAsync(string.Empty, _elasticConfig.MaxResultWindow, cancellationToken);
                return response;
            });
            return Ok(tvgSites);
        }

        [ResponseCache(CacheProfileName = "Long")]
        [HttpGet]
        [Route("sitepacks")]
        public async Task<IActionResult> SitePacksAsync([FromQuery]string filter, CancellationToken cancellationToken)
        {
            var response = await _sitePackService.ListSitePackAsync(filter, cancellationToken: cancellationToken);
            var sitePacks = response.Select(x => new { x.Site, x.Country });
            return new OkObjectResult(sitePacks);
        }

        /// <summary>
        /// match tvg by media name and country
        /// </summary>
        /// <param name="mediaName"></param>
        /// <param name="country"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("matchtvg/name/{mediaName}")]
        public async Task<IActionResult> MatchTvgByMediaName([FromRoute] string mediaName, [FromQuery] string country, CancellationToken cancellationToken = default(CancellationToken))
        {
            var sitePack = await _sitePackService.MatchMediaNameAndBySiteAsync(mediaName, country, cancellationToken);
            return Ok(sitePack);
        }

        /// <summary>
        /// Countries list
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [ResponseCache(CacheProfileName = "Long", VaryByQueryKeys = new string[] { "filter" })]
        [HttpGet]
        [Route("countries")]
        public async Task<IActionResult> CountriesAsync([FromQuery]string filter, CancellationToken cancellationToken)
        {
            var cultures = await _memoryCache.GetOrCreateAsync(CacheKeys.CulturesKey, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(12);
                var response = await _sitePackService.ListCountriesAsync(filter, cancellationToken);
                return response;
            });
            return Ok(cultures);
        }

        [HttpPost("countries")]
        public async Task<IActionResult> SyncCountryAsync(CancellationToken cancellationToken)
        {
            var response = await _sitePackService.SyncCountrySitePackAsync(cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Liste des Sitepacks used in all playlists
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("used")]
        public async Task<IActionResult> GetAllFromPlaylistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<string> result = await GetSitepacksToWebGrab(cancellationToken);
            return Ok(result);
        }

        private async Task<IEnumerable<string>> GetSitepacksToWebGrab(CancellationToken cancellationToken)
        {
            var sitePack = await _sitePackService.GetAllFromPlaylistsAsync(cancellationToken);

            var result = sitePack.Select(x =>
            {
                var fragment = x.Split("/");
                return $"{URL_SITEPACK_PREFIX}{string.Join("/", fragment[fragment.Length - 2], fragment[fragment.Length - 1])}";
            });
            return result;
        }

        /// <summary>
        /// Pousser sur pastebin un nouveau Webgrab.config.xml selon un sitepack.channel.xml
        /// </summary>
        /// <param name="sitePackUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("webgrabconfig")]
        [ValidateModel]
        public async Task<IActionResult> WebgrabConfigBySitePackAsync([FromBody]SimpleModelPost sitePackUrl, CancellationToken cancellationToken = default(CancellationToken))
        {
            var paste = await SynkSitePackToWebgrabAsync(sitePackUrl.Value, cancellationToken);
            return new OkObjectResult(paste);
        }

        /// <summary>
        /// Récupérer la liste des sitepacks à Webgrabber et les pousser sur Pastebin
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("synk/webgrab")]
        [ValidateModel]
        public async Task<IActionResult> SynkAllSitePackToWebGrab(CancellationToken cancellationToken = default(CancellationToken))
        {
            _dbContext.WebGrabConfigDockers.RemoveRange(_dbContext.WebGrabConfigDockers);
            //TODO: Equilibrer la charge sur les différentes hosts
            var host = await _dbContext.Hosts.Where(h => h.Enabled).FirstOrDefaultAsync(cancellationToken);
            var i = 0;

            foreach (var sitePackUrl in await GetSitepacksToWebGrab(cancellationToken))
            {
                try
                {
                    var paste = await SynkSitePackToWebgrabAsync(sitePackUrl, cancellationToken);
                    await _dbContext.WebGrabConfigDockers.AddAsync(new WebGrabConfigDocker
                    {
                        Cron = $"0 {i++} * * *",
                        DockerImage = Docker_WEBGRABBER_IMAGE_NAME,
                        MountSourcePath = "/mnt/nfs/webgrab/xmltv",
                        RunnableHost = host,
                        WebgrabConfigUrl = paste.RawUrl
                    }, cancellationToken);

                    _logger.LogInformation($"Adding new cronned command to webgrab {sitePackUrl} from {nameof(WebgrabConfigBySitePackAsync)} by {UserId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok();
        }

        private async Task<Paste> SynkSitePackToWebgrabAsync(string sitePackUrl, CancellationToken cancellationToken)
        {
            var tab = sitePackUrl.Split("/");
            var fileName = $"{PREFIX_WEBGRAB_FILENAME}_{tab[tab.Length - 1]}";

            var webgrabContent = await _sitePackService.WebgrabConfigBySitePackAsync(sitePackUrl, fileName, cancellationToken);

            await _pasteBinService.DeleteAsync(fileName);
            return await _pasteBinService.PushAsync(fileName, webgrabContent, Expiration.OneDay, PastebinAPI.Language.XML, Visibility.Public);
        }
    }
}
