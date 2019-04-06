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
    using System.Threading;
    using hfa.WebApi.Common;
    using hfa.WebApi.Models.MediaServer;
    using hfa.WebApi.Common.Filters;
    using Microsoft.AspNetCore.Http;
    using hfa.WebApi.Common.Auth;

    [Produces("application/json")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize(AuthenticationSchemes = Authentication.AuthSchemes, Policy = AuthorizePolicies.ADMIN)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStreamsAsync(CancellationToken cancellationToken)
        {
            var response = await _mediaServerService.GetServerStreamsAsync(cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Publish new live
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("live")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ValidateModel]
        public async Task<IActionResult> PostLiveAsync([FromBody]MediaServerLivePost model, CancellationToken cancellationToken)
        {
            var streamId = $"{UserEmail}_{Guid.NewGuid()}";
            var response = await _mediaServerService.PublishLiveAsync(model.Stream, streamId, cancellationToken);
            return Ok(new
            {
                response,
                FlvOutput = $"{_mediaServerOptions.Value.StreamBaseUrl}live/{streamId}.flv",
                HlsOutput = $"{_mediaServerOptions.Value.StreamBaseUrl}live/{streamId}.m3u8",
                DashOutput = $"{_mediaServerOptions.Value.StreamBaseUrl}live/{streamId}.mpd",
                RtmpOutput = $"{_mediaServerOptions.Value.StreamRtmpBaseUrl}live/{streamId}.mpd",
                WsOutput = $"{_mediaServerOptions.Value.StreamWebsocketBaseUrl}live/{streamId}.flv",
                streamId
            });
        }

        /// <summary>
        /// Stop live stream
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ValidateModel]
        public async Task<IActionResult> PostStopLiveAsync([FromBody]MediaServerLivePost model, CancellationToken cancellationToken)
        {
            var response = await _mediaServerService.StopLiveAsync(model.Stream, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Get media Server config
        /// </summary>
        /// <returns></returns>
        [HttpGet("config")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetConfig()
        {
            return Ok(_mediaServerOptions?.Value);
        }
    }
}


