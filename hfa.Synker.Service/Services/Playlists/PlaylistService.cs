namespace hfa.Synker.Service.Services.Playlists
{
    using hfa.Brokers.Messages;
    using hfa.PlaylistBaseLibrary.ChannelHandlers;
    using hfa.PlaylistBaseLibrary.Providers;
    using hfa.Synker.Service.Elastic;
    using hfa.Synker.Service.Entities.Playlists;
    using hfa.Synker.Service.Services.Elastic;
    using hfa.Synker.Service.Services.TvgMediaHandlers;
    using hfa.Synker.Services.Dal;
    using hfa.Synkerk.Service.Services.TvgMediaHandlers;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using PlaylistManager.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class PlaylistService : IPlaylistService
    {
        private SynkerDbContext _dbcontext;
        private IElasticConnectionClient _elasticConnectionClient;
        private IContextTvgMediaHandler _contextHandler;
        private ILogger _logger;
        private IOptions<ElasticConfig> _elasticConfig;
        private ISitePackService _sitePackService;
        private IProviderFactory _providerFactory;

        public PlaylistService(SynkerDbContext synkerDbContext, IElasticConnectionClient elasticConnectionClient,
            IContextTvgMediaHandler contextHandler, ILoggerFactory loggerFactory, IOptions<ElasticConfig> elasticConfig, ISitePackService sitePackService, IProviderFactory providerFactory)
        {
            _dbcontext = synkerDbContext;
            _elasticConnectionClient = elasticConnectionClient;
            _contextHandler = contextHandler;
            _logger = loggerFactory.CreateLogger(nameof(PlaylistService));
            _elasticConfig = elasticConfig;
            _sitePackService = sitePackService;
            _providerFactory = providerFactory;
        }

        public async Task<IEnumerable<Playlist>> ListByUserAsync(int userId)
        {
            return (await _dbcontext.Users.FindAsync(userId))?.Playlists;
        }

        /// <summary>
        /// Synk playlist and match epg, logos and groups
        /// </summary>
        /// <param name="getPlaylist"></param>
        /// <param name="provider"></param>
        /// <param name="isXtreamPlaylist"></param>
        /// <param name="force"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Playlist> SynkPlaylistAsync(Func<Playlist> getPlaylist, PlaylistProvider<Playlist<TvgMedia>, TvgMedia> provider = default, bool isXtreamPlaylist = false, bool force = false,
            CancellationToken cancellationToken = default)
        {
            var playlistEntity = getPlaylist();
            if (playlistEntity == null)
                throw new ArgumentNullException($"Playlist not found");
            if (!playlistEntity.IsSynchronizable)
                throw new ApplicationException($"Playlist isn't synchronizable");

            try
            {
                provider = _providerFactory.CreateInstance(playlistEntity.SynkConfig.Uri, playlistEntity.SynkConfig.Provider);
            }
            catch (Exception)
            {
                throw new ApplicationException("Can't resolve playlist provider");
            }

            using (var playlist = new Playlist<TvgMedia>(provider))
            {
                var sourceList = await playlist.PullAsync(cancellationToken);
                //Faire passer les handlers
                sourceList = ExecuteHandlersAsync(sourceList, cancellationToken);

                if (sourceList.Any())
                {
                    playlistEntity.TvgMedias = sourceList.Where(x => x.IsValid).ToList();
                }

                UpdateIsXtreamTag(isXtreamPlaylist, playlistEntity);
            }

            if (playlistEntity.CreatedDate == default)
                await _dbcontext.Playlist.AddAsync(playlistEntity, cancellationToken);

            var res = await _dbcontext.SaveChangesAsync(cancellationToken);
            return playlistEntity;
        }

        /// <summary>
        /// Synk playlist and match epg, logos and groups
        /// </summary>
        /// <param name="playlistId">Playlist id</param>
        /// <param name="resetAndSynch">Reset and synchronize playlist from scrach</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Playlist> SynkPlaylistAsync(int playlistId, bool resetAndSynch = false,
            CancellationToken cancellationToken = default)
        {
            var playlistEntity = await _dbcontext.Playlist.FindAsync(new object[] { playlistId }, cancellationToken);

            if (playlistEntity == null)
                throw new ArgumentNullException($"Playlist not found");
            if (!playlistEntity.IsSynchronizable || playlistEntity.SynkConfig.AutoSynchronize)
                throw new ApplicationException($"Playlist isn't synchronizable or not auto synchrizable is disabled");

            PlaylistProvider<Playlist<TvgMedia>, TvgMedia> provider = null;
            try
            {
                provider = _providerFactory.CreateInstance(playlistEntity.SynkConfig.Uri, playlistEntity.SynkConfig.Provider);
            }
            catch (Exception)
            {
                throw new ApplicationException("Can't resolve playlist provider");
            }

            if (resetAndSynch)
            {
                return await SynkPlaylistAsync(() => _dbcontext.Playlist.Find(playlistId), provider, false, resetAndSynch, cancellationToken);
            }

            var (tvgMedia, removed) = await DiffWithSourceAsync(() => playlistEntity, provider, true, cancellationToken);

            var changed = false;

            if (removed != null && removed.Any())
            {
                playlistEntity.TvgMedias.RemoveAll(x => removed.Any(r => r.Id == x.Id));
                changed = true;
            }
            if (tvgMedia != null && tvgMedia.Any())
            {
                playlistEntity.TvgMedias.AddRange(tvgMedia);
                changed = true;
            }
            if (changed)
            {
                playlistEntity.TvgMedias = playlistEntity.TvgMedias;
                var resRq = await _dbcontext.SaveChangesAsync(cancellationToken);
            }
            return playlistEntity;
        }

        private static void UpdateIsXtreamTag(bool isXtreamPlaylist, Playlist playlistEntity)
        {
            if (!playlistEntity.Tags.Any(x => string.Equals(x.Key, PlaylistTags.IsXtream, StringComparison.InvariantCultureIgnoreCase)))
            {
                playlistEntity.Tags.Add(PlaylistTags.IsXtream, isXtreamPlaylist.ToString());
            }
            else
            {
                playlistEntity.Tags[PlaylistTags.IsXtream] = isXtreamPlaylist.ToString();
            }
            playlistEntity.Tags = playlistEntity.Tags;
        }

        /// <summary>
        /// Execute Handlers on Tvgmedias list
        /// </summary>
        /// <returns></returns>
        public List<TvgMedia> ExecuteHandlersAsync(List<TvgMedia> tvgmedias, CancellationToken cancellationToken = default)
        {
            if (tvgmedias == null)
                throw new ArgumentNullException(nameof(tvgmedias));

            var handler = FabricHandleMedias();

            var newMedias = tvgmedias.AsParallel().Select(media =>
            {
                handler.HandleTvgMedia(media);
                return media;
            }).WithCancellation(cancellationToken);

            return newMedias.ToList();
        }

        /// <summary>
        /// Genére un rapport avec les new medias et 
        /// les médias qui n'existes plus
        /// </summary>
        /// <param name="id"></param>
        /// <param name="force"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<TvgMedia> tvgMedia, IEnumerable<TvgMedia> removed)> DiffWithSourceAsync(Func<Playlist> getPlaylist,
            PlaylistProvider<Playlist<TvgMedia>, TvgMedia> provider, bool force = false,
            CancellationToken cancellationToken = default)
        {
            var pl = getPlaylist();

            if (pl == null)
                throw new ArgumentNullException($"Playlist not found");

            if (!pl.IsSynchronizable)
                throw new ApplicationException($"Playlist isn't synchronizable");

            using (var playlist = new Playlist<TvgMedia>(provider))
            {
                var sourceList = await playlist.PullAsync(cancellationToken);

                if (sourceList == null)
                {
                    return (new List<TvgMedia>(), pl.TvgMedias ?? new List<TvgMedia>());
                }

                return (sourceList.Where(s => pl.TvgMedias.All(t => t.Url != s.Url)).ToList(), pl.TvgMedias.Where(s => sourceList.All(t => t.Url != s.Url)).ToList());
            }
        }

        /// <summary>
        /// Fabriquer les medias handlers (clean names, match epg, etc ...)
        /// </summary>
        /// <param name="synkConfig"></param>
        /// <returns></returns>
        private TvgMediaHandler FabricHandleMedias(SynkConfig synkConfig = default)
        {
            //TODO : Passer synkconfig dans _contextHandler ( no s'il est singleton )

            var cleanNameHandler = new TvgMediaCleanNameHandler(_contextHandler);
            var cultureHandler = new TvgMediaCultureMatcherHandler(_contextHandler);
            var shiftHandler = new TvgMediaShiftMatcherHandler(_contextHandler);
            var sitePackHandler = new TvgMediaEpgMatcherNameHandler(_contextHandler, _sitePackService);
            //var groupHandler = new TvgMediaGroupMatcherHandler(_contextHandler);

            cultureHandler.SetSuccessor(shiftHandler);
            shiftHandler.SetSuccessor(cleanNameHandler);
            cleanNameHandler.SetSuccessor(sitePackHandler);
            return cultureHandler;
        }

        public async Task<PlaylistHealthState> HealthAsync(Playlist pl, CancellationToken cancellationToken = default)
        {
            var result = new PlaylistHealthState
            {
                Id = pl.Id,
                Name = pl.Freindlyname,
                MediaCount = pl.TvgMedias?.Count ?? 0,
                IsOnline = false,
            };
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(pl.SynkConfig.Uri, HttpCompletionOption.ResponseHeadersRead);
                    result.IsOnline = response.IsSuccessStatusCode;
                    return result;
                }
                catch (HttpRequestException)
                {
                    return result;
                }
            }
        }
    }
}
