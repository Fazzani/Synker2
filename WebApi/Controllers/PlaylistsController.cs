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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    //[ApiVersion("1.0")]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class PlaylistsController : BaseController
    {
        private IPlaylistService _playlistService;

        public PlaylistsController(IOptions<ApplicationConfigData> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient,
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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var playlist = await _dbContext.FindAsync<Playlist>(id);
            if (playlist == null)
                return NotFound(id);

            return Ok(PlaylistModel.ToModel(playlist));
        }

        [HttpGet("{id:guid}")]
        public IActionResult Get(Guid id, CancellationToken cancellationToken)
        {
            var playlist =  _dbContext.Playlist.FirstOrDefault(x => x.UniqueId == id);
            if (playlist == null)
                return NotFound(id);

            return Ok(PlaylistModel.ToModel(playlist));
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
            playlistEntity.Cron = playlist.Cron;

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
        /// Download medias list from elastic
        /// </summary>
        /// <param name="filename">Output filename</param>
        /// <returns></returns>
        [HttpGet]
        [Route("download/{filename}")]
        public async Task<FileContentResult> Download(string filename, CancellationToken cancellationToken)
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
        /// Add new Upload playlist
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="playlistUrl">if playlist not null the file param will ignored</param>
        /// <param name="toType"></param>
        /// <param name="file"></param>
        /// <param name="providersOptions"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("create/{provider}")]
        public async Task<IActionResult> Import(string playlistName, string playlistUrl, string provider, IFormFile file, [FromServices] IOptions<List<PlaylistProviderOption>> providersOptions,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(playlistName))
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (string.IsNullOrEmpty(playlistName))
            {
                playlistName = file.FileName.Replace(Path.GetExtension(file.FileName), string.Empty); //TODO : catch all ArgumentException et les passer en BadRequest
            }
            //Vérifier si la playlist existe-elle avant 

            var optionsProvider = providersOptions.Value.FirstOrDefault(x => x.Name.Equals(provider, StringComparison.InvariantCultureIgnoreCase));
            if (optionsProvider == null)
                return BadRequest($"Not supported Provider : {provider}");

            var providerType = Type.GetType(optionsProvider.Type, false, true);
            if (providerType == null)
                return BadRequest($"Provider type not found : {provider}");

            var playlistStream = file.OpenReadStream();
            if (string.IsNullOrEmpty(playlistUrl))
            {
                //Download playlist from url
                using (var httpClient = new HttpClient())
                {
                    playlistStream = await httpClient.GetStreamAsync(playlistUrl);
                }
            }

            await SavePlaylist(playlistName, providerType, playlistStream, cancellationToken);

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
        [Route("create")]
        [ValidateModel]
        public async Task<IActionResult> ImportFromUrl(PlaylistPostModel playlistPostModel, [FromServices] IOptions<List<PlaylistProviderOption>> providersOptions,
            CancellationToken cancellationToken)
        {
            //Vérifier si la playlist existe-elle avant 

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

                await SavePlaylist(playlistPostModel.PlaylistName, providerType, playlistStream, cancellationToken);
            }

            return NoContent();
        }

        private async Task SavePlaylist(string playlistName, Type providerType, Stream playlistStream, CancellationToken cancellationToken)
        {
            var providerInstance = (FileProvider)Activator.CreateInstance(providerType, playlistStream);

            using (var playlist = new Playlist<TvgMedia>(providerInstance))
            {
                var sourceList = await playlist.PullAsync(cancellationToken);
                var content = JsonConvert.SerializeObject(sourceList.ToArray());
                var playlistEntity = new Playlist { UserId = UserId.Value, Freindlyname = playlistName, Content = content, Status = PlaylistStatus.Enabled };
                await _dbContext.Playlist.AddAsync(playlistEntity, cancellationToken);

                var res = await _dbContext.SaveChangesAsync(cancellationToken);
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
