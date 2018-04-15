using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using Nest;
using hfa.Synker.Services.Dal;
using Microsoft.AspNetCore.Authorization;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Services.Picons;
using hfa.Synker.Service.Entities.MediasRef;
using hfa.WebApi.Models.Xmltv;
using hfa.WebApi.Models.Elastic;
using PlaylistManager.Entities;
using hfa.Synker.Service.Services.Scraper;
using hfa.WebApi.Common;

namespace Hfa.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
#if !DEBUG
    [Authorize]
#endif
    public class PiconsController : BaseController
    {
        private IPiconsService _piconsService;
        private IMediaScraper _mediaScraper;
        GlobalOptions _globalOptions;

        public PiconsController(IPiconsService piconsService, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
            IElasticConnectionClient elasticConnectionClient, SynkerDbContext context, IMediaScraper mediaScraper, IOptions<GlobalOptions> globalOptions)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _piconsService = piconsService;
            _mediaScraper = mediaScraper;
            _globalOptions = globalOptions.Value;
        }

        [HttpPost]
        [Route("_search")]
        public async Task<IActionResult> SearchAsync([FromBody]dynamic request, CancellationToken cancellationToken)
        {
            return await SearchAsync<Picon, PiconModel>(request.ToString(), nameof(MediaRef).ToLowerInvariant(), cancellationToken);
        }

        [HttpPost]
        [Route("_searchstring")]
        public async Task<IActionResult> SearchStringAsync([FromBody]SimpleQueryElastic request, CancellationToken cancellationToken)
        {
            return await SearchQueryStringAsync<Picon, PiconModel>(request, cancellationToken);
        }

        [HttpGet("{id}")]
        [ResponseCache(CacheProfileName = "Long", VaryByQueryKeys = new string[] { "id" })]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.Value.GetAsync(new DocumentPath<Picon>(id), null, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Source);
        }

        /// <summary>
        /// Synchronize Picons index from Github repository
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("synk")]
        public async Task<IActionResult> Synk(CancellationToken cancellationToken)
        {
            var picons = await _piconsService.GetPiconsFromGithubRepoAsync(new SynkPiconConfig(), cancellationToken);
            var elasticResponse = await _piconsService.SynkAsync(picons, cancellationToken);

            if (!elasticResponse.IsValid)
                return BadRequest(elasticResponse.DebugInformation);

            return new OkObjectResult(elasticResponse.Items);
        }

        /// <summary>
        /// Match tvgmedia names with picons
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("match")]
        public IActionResult Match([FromBody]List<TvgMedia> tvgmedias, [FromQuery] int distance = 90, [FromQuery] bool shouldMatchChannelNumber = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            tvgmedias.AsParallel().WithCancellation(cancellationToken).ForAll(m =>
                {
                    if (m.Tvg == null)
                        m.Tvg = new Tvg();

                    if (m.MediaType == MediaType.LiveTv || m.MediaType == MediaType.Radio)
                    {
                        var picons = _piconsService.MatchAsync(m.DisplayName, shouldMatchChannelNumber ? m.GetChannelNumber() : null, distance, cancellationToken).GetAwaiter().GetResult();
                        m.Tvg.Logo = picons.FirstOrDefault()?.RawUrl;
                    }
                    else if (m.MediaType == MediaType.Video)
                    {
                        var matched = _mediaScraper.SearchAsync(m.DisplayName, _globalOptions.TmdbAPI, _globalOptions.TmdbPosterBaseUrl, cancellationToken).GetAwaiter().GetResult();
                        if (matched != null)
                        {
                            m.Tvg.Logo = matched.FirstOrDefault().PosterPath;
                        }
                    }
                });

            return Ok(tvgmedias);
        }

        /// <summary>
        /// Match mediaName and mediaNumber by picon
        /// </summary>
        /// <param name="mediaName"></param>
        /// <param name="mediaNumber"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("match/{mediaName}")]
        public async Task<IActionResult> Match([FromRoute]string mediaName, CancellationToken cancellationToken)
        {
            var picons = await _piconsService.MatchAsync(mediaName, Media.GetChannelNumber(mediaName), 90, cancellationToken);
            return Ok(picons.FirstOrDefault()?.RawUrl);
        }
    }
}
