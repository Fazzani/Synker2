namespace SyncLibrary.Configuration.Entites
{
    internal interface ISynchronizableConfig
    {
        SynchronizableConfigType SynchConfigType { get; set; }
    }
}