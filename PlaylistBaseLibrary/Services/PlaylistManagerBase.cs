using System;
using System.Threading;
using System.Threading.Tasks;
using PlaylistManager.Entities;

namespace PlaylistManager.Services
{
    public abstract class PlaylistManagerBase: IDisposable
    {
        public abstract Task<(TPlaylist, TPlaylist, TPlaylist)> DiffAsync<TPlaylist>(TPlaylist playlist1, TPlaylist playlist2, CancellationToken token) where TPlaylist : Playlist<TvgMedia>;
        public abstract Task CleanMediaNameAsync<TPlaylist>(TPlaylist playlist, CancellationToken token) where TPlaylist : Playlist<TvgMedia>;
        public abstract Task MatchEPGAsync<TPlaylist>(TPlaylist playlist, CancellationToken token) where TPlaylist : Playlist<TvgMedia>;
        public abstract Task GroupMediaAsync<TPlaylist>(TPlaylist playlist, CancellationToken token) where TPlaylist : Playlist<TvgMedia>;

        ///// <summary>
        ///// Clean channel names
        ///// Add lang
        ///// Group channels
        ///// Match EPG
        ///// </summary>
        ///// <typeparam name="TPlaylist"></typeparam>
        ///// <param name="playlist"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //public abstract Task Process<TPlaylist>(TPlaylist playlist, CancellationToken token) where TPlaylist : Playlist<Media>;

        public virtual void Dispose() { }
    }
}