using System.Collections.Generic;
using System.Threading.Tasks;
using hfa.Synker.Service.Entities.Playlists;

namespace hfa.Synker.Service.Services.Playlists
{
    public interface IPlaylistService
    {
        Task<IEnumerable<Playlist>> ListByUserAsync(int userId);
    }
}