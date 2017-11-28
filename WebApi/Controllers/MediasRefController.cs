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
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class MediasRefController : BaseController
    {
        IMediaRefService _mediaRefService;

        public MediasRefController(IMediaRefService mediaRefService, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
            IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _mediaRefService = mediaRefService;
        }

        [HttpPost]
        [Route("_search")]
        public async Task<IActionResult> SearchAsync([FromBody]dynamic request, CancellationToken cancellationToken)
        {
            return await SearchAsync<MediaRef, MediaRefModel>(request.ToString(), nameof(MediaRef).ToLowerInvariant(), cancellationToken);
        }

        [HttpPost]
        [Route("groups")]
        [ValidateModel]
        public async Task<IActionResult> GroupsAsync([FromBody]ElasticQueryAggrRequest query, CancellationToken cancellationToken)
        {
            ISearchResponse<MediaRef> response = await _mediaRefService.GroupsAsync(query?.Filter, query?.Size, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Aggs);
        }

        [HttpPost]
        [Route("synk")]
        public async Task<IActionResult> SynkAsync(CancellationToken cancellationToken)
        {
            var result = await _mediaRefService.SynkAsync(cancellationToken);

            if (!result.IsValid)
                return BadRequest(result.DebugInformation);

            return new OkObjectResult(result.Items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.GetAsync(new DocumentPath<MediaRef>(id), null, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Source);
        }

        [HttpGet("cultures/{filter}")]
        public async Task<IActionResult> Cultures(string filter, CancellationToken cancellationToken)
        {
            var cultures = await _mediaRefService.ListCulturesAsync(filter, cancellationToken);
            return Ok(cultures);
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save([FromBody]List<MediaRef> mediasRef, CancellationToken cancellationToken)
        {
            var response = await _mediaRefService.SaveAsync(mediasRef, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Items);
        }

        [HttpPost("merge")]
        public async Task<IActionResult> Merge(CancellationToken cancellationToken)
        {
            var response = await _mediaRefService.RemoveDuplicatedMediaRefAsync(cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Items);
        }

        [HttpPost(nameof(DeleteMany))]
        public async Task<IActionResult> DeleteMany([FromBody] string[] ids, CancellationToken cancellationToken)
        {
            var response = await _mediaRefService.DeleteManyAsync(ids, cancellationToken);
            return new OkObjectResult(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var response = await _mediaRefService.DeleteManyAsync(new string[] { id }, cancellationToken);
            return new OkObjectResult(response);
        }
    }
}
