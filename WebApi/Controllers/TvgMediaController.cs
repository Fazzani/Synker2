﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlaylistManager.Entities;
using Hfa.WebApi.Common;
using System.Threading;
using Hfa.WebApi.Models;
using hfa.SyncLibrary.Global;
using hfa.WebApi.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using hfa.WebApi.Common.Filters;
using hfa.Synker.Services.Dal;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Elastic;
using hfa.WebApi.Models.TvgMedias;

namespace Hfa.WebApi.Controllers
{
    //[ApiVersion("1.0")]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class TvgMediaController : BaseController
    {
        public TvgMediaController(IOptions<ElasticConfig> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient, 
            SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {

        }

        [HttpPost]
        [Route("_search")]
        public async Task<IActionResult> SearchAsync([FromBody]dynamic request, CancellationToken cancellationToken)
        {
            return await SearchAsync<TvgMedia,TvgMediaModel>(request.ToString(), cancellationToken);
        }

        // [ElasticResult]
        [HttpPost]
        [ValidateModel]
        [Route("search")]
        public async Task<IActionResult> SearchAsync([FromBody] QueryListBaseModel query, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.SearchAsync<TvgMedia>(rq => rq
                .Size(query.PageSize)
                .From(query.Skip)
                .Sort(x => GetSortDescriptor(x, query.SortDict))
                .Query(q => q.Match(m => m.Field(ff => ff.Name)
                             .Query(query.SearchDict.LastOrDefault().Value)))

            , cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);
            //response.AssertElasticResponse();
            return new OkObjectResult(response.GetResultListModel<TvgMedia, TvgMediaModel>());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _elasticConnectionClient.Client.SearchAsync<TvgMedia>(rq => rq
                .From(0)
                .Size(1)
                .Index<TvgMedia>()
                .Query(q => q.Ids(ids => ids.Name(nameof(id)).Values(id)))
            , cancellationToken);

            response.AssertElasticResponse();
            if (response.Documents.FirstOrDefault() == null)
                return NotFound();

            return new OkObjectResult(response.GetResultListModel<TvgMedia, TvgMediaModel>());
        }

        [HttpPost]
        [ValidateModel]
        public IActionResult Post([FromBody]TvgMedia value)
        {
            return Ok();
        }

        [HttpPut("{id}")]
        [ValidateModel]
        public IActionResult Put(int id, [FromBody]TvgMedia value)
        {
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
