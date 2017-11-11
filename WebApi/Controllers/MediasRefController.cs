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
using hfa.Synker.Service.Services.Xmltv;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Entities.MediasRef;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class MediasRefController : BaseController
    {
        private const int ElasticMaxResult = 200000;

        public MediasRefController(IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
            IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
        }

        [HttpPost]
        [Route("_search")]
        public async Task<IActionResult> SearchAsync([FromBody]dynamic request, CancellationToken cancellationToken)
        {
            return await SearchAsync<MediaRef>(request.ToString(), nameof(MediaRef), cancellationToken);
        }

        [HttpPost]
        [Route("synk")]
        public async Task<IActionResult> SynkAsync(CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.SearchAsync<SitePackChannel>(s => s
               .Index(_elasticConfig.SitePackIndex)
               .Size(ElasticMaxResult)
               .From(0)
               .Aggregations(a => a
                   .Terms("unique", te => te
                       .Field(f => f.Channel_name.Suffix("keyword"))
                       .Order(TermsOrder.TermAscending)
                   )
               )
            , cancellationToken);

            var mediasRef = response.Documents.Select(x => new MediaRef(x.Channel_name, x.Site, x.Country, x.Xmltv_id, x.id));

            var descriptor = new BulkDescriptor();

            foreach (var mediaRef in mediasRef)
            {
                descriptor.Index<MediaRef>(op => op.Document(mediaRef));
            }

            var result = _elasticConnectionClient.Client.Bulk(descriptor);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.GetAsync(new DocumentPath<MediaRef>(id), null, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Source);
        }
    }
}
