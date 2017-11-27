using hfa.Synker.Service.Entities.MediasRef;
using Nest;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Synker.Service.Services.MediaRefs
{
    public interface IMediaRefService
    {
        Task<List<string>> ListCulturesAsync(CancellationToken cancellationToken);
        Task<IBulkResponse> RemoveDuplicatedMediaRefAsync(CancellationToken cancellationToken);
        Task<IBulkResponse> SynkAsync(CancellationToken cancellationToken);

        Task<long> DeleteManyAsync(string[] ids, CancellationToken cancellationToken);
        Task<IBulkResponse> SaveAsync(List<MediaRef> mediasRef, CancellationToken cancellationToken);
        Task<ISearchResponse<MediaRef>> GroupsAsync(string filter, int? size, CancellationToken cancellationToken);
        Task<MediaRef> MatchTermByDispaynamesAsync(string term, CancellationToken cancellationToken);
    }
}