using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using PlaylistManager.Entities;

namespace hfa.Synker.Service.Services.Picons
{
    public interface IPiconsService
    {
        Task<IEnumerable<Picon>> GetPiconsFromGithubRepoAsync(SynkPiconConfig synkPiconConfig, CancellationToken cancellationToken);
        Task<IBulkResponse> SynkAsync(IEnumerable<Picon> picons, CancellationToken cancellationToken);

        /// <summary>
        /// Match by media name and media number
        /// </summary>
        /// <param name="mediaName"></param>
        /// <param name="mediaNumber"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<Picon>> MatchAsync(string mediaName, int? mediaNumber, double minimumShouldMatch = 90.0, CancellationToken cancellationToken = default);
    }
}