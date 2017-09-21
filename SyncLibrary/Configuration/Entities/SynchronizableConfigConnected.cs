namespace SyncLibrary.Configuration.Entites
{
    internal class SynchronizableConfigConnected : ISynchronizableConfig
    {
        public ConnectionData Connection { get; set; }
        public SynchronizableConfigType SynchConfigType { get; set; }

        public override string ToString() => $"{Connection.ServerUri}";
    }

    enum SynchronizableConfigType : byte
    {
        None,
        /// <summary>
        /// Tvheadend
        /// </summary>
        Tvh,
        /// <summary>
        /// ElasticSearch
        /// </summary>
        ElasticSearch,
        M3u,
        Tvlist
        
    }
}