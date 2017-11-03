using hfa.Synker.Service.Entities.Playlists;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Services.TvgMediaHandlers;
using hfa.Synker.Services.Dal;
using Microsoft.EntityFrameworkCore;
using PlaylistBaseLibrary.ChannelHandlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace hfa.Synker.Service.Services.Playlists
{
    public class PlaylistService : IPlaylistService
    {
        private SynkerDbContext _dbcontext;
        private IElasticConnectionClient _elasticConnectionClient;
        private IContextTvgMediaHandler _contextHandler;

        public PlaylistService(SynkerDbContext synkerDbContext, IElasticConnectionClient elasticConnectionClient, IContextTvgMediaHandler contextHandler)
        {
            _dbcontext = synkerDbContext;
            _elasticConnectionClient = elasticConnectionClient;
            _contextHandler = contextHandler;
        }

        public async Task<IEnumerable<Playlist>> ListByUserAsync(int userId)
        {
            return (await _dbcontext.Users.FindAsync(userId))?.Playlists;
        }

        public async Task SynkPlaylist(int id)
        {
            var pl = await _dbcontext.Playlist.FindAsync(id);
            if (pl == null)
                throw new ArgumentNullException($"Playlist not found {id}");


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
