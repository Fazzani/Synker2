using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlaylistBaseLibrary.Entities;
using Hfa.WebApi.Common;
using Hfa.WebApi.Models;
using hfa.SyncLibrary.Global;
using hfa.WebApi.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using hfa.WebApi.Common.Filters;
using System.Threading;
using hfa.Synker.Services.Dal;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Elastic;
using hfa.WebApi.Models.Xmltv;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class EpgController : BaseController
    {
        public EpgController(IOptions<ElasticConfig> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient, 
            SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {

        }

        [HttpPost]
        [Route("_search")]
        public async Task<IActionResult> Search([FromBody] dynamic request)
        {
            return await SearchAsync<tvChannel, TvChannelModel>(request.ToString(), HttpContext.RequestAborted);
        }

        [HttpPost]
        [ValidateModel]
        [Route("search")]
        public async Task<IActionResult> SearchAsync([FromBody] QueryListBaseModel query, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.Value.SearchAsync<tvChannel>(rq => rq
                .Size(query.PageSize)
                .From(query.Skip)
                .Sort(x => GetSortDescriptor(x, query.SortDict))
                .Query(q => q.Match(m => m.Field(ff => ff.displayname)
                                          .Query(query.SearchDict.LastOrDefault().Value)))

            , cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);
            //response.AssertElasticResponse();
            return new OkObjectResult(response.GetResultListModel<tvChannel, TvChannelModel>());
        }

        [ValidateModel]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.Value.SearchAsync<tvChannel>(rq => rq
                .From(0)
                .Size(1)
                .Index<tvChannel>()
                .Query(q => q.Ids(ids => ids.Name(nameof(id)).Values(id)))
            , cancellationToken);

            response.AssertElasticResponse();
            if (response.Documents.FirstOrDefault() == null)
                return NotFound();

            return new OkObjectResult(response.GetResultListModel<tvChannel, TvChannelModel>());
        }
    }
}
