using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlaylistManager.Entities;
using System.Threading;
using Hfa.WebApi.Models;
using hfa.SyncLibrary.Global;
using hfa.WebApi.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using hfa.PlaylistBaseLibrary.Providers;
using hfa.WebApi.Common.Filters;
using hfa.Synker.Services.Dal;
using hfa.Synker.Service.Entities.Playlists;
using hfa.Synker.Service.Services.Playlists;
using Newtonsoft.Json;
using hfa.WebApi.Models.Playlists;
using System.Net.Http;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Elastic;
using System.Diagnostics;
using System.Text;
using System.Net.Http.Headers;

namespace Hfa.WebApi.Controllers
{
    //[ApiVersion("1.0")]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class PlaylistsController : BaseController
    {
        private IPlaylistService _playlistService;

        public PlaylistsController(IOptions<ElasticConfig> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient,
            SynkerDbContext context, IPlaylistService playlistService)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _playlistService = playlistService;
        }

        /// <summary>
        /// List Messages
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateModel]
        [Route("search")]
        public IActionResult List([FromBody] QueryListBaseModel query, CancellationToken cancellationToken)
        {
            var response = _dbContext.Playlist
                .Where(x => x.UserId == UserId)
                .OrderByDescending(x => x.Id)
                .Select(PlaylistModel.ToModel)
                .GetPaged(query.PageNumber, query.Skip);

            return new OkObjectResult(response);
        }

        [HttpGet("{id:guid}")]
        public IActionResult Get(Guid id, CancellationToken cancellationToken)
        {
            var playlist = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == id);
            if (playlist == null)
                return NotFound(id);

            return Ok(PlaylistModel.ToModel(playlist));
        }

        [AllowAnonymous]
        [HttpGet("files/{id:required}", Name = nameof(GetFile))]
        public async Task<IActionResult> GetFile(string id, [FromServices] IOptions<List<PlaylistProviderOption>> providersOptions,
          [FromQuery] string provider = "m3u", CancellationToken cancellationToken = default(CancellationToken))
        {
            var idGuid = new Guid(Encoding.UTF8.DecodeBase64(id));

            var playlist = _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == idGuid);
            if (playlist == null)
                return NotFound(id);

            using (var ms = new MemoryStream())
            using (var sourceProvider = FileProvider.Create(provider, providersOptions.Value, ms))
            using (var pl = new Playlist<TvgMedia>(sourceProvider))
            using (var sourcePl = new Playlist<TvgMedia>(playlist.TvgMedias))
            {
                //var sourcePl = await sourceProvider.PullAsync(cancellationToken);
                ms.Seek(0, SeekOrigin.Begin);
                await pl.PushAsync(sourcePl, cancellationToken);
                return File(ms.GetBuffer(), "text/plain");
            }
        }

        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> Put(int id, [FromBody]PlaylistModel playlist, CancellationToken cancellationToken)
        {
            var playlistEntity = await _dbContext.FindAsync<Playlist>(id);
            if (playlistEntity == null)
                return NotFound(playlistEntity);

            playlistEntity.Status = playlist.Status;
            playlistEntity.Freindlyname = playlist.Freindlyname;
            playlistEntity.SynkConfig.Cron = playlist.Cron;
            playlistEntity.SynkConfig.Url = playlist.Url;
            playlistEntity.SynkConfig.SynkEpg = playlist.SynkEpg;
            playlistEntity.SynkConfig.SynkGroup = playlist.SynkGroup;
            playlistEntity.SynkConfig.SynkLogos = playlist.SynkLogos;

            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var playlist = await _dbContext.FindAsync<Playlist>(id);
            if (playlist == null)
                return NotFound(playlist);

            _dbContext.Playlist.Remove(playlist);

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Add new Upload playlist from url
        /// </summary>
        /// <param name="playlistPostModel"></param>
        /// <param name="providersOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("synk")]
        [ValidateModel]
        public async Task<IActionResult> Synk(PlaylistPostModel playlistPostModel, [FromServices] IOptions<List<PlaylistProviderOption>> providersOptions,
            CancellationToken cancellationToken)
        {
            //Vérifier si la playlist existe-elle avant 
            var stopwatch = Stopwatch.StartNew();
            var optionsProvider = providersOptions.Value.FirstOrDefault(x => x.Name.Equals(playlistPostModel.Provider, StringComparison.InvariantCultureIgnoreCase));
            if (optionsProvider == null)
                return BadRequest($"Not supported Provider : {playlistPostModel.Provider}");

            var providerType = Type.GetType(optionsProvider.Type, false, true);
            if (providerType == null)
                return BadRequest($"Provider type not found : {playlistPostModel.Provider}");

            //Download playlist from url
            using (var httpClient = new HttpClient())
            {
                var playlistStream = await httpClient.GetStreamAsync(playlistPostModel.PlaylistUrl);
                var providerInstance = (FileProvider)Activator.CreateInstance(providerType, playlistStream);

                var playlist = _dbContext.Playlist.FirstOrDefault(x => x.SynkConfig.Url == playlistPostModel.PlaylistUrl) ?? new Playlist
                {
                    UserId = UserId.Value,
                    Freindlyname = playlistPostModel.PlaylistName,
                    Status = PlaylistStatus.Enabled,
                    SynkConfig = new SynkConfig { Url = playlistPostModel.PlaylistUrl, Provider = playlistPostModel.Provider }
                };

                var pl = await _playlistService.SynkPlaylist(() => playlist, providerInstance, cancellationToken: cancellationToken);

                if (playlist.UniqueId == default(Guid))
                    await _dbContext.Playlist.AddAsync(pl);

                var res = await _dbContext.SaveChangesAsync(cancellationToken);

                stopwatch.Stop();
                _logger.LogInformation($"Elapsed time : {stopwatch.Elapsed.ToString("c")}");
                return CreatedAtRoute(nameof(GetFile), new { id = UTF8Encoding.UTF8.EncodeBase64(pl.UniqueId.ToString()) }, null);
            }
        }

        private async Task<Playlist> SavePlaylist(string playlistName, Type providerType, Stream playlistStream, string playlistUrl, CancellationToken cancellationToken)
        {
            var providerInstance = (FileProvider)Activator.CreateInstance(providerType, playlistStream);

            using (var playlist = new Playlist<TvgMedia>(providerInstance))
            {
                var sourceList = await playlist.PullAsync(cancellationToken);
                var content = JsonConvert.SerializeObject(sourceList.ToArray());

                var synkcfg = new SynkConfig { Url = playlistUrl };
                var playlistEntity = new Playlist
                {
                    UserId = UserId.Value,
                    Freindlyname = playlistName,
                    Content = content,
                    Status = PlaylistStatus.Enabled,
                    SynkConfig = synkcfg
                };
                await _dbContext.Playlist.AddAsync(playlistEntity, cancellationToken);

                var res = await _dbContext.SaveChangesAsync(cancellationToken);
                return playlistEntity;
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
        [HttpPost]
        [Route("export/{fromType}/{toType}")]
        public async Task<IActionResult> Export(string fromType, string toType, IFormFile file, [FromServices] IOptions<List<PlaylistProviderOption>> providersOptions,
            CancellationToken cancellationToken)
        {
            try
            {
                var sourceProvider = FileProvider.Create(fromType, providersOptions.Value, file.OpenReadStream());

                using (var stream = new MemoryStream())
                {
                    var targetProvider = FileProvider.Create(toType, providersOptions.Value, stream);

                    using (var sourcePlaylist = new Playlist<TvgMedia>(sourceProvider))
                    {
                        var sourceList = await sourcePlaylist.PullAsync(HttpContext.RequestAborted);
                        using (var targetPlaylist = new Playlist<TvgMedia>(targetProvider))
                        {
                            await targetPlaylist.PushAsync(sourcePlaylist, HttpContext.RequestAborted);
                            return File(stream.GetBuffer(), "text/plain", file.FileName);
                        }
                    }
                }
            }
            catch (InvalidFileProviderException ifileEx)
            {
                return BadRequest(ifileEx.Message);
            }
        }
    }
}
