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
using hfa.Synker.Service.Services.Picons;
using hfa.Synker.Service.Entities.MediasRef;
using hfa.WebApi.Models.Xmltv;

namespace Hfa.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class PiconsController : BaseController
    {
        private const int ElasticMaxResult = 200000;
        private IPiconsService _piconsService;

        public PiconsController(IPiconsService piconsService, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
            IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _piconsService = piconsService;
        }

        [HttpPost]
        [Route("_search")]
        public async Task<IActionResult> SearchAsync([FromBody]dynamic request, CancellationToken cancellationToken)
        {
            return await SearchAsync<Picon, PiconModel>(request.ToString(), nameof(MediaRef).ToLowerInvariant(), cancellationToken);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.GetAsync(new DocumentPath<Picon>(id), null, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Source);
        }

        [HttpPost("synk")]
        public async Task<IActionResult> Synk(CancellationToken cancellationToken)
        {
            var picons = await _piconsService.GetPiconsFromGithubRepoAsync(new SynkPiconConfig(), cancellationToken);
            var elasticResponse = await _piconsService.SynkAsync(picons, cancellationToken);

            if (!elasticResponse.IsValid)
                return BadRequest(elasticResponse.DebugInformation);

            return new OkObjectResult(elasticResponse.Items);
        }
    }
}
