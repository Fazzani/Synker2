using PlaylistManager.Entities;
using PlaylistManager.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SyncLibrary.Configuration.Entites
{
    /// <summary>
    /// files, TVH, Elastic, BD 
    /// </summary>
    internal class ConfigSync
    {
        public List<ISynchronizableConfig> Sources { get; set; }

        public ISynchronizableConfig Target { get; set; }
    }
}
