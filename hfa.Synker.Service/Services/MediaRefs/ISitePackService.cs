using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using hfa.Synker.Service.Services.Xmltv;
using Nest;

namespace hfa.Synker.Service.Services
{
    public interface ISitePackService
    {

        /// <summary>
        /// Search by MediaName and filter by site
        /// </summary>
        /// <param name="mediaName"></param>
        /// <param name="site"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SitePackChannel> MatchMediaNameAndBySiteAsync(string mediaName, string site, CancellationToken cancellationToken);

        /// <summary>
        /// Search by MediaName and filter by country (Lang)
        /// </summary>
        /// <param name="mediaName"></param>
        /// <param name="country"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SitePackChannel> MatchMediaNameAndCountryAsync(string mediaName, string country, CancellationToken cancellationToken);

        /// <summary>
        /// List SitePack
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="count"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<SitePackChannel>> ListSitePackAsync(string filter, int count = 10, CancellationToken cancellationToken = default);

        Task<SitePackChannel> MatchTermByDispaynamesAndFiltredBySiteNameAsync(string mediaName, string culture, IEnumerable<string> tvgSites, CancellationToken cancellationToken);
        Task<IBulkResponse> SaveAsync(List<SitePackChannel> sitepacks, CancellationToken cancellationToken);
        Task<long> DeleteManyAsync(string[] ids, CancellationToken cancellationToken);

        /// <summary>
        /// List Countries
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<string>> ListCountriesAsync(string filter, CancellationToken cancellationToken);

        /// <summary>
        /// Match Tvg media by mediaName AND/OR Country
        /// AND/OR TvgSites
        /// </summary>
        /// <param name="mediaName"></param>
        /// <param name="country"></param>
        /// <param name="tvgSites"></param>
        /// <param name="minScore"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SitePackChannel> MatchTvgAsync(string mediaName, string country, IEnumerable<string> tvgSites, double minScore, CancellationToken cancellationToken);

        /// <summary>
        /// Force update country field
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IBulkResponse> SyncCountrySitePackAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Get all used Sitepack channels by playlists
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<SitePackChannel>> GetAllFromPlaylists(CancellationToken cancellationToken = default);
    }
}