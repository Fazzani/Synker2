namespace hfa.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Hfa.WebApi.Controllers;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Authorization;
    using hfa.Synker.Services.Dal;
    using hfa.Synker.Service.Services.Elastic;
    using hfa.Synker.Service.Elastic;
    using hfa.Synker.Service.Services;
    using System.Net;
    using System.Threading;
    using hfa.WebApi.Common;

    [Produces("application/json")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    [ApiController]

    public class MediaServerController : BaseController
    {
        private readonly MediaServerService _mediaServerService;
        private readonly IOptions<MediaServerOptions> _mediaServerOptions;

        public MediaServerController(IOptions<ElasticConfig> config,
            MediaServerService mediaServerService,
            IOptions<MediaServerOptions> mediaServerOptions,
           ILoggerFactory loggerFactory,
           IElasticConnectionClient elasticConnectionClient,
           SynkerDbContext context)
           : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _mediaServerService = mediaServerService;
            _mediaServerOptions = mediaServerOptions;
        }

        /// <summary>
        /// Get server informations
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("server")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetServerAsync(CancellationToken cancellationToken)
        {
            var response = await _mediaServerService.GetServerStatsAsync(cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Get streams
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("streams")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetStreamsAsync(CancellationToken cancellationToken)
        {
            var response = await _mediaServerService.GetServerStreamsAsync(cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Get media Server config
        /// </summary>
        /// <returns></returns>
        [HttpGet("config")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult GetConfig()
        {
            return Ok(_mediaServerOptions?.Value);
        }
    }
}


