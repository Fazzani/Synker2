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
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class SitePackController : BaseController
    {
        IMemoryCache _memoryCache;
        private ISitePackService _sitePackService;

        public SitePackController(IMemoryCache memoryCache, ISitePackService sitePackService, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
            IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _memoryCache = memoryCache;
            _sitePackService = sitePackService;
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
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.GetAsync(new DocumentPath<SitePackChannel>(id), null, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Source);
        }

        [HttpPost]
        [ValidateModel]
        [Route("save")]
        public async Task<IActionResult> Save([FromBody]List<SitePackChannel> sitepacks, CancellationToken cancellationToken)
        {
            sitepacks.ForEach(x =>
            {
                if (x.DisplayNames == null)
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
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
            var sitePacks = await _memoryCache.GetOrCreateAsync($"{CacheKeys.SitePacksKey}-{filter}", async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(12);
                var response = await _sitePackService.ListSitePackAsync(filter, cancellationToken: cancellationToken);
                return response.Select(x => new { x.Site, x.Country }).Distinct();
            });
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

    }
}
