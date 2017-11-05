using hfa.Synker.Service.Elastic;
using Nest;

namespace hfa.Synker.Service.Services.Elastic
{
    public interface IElasticConnectionClient
    {
        ElasticClient Client { get; }

        void DeleteDefaultIndex();
        void MappingPlaylistConfig();

        ElasticConfig ElasticConfig { get; }
    }
}