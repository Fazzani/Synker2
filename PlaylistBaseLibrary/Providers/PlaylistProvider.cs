using PlaylistBaseLibrary.Providers.Linq;
using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.PlaylistBaseLibrary.Providers
{
    public abstract class PlaylistProvider<TPlaylist, TMedia> : QueryProvider, IPlaylistProvider<TPlaylist, TMedia>
          where TPlaylist : Playlist<TMedia>
        where TMedia : Media
    {

        public abstract MemoryStream PlaylistStream { get; }
        
        public abstract void Dispose();

        public abstract IEnumerable<TMedia> Pull();

        public abstract Task<IEnumerable<TMedia>> PullAsync(CancellationToken token);

        public abstract void Push(TPlaylist playlist);

        public abstract Task PushAsync(TPlaylist playlist, CancellationToken token);

        public abstract TPlaylist Sync(TPlaylist playlist);

        public abstract Task<TPlaylist> SyncAsync(TPlaylist playlist, CancellationToken token);

        /// <summary>
        /// Get FileProvider instance from name (string)
        /// ex: m3u, tvlist
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="providersOptions"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static PlaylistProvider<TPlaylist, TMedia> Create(string provider, List<PlaylistProviderOption> providersOptions, Uri uri)
        {
            var sourceOption = providersOptions.FirstOrDefault(x => x.Name.Equals(provider, StringComparison.InvariantCultureIgnoreCase));
            if (sourceOption == null)
                throw new InvalidFileProviderException($"Source Provider not found : {provider}");

            var targetProviderType = Type.GetType(sourceOption.Type, false, true);
            if (targetProviderType == null)
                throw new InvalidFileProviderException($"Target Provider not found : {sourceOption.Type}");

            return (PlaylistProvider<TPlaylist, TMedia>)Activator.CreateInstance(targetProviderType, uri);
        }
    }
}
