using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Services.Xmltv;
using hfa.Synker.Services.Dal;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace hfa.Synker.Service.Services
{
    public class SitePackService : ISitePackService
    {
        private SynkerDbContext _dbcontext;
        private IElasticConnectionClient _elasticConnectionClient;
        private ILogger _logger;

        public SitePackService(SynkerDbContext synkerDbContext, IElasticConnectionClient elasticConnectionClient,
            ILoggerFactory loggerFactory)
        {
            _dbcontext = synkerDbContext;
            _elasticConnectionClient = elasticConnectionClient;
            _logger = loggerFactory.CreateLogger(nameof(SitePackService));
        }

        /// <summary>
        /// Search by MediaName and filter by site
        /// </summary>
        /// <param name="mediaName"></param>
        /// <param name="site"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SitePackChannel> MatchMediaNameAndBySiteAsync(string mediaName, string site, CancellationToken cancellationToken)
        {
            var req = new SearchRequest<SitePackChannel>
            {
                From = 0,
                Size = 1,
                Query = new TermQuery { Field = "site.keyword", Value = site } &
                        new MatchQuery { Field = "channel_name", Query = mediaName, Fuzziness = Fuzziness.Auto }
            };

            var sitePacks = await _elasticConnectionClient.Client.SearchAsync<SitePackChannel>(req, cancellationToken);

            return sitePacks.Documents.FirstOrDefault();
        }

        /// <summary>
        /// List SitePack channels
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="count"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<SitePackChannel>> ListSitePackAsync(string filter, int count = 10, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Lister les SitePackChannels");

            var tvgSites = await _elasticConnectionClient.Client.SearchAsync<SitePackChannel>(s => s
               .Index(_elasticConnectionClient.ElasticConfig.SitePackIndex)
               .Size(count)
               .From(0)
               .Query(a => a.Wildcard(x => x.Field(f => f.Site).Value(filter)))
               , cancellationToken);

            return tvgSites.Documents.Distinct(new DistinctTvgSiteBySite()).ToList();
        }
    }
}
