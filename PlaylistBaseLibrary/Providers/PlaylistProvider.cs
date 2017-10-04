using PlaylistBaseLibrary.Providers.Linq;
using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.PlaylistBaseLibrary.Providers
{
    public abstract class PlaylistProvider<TPlaylist, TMedia> : QueryProvider, IPlaylistProvider<TPlaylist, TMedia>
          where TPlaylist : Playlist<TMedia>
        where TMedia : Media
    {
        public abstract void Dispose();

        public abstract IEnumerable<TMedia> Pull();

        public abstract Task<IEnumerable<TMedia>> PullAsync(CancellationToken token);

        public abstract void Push(TPlaylist playlist);

        public abstract Task PushAsync(TPlaylist playlist, CancellationToken token);

        public abstract TPlaylist Sync(TPlaylist playlist);

        public abstract Task<TPlaylist> SyncAsync(TPlaylist playlist, CancellationToken token);
    }
}
