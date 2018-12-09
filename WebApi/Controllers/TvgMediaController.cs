using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlaylistManager.Entities;
using Hfa.WebApi.Common;
using System.Threading;
using Hfa.WebApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using hfa.WebApi.Common.Filters;
using hfa.Synker.Services.Dal;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Elastic;
using hfa.WebApi.Models.TvgMedias;
using hfa.Synker.Service.Services;
using hfa.WebApi.Models;

namespace Hfa.WebApi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    [ApiController]
    public class TvgMediaController : BaseController
    {
        private readonly ISitePackService _sitePackService;

        public TvgMediaController(IOptions<ElasticConfig> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient,
            SynkerDbContext context, ISitePackService sitePackService)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _sitePackService = sitePackService ?? throw new ArgumentNullException(nameof(sitePackService));
        }

        [HttpPost]
        [Route("_search")]
        public async Task<IActionResult> SearchAsync([FromBody]dynamic request, CancellationToken cancellationToken)
        {
            return await SearchAsync<TvgMedia, TvgMediaModel>(request.ToString(), cancellationToken);
        }

        // [ElasticResult]
        [HttpPost]
        [ValidateModel]
        [Route("search")]
        public async Task<IActionResult> SearchAsync([FromBody] QueryListBaseModel query, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.Value.SearchAsync<TvgMedia>(rq => rq
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

            var response = await _elasticConnectionClient.Client.Value.SearchAsync<TvgMedia>(rq => rq
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

        /// <summary>
        ///  Match playlist tvg (site pack directement)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("matchtvg")]
        [ValidateModel]
        public async Task<IActionResult> MatchTvg([FromBody] MatchTvgPostModel matchTvgPostModel, CancellationToken cancellationToken = default)
        {
            var sitePack = await _sitePackService.MatchTvgAsync(matchTvgPostModel.MediaName, matchTvgPostModel.Country, matchTvgPostModel.TvgSites, matchTvgPostModel.MinScore, cancellationToken);
            return Ok(sitePack);
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
