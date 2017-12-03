using hfa.Synker.Service.Entities.MediasRef;
using Nest;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Synker.Service.Services.MediaRefs
{
    public interface IMediaRefService
    {
        Task<List<string>> ListCulturesAsync(string filter, CancellationToken cancellationToken);
        Task<IBulkResponse> RemoveDuplicatedMediaRefAsync(CancellationToken cancellationToken);
        Task<IBulkResponse> SynkAsync(CancellationToken cancellationToken);

        Task<long> DeleteManyAsync(string[] ids, CancellationToken cancellationToken);
        Task<IBulkResponse> SaveAsync(List<MediaRef> mediasRef, CancellationToken cancellationToken);
        Task<ISearchResponse<MediaRef>> GroupsAsync(string filter, int? size, CancellationToken cancellationToken);
        Task<MediaRef> MatchTermByDispaynamesAsync(string term, CancellationToken cancellationToken);
        /// <summary>
        /// Synchronize mediaRef picons 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IBulkResponse> SynkPiconsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// List des tvg providers dispo
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="size"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ICollection<string>> ListTvgSitesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Match displayNames with filtred MediaRef by TvgSites
        /// </summary>
        /// <param name="term"></param>
        /// <param name="tvgSites"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<MediaRef> MatchTermByDispaynamesAndFiltredBySiteNameAsync(string term, string culture, IEnumerable<string> tvgSites, CancellationToken cancellationToken);
    }
}