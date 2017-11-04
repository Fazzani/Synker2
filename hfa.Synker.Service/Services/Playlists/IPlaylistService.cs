using System.Collections.Generic;
using System.Threading.Tasks;
using hfa.Synker.Service.Entities.Playlists;
using System.Threading;
using hfa.PlaylistBaseLibrary.Providers;
using System;

namespace hfa.Synker.Service.Services.Playlists
{
    public interface IPlaylistService
    {
        Task<IEnumerable<Playlist>> ListByUserAsync(int userId);
        Task<Playlist> SynkPlaylist(Func<Playlist> getPlaylist, FileProvider provider, bool force = false,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}