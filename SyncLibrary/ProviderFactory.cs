using PlaylistBaseLibrary.Providers;
using PlaylistManager.Entities;
using PlaylistManager.Providers;
using SyncLibrary.Configuration.Entites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SyncLibrary
{
    class ProviderFactory
    {
        public static async Task<PlaylistProvider<Playlist<TvgMedia>, TvgMedia>> Create(ISynchronizableConfig synchronizableConfig)
        {
            if (synchronizableConfig is SynchronizableConfigFile && synchronizableConfig.SynchConfigType == SynchronizableConfigType.M3u)
                return new M3uProvider(System.IO.File.OpenRead((synchronizableConfig as SynchronizableConfigFile).Path));
            if (synchronizableConfig is SynchronizableConfigUri)
            {
                var synConfig = synchronizableConfig as SynchronizableConfigUri;
                using (var client = new WebClient())
                {
                    var fileName = $"{Path.GetRandomFileName()}.tmp";
                    await client.DownloadFileTaskAsync(synConfig.Uri, fileName);
                    if (synchronizableConfig.SynchConfigType == SynchronizableConfigType.M3u)
                        return new M3uProvider(System.IO.File.OpenRead(fileName));
                    if (synchronizableConfig.SynchConfigType == SynchronizableConfigType.Tvlist)
                        return new TvlistProvider(System.IO.File.OpenRead(fileName));
                }
            }
            return null;
        }
    }
}
