using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlaylistManager.Entities;
using Hfa.WebApi.Common;
using System.Threading;
using Hfa.WebApi.Models;
using hfa.SyncLibrary.Global;
using Nest;
using Hfa.WebApi.Common.ActionsFilters;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Cors;
using hfa.WebApi.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    //[ApiVersion("1.0")]
    [Route("api/v1/[controller]")]
    public class TvgMediaController : BaseController
    {
        public TvgMediaController(IOptions<ApplicationConfigData> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient)
            : base(config, loggerFactory, elasticConnectionClient)
        {

        }

        [HttpPost]
        [Route("_search")]
        public async Task<IActionResult> SearchAsync([FromBody]dynamic request)
        {
            return await SearchAsync<TvgMedia>(request.ToString());
        }

        // [ElasticResult]
        [HttpPost]
        [Route("search")]
        public async Task<IActionResult> SearchAsync([FromBody] QueryListBaseModel query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _elasticConnectionClient.Client.SearchAsync<TvgMedia>(rq => rq
                .Size(query.PageSize)
                .From(query.Skip)
                .Sort(x => GetSortDescriptor(x, query.SortDict))

                .Query(q => q.Match(m => m.Field(ff => ff.Name)
                                          .Query(query.SearchDict.LastOrDefault().Value)))

            , cancelToken.Token);

            cancelToken.Token.ThrowIfCancellationRequested();

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);
            //response.AssertElasticResponse();
            return new OkObjectResult(response.GetResultListModel());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _elasticConnectionClient.Client.SearchAsync<TvgMedia>(rq => rq
                .From(0)
                .Size(1)
                .Index<TvgMedia>()
                .Query(q => q.Ids(ids => ids.Name(nameof(id)).Values(id)))
            , cancelToken.Token);

            cancelToken.Token.ThrowIfCancellationRequested();

            response.AssertElasticResponse();
            if (response.Documents.FirstOrDefault() == null)
                return NotFound();

            return new OkObjectResult(response.GetResultListModel());
        }

        [HttpPost]
        public IActionResult Post([FromBody]TvgMedia value)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]TvgMedia value)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return NoContent();
        }
    }
}
