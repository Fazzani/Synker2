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
using Nest;
using hfa.WebApi.Services;
using System.IO;
using PastebinAPI;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    //[ApiVersion("1.0")]
    //[Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/v1/[controller]")]
    public class XmltvController : BaseController
    {
        private readonly IPasteBinService _pasteBinService;

        public XmltvController(IPasteBinService pasteBinService, IOptions<ApplicationConfigData> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _pasteBinService = pasteBinService;
        }

        [HttpPost]
        [Route("channels/_search")]
        public async Task<IActionResult> SearchAsync([FromBody]dynamic request, CancellationToken cancellationToken)
        {
            return await SearchAsync<SitePackChannel>(request.ToString(), _config.SitePackIndex, cancellationToken);
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
        public async Task<IActionResult> Webgrab([FromBody]List<string> siteIds, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.MultiGetAsync(x => x.GetMany<SitePackChannel>(siteIds), cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            var webGragModel = Settings.New;
            webGragModel.Channel = response.Documents.Select(x => x.Source).Cast<SitePackChannel>().ToList();
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
            using (var ms = new MemoryStream())
            {
                xmlSerializer.Serialize(ms, webGragModel);
                ms.Position = 0;
                var sr = new StreamReader(ms);
                var myStr = sr.ReadToEnd();
                //Put WebGrabConfig to PasteBin
                var paste = await _pasteBinService.PushAsync(webGragModel.Filename, myStr, Expiration.OneDay, PastebinAPI.Language.XML);
                //Call ssh instantWebGrab batch script

                return new OkObjectResult(paste);
            }
        }

        /// <summary>
        /// Get channel Xmltv From SitePack by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("channels/{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.GetAsync(new DocumentPath<SitePackChannel>(id), null, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Source);
        }
    }
}
