namespace hfa.Synker.Service.Services.Elastic
{
    using hfa.Synker.Service.Elastic;
    using Nest;
    using System;

    public interface IElasticConnectionClient
    {
        Lazy<ElasticClient> Client { get; }

        void DeleteDefaultIndex();

        void MappingPlaylistConfig();

        ElasticConfig ElasticConfig { get; }
    }
}