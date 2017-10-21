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
using hfa.WebApi.Dal;
using hfa.WebApi.Common.Filters;
using hfa.WebApi.Models.Xmltv;
using System.Threading;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    //[ApiVersion("1.0")]
    //[Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/v1/[controller]")]
    public class XmltvController : BaseController
    {
        private const string IndexName = "sitepack-*";

        public XmltvController(IOptions<ApplicationConfigData> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {

        }

        [HttpPost]
        [Route("channels/_search")]
        public async Task<IActionResult> SearchAsync([FromBody]dynamic request, CancellationToken cancellationToken)
        {
            return await SearchAsync<SitePackChannel>(request.ToString(), IndexName, cancellationToken);
        }

        /// <summary>
        /// Récupérer la liste des chaines présentes dans Site.pack and Site.user
        /// Depuis ELasticsearch
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateModel]
        [Route("channels")]
        public async Task<IActionResult> ListChaines([FromBody] QueryListBaseModel query, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.SearchAsync<SitePackChannel>(rq => rq
                .Size(query.PageSize)
                .From(query.Skip)
                .Sort(x => GetSortDescriptor(x, query.SortDict))
                .Query(q => q.Match(m => m.Field(ff => ff.Site)
                                          .Query(query.SearchDict.LastOrDefault().Value)))

            , HttpContext.RequestAborted);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            //response.AssertElasticResponse();
            return new OkObjectResult(response.GetResultListModel());
        }

        /// <summary>
        /// WebGrab a list of channels
        /// Genetate a xml file config And run SSH webgrab on the server
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateModel]
        [Route("webgrab")]
        public async Task<IActionResult> Webgrab([FromBody] QueryListBaseModel query, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.SearchAsync<tvChannel>(rq => rq
                .Size(query.PageSize)
                .From(query.Skip)
                .Sort(x => GetSortDescriptor(x, query.SortDict))
                .Query(q => q.Match(m => m.Field(ff => ff.displayname)
                                          .Query(query.SearchDict.LastOrDefault().Value)))

            , HttpContext.RequestAborted);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);
            //response.AssertElasticResponse();
            return new OkObjectResult(response.GetResultListModel());
        }

        /// <summary>
        /// Get Xmltv by user (Principal) and xmltv tag
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var response = await _elasticConnectionClient.Client.SearchAsync<tvChannel>(rq => rq
                .From(0)
                .Size(1)
                .Index<tvChannel>()
                .Query(q => q.Ids(ids => ids.Name(nameof(id)).Values(id)))
            , HttpContext.RequestAborted);

            response.AssertElasticResponse();
            if (response.Documents.FirstOrDefault() == null)
                return NotFound();

            return new OkObjectResult(response.GetResultListModel());
        }
    }
}
