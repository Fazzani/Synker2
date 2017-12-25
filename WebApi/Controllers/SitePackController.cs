﻿using System;
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

        public SitePackController(IMemoryCache memoryCache,  ISitePackService sitePackService, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
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

        //[HttpPost]
        //[Route("synk")]
        //public async Task<IActionResult> SynkAsync(CancellationToken cancellationToken)
        //{
        //    var result = await _sitePackService.SynkAsync(cancellationToken);

        //    if (!result.IsValid)
        //        return BadRequest(result.DebugInformation);

        //    return new OkObjectResult(result.Items);
        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.GetAsync(new DocumentPath<SitePackChannel>(id), null, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Source);
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
    }
}