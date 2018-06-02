using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using hfa.SyncLibrary.Global;
using hfa.WebApi.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using hfa.WebApi.Common.Filters;
using System.Threading;
using Nest;
using hfa.WebApi.Services;
using System.IO;
using PastebinAPI;
using System.Text;
using PlaylistBaseLibrary.Entities;
using hfa.Synker.Service.Entities.Auth;
using hfa.Synker.Services.Dal;
using Microsoft.AspNetCore.Authorization;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Services.Xmltv;
using hfa.Synker.Service.Elastic;
using hfa.WebApi.Models.Xmltv;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    [ApiController]
    public class XmltvController : BaseController
    {
        private readonly IPasteBinService _pasteBinService;

        public XmltvController(IPasteBinService pasteBinService, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory, 
            IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _pasteBinService = pasteBinService ?? throw new ArgumentNullException(nameof(pasteBinService));
        }

        [HttpPost]
        [Route("channels/_search")]
        public async Task<IActionResult> SearchAsync([FromBody]dynamic request, CancellationToken cancellationToken)
        {
            return await SearchAsync<SitePackChannel, SitePackChannelModel>(request.ToString(), _elasticConfig.SitePackIndex, cancellationToken);
        }

        /// <summary>
        /// WebGrab a list of channels
        /// Generate a xml file config
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateModel]
        [Route("webgrab")]
        public async Task<IActionResult> Webgrab([FromBody]List<string> siteIds, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.Value.MultiGetAsync(x => x.GetMany<SitePackChannel>(siteIds), cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            var webGragModel = Settings.New;
            webGragModel.Channel = response.Documents.Select(x => x.Source).Cast<SitePackChannel>().ToList();
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(Settings));

            using (var ms = new MemoryStream())
            {
                xmlSerializer.Serialize(ms, webGragModel);
                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    var myStr = sr.ReadToEnd();

                    //Put WebGrabConfig to PasteBin
                    var paste = await _pasteBinService.PushAsync(webGragModel.Filename, myStr, Expiration.OneDay, PastebinAPI.Language.XML);
                    //Add new command to Database
                    await _dbContext.Command.AddAsync(new Command
                    {
                        CommandText = $"cd /root/.wg++ && ls && ./instantWebGrab.sh {paste.RawUrl}",
                        UserId = UserId.Value,
                        Comments = $"Adding new command from {nameof(Webgrab)} by {UserId}{Environment.NewLine}"
                    });
                    return new OkObjectResult(paste);
                }
            }
        }

        /// <summary>
        /// Upload Xmltv from json file
        /// </summary>
        /// <param name="tv"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateModel]
        [Route("uploadjson")]
        public async Task<IActionResult> UploadFromJson([FromBody]tv tv, CancellationToken cancellationToken)
        {
            var progGroupedByDay = tv.programme
                .Distinct(new tvProgramme())
                .OrderByDescending(o => o.StartTime)
                .GroupBy(p => p.StartTime.Date.ToString("yyyy-MM-dd"));

            var errors = new List<object>();

            foreach (var prog in progGroupedByDay)
            {
                string indexName = $"xmltv-{prog.Key}";
                //Indexer les prog by day, chaque dans un index à part
                var responseBulk = await _elasticConnectionClient.Client.Value
                    .BulkAsync(x => x.Index(indexName)
                        .CreateMany(prog.ToList(),
                        (bd, q) => bd.Index(indexName).Id(q.Id))
                    , cancellationToken);

                responseBulk.AssertElasticResponse();

                if (!responseBulk.IsValid)
                {
                    if (responseBulk.ServerError != null)
                    {
                        errors.Add(responseBulk.ServerError.Error);
                        _logger.LogWarning(responseBulk.ServerError.Error.ToString());
                    }
                    if (responseBulk.Errors)
                    {
                        errors.Add(responseBulk.ItemsWithErrors.Select(x => x.Error));
                        _logger.LogWarning(responseBulk.ItemsWithErrors.Select(x => x.Error.Reason).Aggregate((x, x1) => $"{x}{Environment.NewLine}{x1}"));
                    }
                }
            }

            if (errors.Any())
                return BadRequest(errors);

            return Ok();
        }

        /// <summary>
        /// Get channel Xmltv From SitePack by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("channels/{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.Value.GetAsync(new DocumentPath<SitePackChannel>(id), null, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Source);
        }
    }
}
