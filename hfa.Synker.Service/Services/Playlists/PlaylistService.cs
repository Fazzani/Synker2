using hfa.Synker.Service.Entities.Playlists;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Services.TvgMediaHandlers;
using hfa.Synker.Services.Dal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlaylistBaseLibrary.ChannelHandlers;
using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public PlaylistService(SynkerDbContext synkerDbContext, IElasticConnectionClient elasticConnectionClient, 
            IContextTvgMediaHandler contextHandler, ILoggerFactory loggerFactory)
        {
            _dbcontext = synkerDbContext;
            _elasticConnectionClient = elasticConnectionClient;
            _contextHandler = contextHandler;
            _logger = loggerFactory.CreateLogger(nameof(PlaylistService));
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
        public async Task SynkPlaylist(int id, bool force = false, CancellationToken token = default(CancellationToken))
        {
            var pl = await _dbcontext.Playlist.FindAsync(id);
            if (pl == null)
                throw new ArgumentNullException($"Playlist not found {id}");

            var newMedias = new List<TvgMedia>();


            //Faire passer les handlers
            var handler = FabricHandleMedias(_elasticConnectionClient);

            foreach (var media in pl.TvgMedias)
            {
                handler.HandleTvgMedia(media);
                if (media.IsValid)
                {
                    _logger.LogInformation($"Treating media  => {media.Name} : {media.Url}");

                    var result = await _elasticConnectionClient.Client
                        .SearchAsync<TvgMedia>(x => x.Index<TvgMedia>()
                            .Query(q => q.Term(m => m.Url, media.Url)), token);

                    if (result.Total < 1)
                    {
                        newMedias.Add(media);
                    }
                    else
                    {
                        if (force && media != result.Documents.FirstOrDefault() || (media.Tvg?.Id != result.Documents.FirstOrDefault()?.Id))
                        {
                            //Modification
                            _logger.LogInformation($"Updating media {result.Documents.SingleOrDefault().Id} in Elastic");

                            var response = await _elasticConnectionClient.Client.UpdateAsync<TvgMedia>(result.Documents.SingleOrDefault().Id,
                                m => m.Doc(new TvgMedia { Id = null, Name = media.Name, Lang = media.Lang }), token);
                        }
                    }
                }
            }
            if (newMedias.Any())
            {
                //Push to Elastic
                var responseBulk = await _elasticConnectionClient.Client.BulkAsync(x => x.Index(_elasticConnectionClient.ElasticConfig.DefaultIndex).CreateMany(newMedias,
                    (bd, q) => bd.Index(_elasticConnectionClient.ElasticConfig.DefaultIndex)), token);

               // responseBulk.AssertElasticResponse();
            }
        }

        /// <summary>
        /// Fabriquer les medias handlers (clean names, match epg, etc ...)
        /// </summary>
        private TvgMediaHandler FabricHandleMedias(IElasticConnectionClient elasticConnectionClient)
        {
            var cleanNameHandler = new TvgMediaCleanNameHandler(_contextHandler);
            var groupHandler = new TvgMediaGroupMatcherHandler(_contextHandler);
            var epgHandler = new TvgMediaEpgMatcherNameHandler(_contextHandler, elasticConnectionClient);
            var langHandler = new TvgMediaLangMatcherHandler(_contextHandler);

            langHandler.SetSuccessor(groupHandler);
            groupHandler.SetSuccessor(epgHandler);
            epgHandler.SetSuccessor(cleanNameHandler);
            return langHandler;
        }
    }
}
