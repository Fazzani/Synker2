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
using hfa.WebApi.Common.Filters;
using Microsoft.Extensions.Caching.Memory;
using hfa.WebApi.Models.Elastic;
using Hfa.WebApi.Commmon;
using hfa.Synker.Service.Services;
using System.Net;
using Microsoft.AspNetCore.Http;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    [ApiVersion("1.0")]
    [ApiController]
    public class MediasRefController : BaseController
    {
        private readonly IMediaRefService _mediaRefService;
        private readonly IMemoryCache _memoryCache;
        private readonly ISitePackService _sitePackService;

        public MediasRefController(IMemoryCache memoryCache, IMediaRefService mediaRefService, ISitePackService sitePackService, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
            IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _mediaRefService = mediaRefService ?? throw new ArgumentNullException(nameof(mediaRefService));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _sitePackService = sitePackService ?? throw new ArgumentNullException(nameof(sitePackService));
        }

        [HttpPost]
        [Route("_search")]
        public async Task<IActionResult> SearchAsync([FromBody]dynamic request, CancellationToken cancellationToken)
        {
            return await SearchAsync<MediaRef, MediaRefModel>(request.ToString(), nameof(MediaRef).ToLowerInvariant(), cancellationToken);
        }

        [HttpPost]
        [Route("_searchstring")]
        public async Task<IActionResult> SearchStringAsync([FromBody]SimpleQueryElastic request, CancellationToken cancellationToken)
        {
            return await SearchQueryStringAsync<MediaRef, MediaRefModel>(request, cancellationToken);
        }

        [HttpPost]
        [Route("synk")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> SynkAsync(CancellationToken cancellationToken)
        {
            var result = await _mediaRefService.SynkAsync(cancellationToken);

            if (!result.IsValid)
                return BadRequest(result.DebugInformation);

            return new OkObjectResult(result.Items);
        }

        [HttpPost]
        [Route("synkpicons")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> SynkPiconsAsync(CancellationToken cancellationToken)
        {
            var result = await _mediaRefService.SynkPiconsAsync(cancellationToken);

            if (!result.IsValid)
                return BadRequest(result.DebugInformation);

            return new OkObjectResult(result.Items);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.Value.GetAsync(new DocumentPath<MediaRef>(id), null, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Source);
        }

        [HttpPost]
        [ValidateModel]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Save([FromBody]List<MediaRef> mediasRef, CancellationToken cancellationToken)
        {
            var response = await _mediaRefService.SaveAsync(mediasRef, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Items);
        }

        [HttpPost("merge")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Merge(CancellationToken cancellationToken)
        {
            var response = await _mediaRefService.RemoveDuplicatedMediaRefAsync(cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Items);
        }

        [HttpPost(nameof(DeleteMany))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteMany([FromBody] string[] ids, CancellationToken cancellationToken)
        {
            var response = await _mediaRefService.DeleteManyAsync(ids, cancellationToken);
            return new OkObjectResult(response);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var response = await _mediaRefService.DeleteManyAsync(new string[] { id }, cancellationToken);
            return new OkObjectResult(response);
        }

        [HttpPost]
        [Route("groups")]
        [ValidateModel]
        [ResponseCache(CacheProfileName = "Long")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GroupsAsync([FromBody]ElasticQueryAggrRequest query, CancellationToken cancellationToken)
        {
            ISearchResponse<MediaRef> response = await _mediaRefService.GroupsAsync(query?.Filter, query?.Size, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Aggs);
        }

        [ResponseCache(CacheProfileName = "Long", VaryByQueryKeys = new string[] { "filter" })]
        [HttpGet]
        [Route("cultures")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CulturesAsync([FromQuery]string filter, CancellationToken cancellationToken)
        {
            var cultures = await _memoryCache.GetOrCreateAsync(CacheKeys.CulturesKey, async entry =>
           {
               entry.SlidingExpiration = TimeSpan.FromHours(12);
               var response = await _mediaRefService.ListCulturesAsync(filter, cancellationToken);
               return response;
           });
            return Ok(cultures);
        }

        [ResponseCache(CacheProfileName = "Long")]
        [HttpGet]
        [Route("sitepacks")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
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
