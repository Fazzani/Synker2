using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using PlaylistManager.Entities;

namespace hfa.Synker.Service.Services.Picons
{
    public interface IPiconsService
    {
        Task<IEnumerable<Picon>> GetPiconsFromGithubRepoAsync(SynkPiconConfig synkPiconConfig, CancellationToken cancellationToken = default);
        /// <summary>
        /// Synk picons
        /// </summary>
        /// <param name="picons"></param>
        /// <param name="reset">Delete the index and recreate it</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IBulkResponse> SynkAsync(IEnumerable<Picon> picons, bool reset = false, CancellationToken cancellationToken = default);

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