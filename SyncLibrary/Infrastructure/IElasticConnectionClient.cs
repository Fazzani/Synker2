using Hfa.SyncLibrary.Infrastructure;
using Microsoft.Extensions.Options;
using Nest;

namespace hfa.SyncLibrary.Common
{
    internal interface IElasticConnectionClient
    {
        ElasticClient Client { get; }

        void DeleteDefaultIndex();

        void MappingConfig(IOptions<ApplicationConfigData> config);
    }
}