using hfa.Synker.Service.Entities.Playlists;
using hfa.Synker.Services.Dal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace hfa.Synker.Service.Services.Playlists
{
    public class PlaylistService : IPlaylistService
    {
        private SynkerDbContext _dbcontext;

        public PlaylistService(SynkerDbContext synkerDbContext)
        {
            _dbcontext = synkerDbContext;
        }

        public async Task<IEnumerable<Playlist>> ListByUserAsync(int userId)
        {
            return (await _dbcontext.Users.FindAsync(userId))?.Playlists;
        }
    }
}
