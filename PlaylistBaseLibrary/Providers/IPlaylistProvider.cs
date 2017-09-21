using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PlaylistManager.Providers
{
    public interface IPlaylistProvider<TPlaylist, TMedia>: IDisposable 
        where TPlaylist : Playlist<TMedia> 
        //where TMedia : Media
    {
        IEnumerable<TMedia> Pull();
        Task<IEnumerable<TMedia>> PullAsync(CancellationToken token);
        void Push(TPlaylist playlist);
        Task PushAsync(TPlaylist playlist, CancellationToken token);

        /// <summary>
        /// Push and Pull
        /// </summary>
        TPlaylist Sync(TPlaylist playlist);

        /// <summary>
        /// Push and pull
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<TPlaylist> SyncAsync(TPlaylist playlist, CancellationToken token);
    }
}