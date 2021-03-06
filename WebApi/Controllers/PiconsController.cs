﻿
namespace Hfa.WebApi.Controllers
{
    using hfa.Synker.Service.Elastic;
    using hfa.Synker.Service.Entities.MediasRef;
    using hfa.Synker.Service.Services.Elastic;
    using hfa.Synker.Service.Services.Picons;
    using hfa.Synker.Service.Services.Scraper;
    using hfa.Synker.Services.Dal;
    using hfa.WebApi.Common;
    using hfa.WebApi.Models.Elastic;
    using hfa.WebApi.Models.Xmltv;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nest;
    using PlaylistManager.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using hfa.WebApi.Common.Auth;

    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(AuthenticationSchemes = Authentication.AuthSchemes)]
    public class PiconsController : BaseController
    {
        private readonly IPiconsService _piconsService;
        private readonly IMediaScraper _mediaScraper;
        private readonly GlobalOptions _globalOptions;

        public PiconsController(IPiconsService piconsService, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
            IElasticConnectionClient elasticConnectionClient, SynkerDbContext context, IMediaScraper mediaScraper, IOptions<GlobalOptions> globalOptions)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _piconsService = piconsService ?? throw new ArgumentNullException(nameof(piconsService));
            _mediaScraper = mediaScraper ?? throw new ArgumentNullException(nameof(mediaScraper));
            _globalOptions = globalOptions.Value ?? throw new ArgumentNullException(nameof(globalOptions));
        }

        [HttpPost]
        [Route("_search")]
        [Authorize(Policy = AuthorizePolicies.READER)]
        public async Task<IActionResult> SearchAsync([FromBody]dynamic request, CancellationToken cancellationToken = default)
        {
            return await SearchAsync<Picon, PiconModel>(request.ToString(), nameof(MediaRef).ToLowerInvariant(), cancellationToken);
        }

        [HttpPost]
        [Route("_searchstring")]
        [Authorize(Policy = AuthorizePolicies.READER)]
        public async Task<IActionResult> SearchStringAsync([FromBody]SimpleQueryElastic request,
            CancellationToken cancellationToken = default)
        {
            return await SearchQueryStringAsync<Picon, PiconModel>(request, cancellationToken);
        }

        [HttpGet("{id}")]
        [ResponseCache(CacheProfileName = "Long", VaryByQueryKeys = new string[] { "id" })]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [Authorize(Policy = AuthorizePolicies.READER)]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken = default)
        {
            var response = await _elasticConnectionClient.Client.Value.GetAsync(new DocumentPath<Picon>(id), null, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Source);
        }

        /// <summary>
        /// Synchronize Picons index from Github repository
        /// </summary>
        /// <param name="reset">Revove the indew before indexing picons</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("synk")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [Authorize(Policy = AuthorizePolicies.FULLACCESS)]
        public async Task<IActionResult> Synk([FromQuery] bool reset, CancellationToken cancellationToken = default)
        {
            var picons = await _piconsService.GetPiconsFromGithubRepoAsync(new SynkPiconConfig(), cancellationToken);
            var elasticResponse = await _piconsService.SynkAsync(picons, reset, cancellationToken);

            if (!elasticResponse.IsValid)
                return BadRequest(elasticResponse.DebugInformation);

            return new OkObjectResult(elasticResponse.Items);
        }

        /// <summary>
        /// Match tvgmedia names with picons
        /// </summary>
        /// <param name="tvgmedias"></param>
        /// <param name="distance"></param>
        /// <param name="shouldMatchChannelNumber"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("match")]
        [Authorize(Policy = AuthorizePolicies.FULLACCESS)]
        public IActionResult Match([FromBody]List<TvgMedia> tvgmedias,
            [FromQuery] int distance = 90,
            [FromQuery] bool shouldMatchChannelNumber = true,
            CancellationToken cancellationToken = default)
        {
            tvgmedias.AsParallel().WithCancellation(cancellationToken).ForAll(m =>
                {
                    if (m.Tvg == null)
                        m.Tvg = new Tvg();

                    if (m.MediaType == MediaType.Video)
                    {
                        var matched = _mediaScraper.SearchAsync(m.DisplayName, _globalOptions.TmdbAPI, _globalOptions.TmdbPosterBaseUrl, cancellationToken).GetAwaiter().GetResult();
                        if (matched != null)
                        {
                            m.Tvg.Logo = matched.FirstOrDefault().PosterPath;
                        }
                    }
                    else
                    {
                        var picons = _piconsService.MatchAsync(m.DisplayName, shouldMatchChannelNumber ? m.GetChannelNumber() : null, distance, cancellationToken).GetAwaiter().GetResult();
                        m.Tvg.Logo = picons.FirstOrDefault()?.RawUrl;
                    }
                });

            return Ok(tvgmedias);
        }

        /// <summary>
        /// Match mediaName and mediaNumber by picon
        /// </summary>
        /// <param name="mediaName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("match/{mediaName}")]
        [Authorize(Policy = AuthorizePolicies.FULLACCESS)]
        public async Task<IActionResult> Match([FromRoute]string mediaName, CancellationToken cancellationToken = default)
        {
            var picons = await _piconsService.MatchAsync(mediaName, Media.GetChannelNumber(mediaName), 90, cancellationToken);
            return Ok(picons.FirstOrDefault()?.RawUrl);
        }
    }
}
