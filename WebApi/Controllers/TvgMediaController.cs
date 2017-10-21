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
using System.IO;
using System.Text;
using PlaylistBaseLibrary.Providers;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using hfa.WebApi.Dal;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using hfa.PlaylistBaseLibrary.Providers;
using hfa.WebApi.Common.Filters;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    //[ApiVersion("1.0")]
    [Route("api/v1/[controller]")]
    public class TvgMediaController : BaseController
    {
        public TvgMediaController(IOptions<ApplicationConfigData> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {

        }

        [HttpPost]
        [Route("_search")]
        public async Task<IActionResult> SearchAsync([FromBody]dynamic request, CancellationToken cancellationToken)
        {
            return await SearchAsync<TvgMedia>(request.ToString(), cancellationToken);
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
            return new OkObjectResult(response.GetResultListModel());
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

            return new OkObjectResult(response.GetResultListModel());
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

        /// <summary>
        /// Download medias list from elastic
        /// </summary>
        /// <param name="filename">Output filename</param>
        /// <returns></returns>
        [HttpGet]
        [Route("download/{filename}")]
        public async Task<FileContentResult> Download(string filename)
        {
            var response = await _elasticConnectionClient.Client.SearchAsync<TvgMedia>(rq => rq
               .From(0)
               .Size(10000)
               .Index<TvgMedia>()
           , HttpContext.RequestAborted);

            response.AssertElasticResponse();

            using (var ms = new MemoryStream())
            {
                var provider = new M3uProvider(ms);
                await provider.PushAsync(new Playlist<TvgMedia>(response.Documents), HttpContext.RequestAborted);
                return File(ms.GetBuffer(), "application/octet-stream", filename);
            }
        }

        /// <summary>
        /// Export from provider to another
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        /// <param name="file"></param>
        /// <param name="providersOptions"></param>
        /// <returns></returns>
#if DEBUG
        [AllowAnonymous]
#endif
        [HttpPost]
        [Route("export/{fromType}/{toType}")]
        public async Task<IActionResult> Export(string fromType, string toType, IFormFile file, [FromServices] IOptions<List<PlaylistProviderOption>> providersOptions)
        {
            var sourceOption = providersOptions.Value.FirstOrDefault(x => x.Name.Equals(fromType, StringComparison.InvariantCultureIgnoreCase));
            if (sourceOption == null)
                return BadRequest($"Source Provider not found : {fromType}");

            var sourceProviderType = Type.GetType(sourceOption.Type, false, true);
            if (sourceProviderType == null)
                return BadRequest($"Source Provider not found : {fromType}");

            var targtOption = providersOptions.Value.FirstOrDefault(x => x.Name.Equals(toType, StringComparison.InvariantCultureIgnoreCase));
            if (targtOption == null)
                return BadRequest($"Source Provider not found : {toType}");

            var targetProviderType = Type.GetType(targtOption.Type, false, true);
            if (targetProviderType == null)
                return BadRequest($"Target Provider not found : {toType}");

            var sourceProvider = (FileProvider)Activator.CreateInstance(sourceProviderType, file.OpenReadStream());

            using (var stream = new MemoryStream())
            {
                var targetProvider = (FileProvider)Activator.CreateInstance(targetProviderType, stream);

                using (var sourcePlaylist = new Playlist<TvgMedia>(sourceProvider))
                {
                    var sourceList = await sourcePlaylist.PullAsync(HttpContext.RequestAborted);
                    using (var targetPlaylist = new Playlist<TvgMedia>(targetProvider))
                    {
                        await targetPlaylist.PushAsync(sourcePlaylist, HttpContext.RequestAborted);
                        return File(stream.GetBuffer(), "application/octet-stream", file.FileName);
                    }
                }
            }
        }

    }
}
