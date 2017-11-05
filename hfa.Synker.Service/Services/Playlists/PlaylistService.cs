﻿using hfa.PlaylistBaseLibrary.Providers;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Entities.Playlists;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Services.TvgMediaHandlers;
using hfa.Synker.Services.Dal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PlaylistBaseLibrary.ChannelHandlers;
using PlaylistManager.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Synker.Service.Services.Playlists
{
    public class PlaylistService : IPlaylistService
    {
        private SynkerDbContext _dbcontext;
        private IElasticConnectionClient _elasticConnectionClient;
        private IContextTvgMediaHandler _contextHandler;
        private ILogger _logger;
        private IOptions<ElasticConfig> _elasticConfig;

        public PlaylistService(SynkerDbContext synkerDbContext, IElasticConnectionClient elasticConnectionClient,
            IContextTvgMediaHandler contextHandler, ILoggerFactory loggerFactory, IOptions<ElasticConfig> elasticConfig)
        {
            _dbcontext = synkerDbContext;
            _elasticConnectionClient = elasticConnectionClient;
            _contextHandler = contextHandler;
            _logger = loggerFactory.CreateLogger(nameof(PlaylistService));
            _elasticConfig = elasticConfig;
        }

        public async Task<IEnumerable<Playlist>> ListByUserAsync(int userId)
        {
            return (await _dbcontext.Users.FindAsync(userId))?.Playlists;
        }

        /// <summary>
        /// Synk playlist and match epg, logos and groups
        /// </summary>
        /// <param name="id"></param>
        /// <param name="force"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Playlist> SynkPlaylist(Func<Playlist> getPlaylist, FileProvider provider, bool force = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            //var pl = await _dbcontext.Playlist.FindAsync(id);
            var pl = getPlaylist();
            if (pl == null)
                throw new ArgumentNullException($"Playlist not found");
            if (!pl.IsSynchronizable)
                throw new ApplicationException($"Playlist isn't synchronizable");

            //var newMedias = new BlockingCollection<TvgMedia>();
            using (var playlist = new Playlist<TvgMedia>(provider))
            {
                var sourceList = await playlist.PullAsync(cancellationToken);
                //Faire passer les handlers
                var handler = FabricHandleMedias(_elasticConnectionClient, pl.SynkConfig);

                var newMedias = sourceList.AsParallel().Select(media =>
                  {
                      handler.HandleTvgMedia(media);
                      return media;
                  }).WithCancellation(cancellationToken);

                if (newMedias.Any())
                    pl.Content = JsonConvert.SerializeObject(newMedias.Where(x => x.IsValid).ToArray());
            }
            return pl;
        }

        /// <summary>
        /// Fabriquer les medias handlers (clean names, match epg, etc ...)
        /// </summary>
        private TvgMediaHandler FabricHandleMedias(IElasticConnectionClient elasticConnectionClient, SynkConfig synkConfig)
        {
            //TODO : Passer synkconfig dans _contextHandler ( no s'il est singleton )

            var cleanNameHandler = new TvgMediaCleanNameHandler(_contextHandler);
            var groupHandler = new TvgMediaGroupMatcherHandler(_contextHandler);
            var epgHandler = new TvgMediaEpgMatcherNameHandler(_contextHandler, elasticConnectionClient, _elasticConfig);
            var langHandler = new TvgMediaLangMatcherHandler(_contextHandler);

            langHandler.SetSuccessor(groupHandler);
            groupHandler.SetSuccessor(epgHandler);
            epgHandler.SetSuccessor(cleanNameHandler);
            return langHandler;
        }
    }
}
