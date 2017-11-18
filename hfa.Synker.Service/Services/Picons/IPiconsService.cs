using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nest;

namespace hfa.Synker.Service.Services.Picons
{
    public interface IPiconsService
    {
        Task<IEnumerable<Picon>> GetPiconsFromGithubRepoAsync(SynkPiconConfig synkPiconConfig, CancellationToken cancellationToken);
        Task<IBulkResponse> SynkAsync(IEnumerable<Picon> picons, CancellationToken cancellationToken);
    }
}