using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlaylistBaseLibrary.Entities;
using Hfa.WebApi.Common;
using Hfa.WebApi.Models;
using hfa.SyncLibrary.Global;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    //[ApiVersion("1.0")]
    //[Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/v1/[controller]")]
    public class EpgController : BaseController
    {
        [HttpPost]
        [Route("_search")]
        public async Task<IActionResult> Search([FromBody] dynamic request)
        {
            return await SearchAsync<tvChannel>(request.ToString());
        }

        [HttpPost]
        [Route("search")]
        public async Task<IActionResult> SearchAsync([FromBody] QueryListBaseModel query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await ElasticConnectionClient.Client.SearchAsync<tvChannel>(rq => rq
                .Size(query.PageSize)
                .From(query.Skip)
                .Sort(x => GetSortDescriptor(x, query.SortDict))
                .Query(q => q.Match(m => m.Field(ff => ff.displayname)
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

            var response = await ElasticConnectionClient.Client.SearchAsync<tvChannel>(rq => rq
                .From(0)
                .Size(1)
                .Index<tvChannel>()
                .Query(q => q.Ids(ids => ids.Name(nameof(id)).Values(id)))
            , cancelToken.Token);

            cancelToken.Token.ThrowIfCancellationRequested();

            response.AssertElasticResponse();
            if (response.Documents.FirstOrDefault() == null)
                return NotFound();

            return new OkObjectResult(response.GetResultListModel());
        }
    }
}
