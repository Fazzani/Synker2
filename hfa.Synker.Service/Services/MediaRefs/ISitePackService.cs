using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using hfa.Synker.Service.Services.Xmltv;

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
        /// List SitePack
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="count"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<SitePackChannel>> ListSitePackAsync(string filter, int count = 10, CancellationToken cancellationToken = default);

    }
}