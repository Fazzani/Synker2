using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Services.Xtream;
using hfa.Synker.Services.Dal;
using Hfa.WebApi.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace hfa.WebApi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Authorize]
    public class XtreamController : BaseController
    {
        private IXtreamService _xtreamService;

        public XtreamController(IXtreamService xtreamService, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
          IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
          : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _xtreamService = xtreamService;
        }

        [HttpGet]
        [Route("allepg/playlist/{playlistId}")]
        public async Task<IActionResult> GetAllEpgAsync([FromRoute]string playlistId, CancellationToken cancellationToken)
        {
            var idGuid = GetInternalPlaylistId(playlistId);

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            var epgs = await _xtreamService.GetAllEpgAsync(playlistEntity.SynkConfig.Url, cancellationToken);
            return Ok(epgs);
        }

        [HttpGet]
        [Route("livecats/playlist/{playlistId}")]
        public async Task<IActionResult> GetLiveCategoriesAsync([FromRoute]string playlistId, CancellationToken cancellationToken)
        {
            var idGuid = GetInternalPlaylistId(playlistId);

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            var cats = await _xtreamService.GetLiveCategoriesAsync(playlistEntity.SynkConfig.Url, cancellationToken);
            return Ok(cats);
        }

        [HttpGet]
        [Route("livestreams/playlist/{playlistId}")]
        public async Task<IActionResult> GetLiveStreamsAsync([FromRoute]string playlistId, CancellationToken cancellationToken)
        {
            var idGuid = GetInternalPlaylistId(playlistId);

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            var cats = await _xtreamService.GetLiveStreamsAsync(playlistEntity.SynkConfig.Url, cancellationToken);
            return Ok(cats);
        }

        [HttpGet]
        [Route("livestreams/playlist/{playlistId}/{catId}")]
        public async Task<IActionResult> GetLiveStreamsByCategoriesAsync([FromRoute]string playlistId, [FromRoute]string catId, CancellationToken cancellationToken)
        {
            var idGuid = GetInternalPlaylistId(playlistId);

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            var cats = await _xtreamService.GetLiveStreamsByCategoriesAsync(playlistEntity.SynkConfig.Url, catId, cancellationToken);
            return Ok(cats);
        }

        [HttpGet]
        [Route("panel/playlist/{playlistId}")]
        public async Task<IActionResult> GetPanelAsync([FromRoute]string playlistId, CancellationToken cancellationToken)
        {
            var idGuid = GetInternalPlaylistId(playlistId);

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            var cats = await _xtreamService.GetPanelAsync(playlistEntity.SynkConfig.Url, cancellationToken);
            return Ok(cats);
        }

        [HttpGet]
        [Route("panel/playlist/{playlistId}/{streamId}")]
        public async Task<IActionResult> GetShortEpgForStreamAsync([FromRoute]string playlistId, string streamId, CancellationToken cancellationToken)
        {
            var idGuid = GetInternalPlaylistId(playlistId);

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            var cats = await _xtreamService.GetShortEpgForStreamAsync(playlistEntity.SynkConfig.Url, streamId, cancellationToken);
            return Ok(cats);
        }

        [HttpGet]
        [Route("infos/playlist/{playlistId}")]
        public async Task<IActionResult> GetUserAndServerInfoAsync([FromRoute]string playlistId, CancellationToken cancellationToken)
        {
            var idGuid = GetInternalPlaylistId(playlistId);

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            var cats = await _xtreamService.GetUserAndServerInfoAsync(playlistEntity.SynkConfig.Url, cancellationToken);
            return Ok(cats);
        }

        [HttpGet]
        [Route("vods/playlist/{playlistId}")]
        public async Task<IActionResult> GetVodStreamsAsync([FromRoute]string playlistId, CancellationToken cancellationToken)
        {
            var idGuid = GetInternalPlaylistId(playlistId);

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            var cats = await _xtreamService.GetVodStreamsAsync(playlistEntity.SynkConfig.Url, cancellationToken);
            return Ok(cats);
        }

        [HttpGet]
        [Route("xmltv/playlist/{playlistId}")]
        public async Task<IActionResult> GetXmltvAsync([FromRoute]string playlistId, CancellationToken cancellationToken)
        {
            var idGuid = GetInternalPlaylistId(playlistId);

            var playlistEntity = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            var cats = await _xtreamService.GetXmltvAsync(playlistEntity.SynkConfig.Url, cancellationToken);
            return Ok(cats);
        }
    }
}