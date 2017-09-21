using PlaylistManager.Entities;
using PlaylistManager.Providers;
using PlaylistManager.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlaylistBaseLibrary.Services
{
    public abstract class PlaylistDecorator : PlaylistManagerBase
    {
        readonly PlaylistManagerBase _playlistManager;

        protected PlaylistDecorator(PlaylistManagerBase playlistManager)
        {
            _playlistManager = playlistManager;
        }

        public override Task CleanMediaNameAsync<TPlaylist>(TPlaylist playlist, CancellationToken token) =>
           _playlistManager.CleanMediaNameAsync(playlist, token);

        public override Task<(TPlaylist, TPlaylist, TPlaylist)> DiffAsync<TPlaylist>(TPlaylist playlist1, TPlaylist playlist2, CancellationToken token) =>
            _playlistManager.DiffAsync(playlist1, playlist2, token);

        public override void Dispose() =>
            _playlistManager.Dispose();

        public override Task GroupMediaAsync<TPlaylist>(TPlaylist playlist, CancellationToken token) =>
            _playlistManager.GroupMediaAsync(playlist, token);

        public override Task MatchEPGAsync<TPlaylist>(TPlaylist playlist, CancellationToken token) =>
           _playlistManager.MatchEPGAsync(playlist, token);

    }
}
