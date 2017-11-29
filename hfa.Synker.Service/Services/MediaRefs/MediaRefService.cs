using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Entities.MediasRef;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Services.Picons;
using hfa.Synker.Service.Services.Xmltv;
using hfa.Synker.Services.Dal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Synker.Service.Services.MediaRefs
{
    public class MediaRefService : IMediaRefService
    {
        private SynkerDbContext _dbcontext;
        private IElasticConnectionClient _elasticConnectionClient;
        private ILogger _logger;

        public MediaRefService(SynkerDbContext synkerDbContext, IElasticConnectionClient elasticConnectionClient,
            ILoggerFactory loggerFactory)
        {
            _dbcontext = synkerDbContext;
            _elasticConnectionClient = elasticConnectionClient;
            _logger = loggerFactory.CreateLogger(nameof(MediaRefService));
        }

        public async Task<MediaRef> MatchTermByDispaynamesAsync(string term, CancellationToken cancellationToken)
        {
            var allMediasRef = await _elasticConnectionClient.Client.SearchAsync<MediaRef>(s => s
               .Index(_elasticConnectionClient.ElasticConfig.MediaRefIndex)
               .Size(1)
               .From(0)
               .Query(a => a.Match(x => x.Name("matchDisplaynames").Field(f => f.DisplayNames).Query(term)))
               , cancellationToken);

            return allMediasRef.Documents.Distinct(new MediaRef()).FirstOrDefault();
        }

        public async Task<IBulkResponse> RemoveDuplicatedMediaRefAsync(CancellationToken cancellationToken)
        {
            var allMediasRef = await _elasticConnectionClient.Client.SearchAsync<MediaRef>(s => s
               .Index(_elasticConnectionClient.ElasticConfig.MediaRefIndex)
               .Size(_elasticConnectionClient.ElasticConfig.MaxResultWindow)
               .From(0)
               .Query(a => a.MatchAll(x => x.Name("all")))
               , cancellationToken);

            var distinctMediaRef = allMediasRef.Documents.Distinct(new MediaRef()).ToList();

            //Merge groups
            foreach (var m in allMediasRef.Documents)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var occu = distinctMediaRef.FirstOrDefault(d => d.Equals(m));
                occu.Groups.AddRange(m.Groups.Where(x => occu.Groups.All(o => !o.Equals(x))));
            }

            //Remove duplicated
            await _elasticConnectionClient.Client.DeleteByQueryAsync<MediaRef>(d => d.MatchAll(), cancellationToken);
            return await _elasticConnectionClient.Client.IndexManyAsync<MediaRef>(distinctMediaRef, _elasticConnectionClient.ElasticConfig.MediaRefIndex, null, cancellationToken);
        }

        public async Task<IBulkResponse> SynkAsync(CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.SearchAsync<SitePackChannel>(s => s
               .Index(_elasticConnectionClient.ElasticConfig.SitePackIndex)
               .Size(_elasticConnectionClient.ElasticConfig.MaxResultWindow)
               .From(0)
               .Aggregations(a => a
                   .Terms("unique", te => te
                       .Field(f => f.Channel_name.Suffix(ElasticConfig.ELK_KEYWORD_SUFFIX))
                       .Order(TermsOrder.TermAscending)
                   )
               )
            , cancellationToken);

            var mediasRef = response.Documents.Select(x => new MediaRef(x.Channel_name, x.Site, x.Country, x.Xmltv_id, x.id)).ToList();

            //var qc = new QueryContainerDescriptor<Picon>();

            //var query = mediasRef.SelectMany(x => x.DisplayNames.Take(2))
            //    .Select(val => qc.Fuzzy(fz => fz.Field(f => f.Name).Value(val.Replace("+", "plus")))).Aggregate((a, b) => a || b);

            //var query = mediasRef.Select(x => x.DisplayNames.FirstOrDefault())
            //    .Select(val => qc.Fuzzy(fz => fz.Field(f => f.Name).Value(val.Replace("+", "plus")))).FirstOrDefault();

            //var searchPiconDisc = new SearchDescriptor<Picon>()
            //    .Size(1)
            //    .Query(x => query);

            var tasks = mediasRef.Select(async m =>
            {
                var findLogoResponse = await _elasticConnectionClient.Client.SearchAsync<Picon>(x =>
                x.Query(q => q.Match(fz =>
                          fz.Field(f => f.Name)
                             .Query(m.DisplayNames.FirstOrDefault()))).Size(1), cancellationToken);

                if (findLogoResponse.Documents.Any())
                    m.Tvg.Logo = findLogoResponse.Documents.First().RawUrl;
            });

            await Task.WhenAll(tasks);

            var descriptor = new BulkDescriptor();

            descriptor.IndexMany(mediasRef);

            return await _elasticConnectionClient.Client.BulkAsync(descriptor, cancellationToken);
        }

        /// <summary>
        /// Synchronize mediaRef picons 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IBulkResponse> SynkPiconsAsync(CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.SearchAsync<MediaRef>(s => s
               .Index(_elasticConnectionClient.ElasticConfig.MediaRefIndex)
               .Size(_elasticConnectionClient.ElasticConfig.MaxResultWindow)
               .From(0)
               .Query(q => q.MatchAll())
            , cancellationToken);

            var tasks = response.Documents.Select(async m =>
            {
                var findLogoResponse = await _elasticConnectionClient.Client.SearchAsync<Picon>(x =>
                x.Query(q => q.Match(fz =>
                          fz.Field(f => f.Name)
                             .Query(m.DisplayNames.FirstOrDefault()))).Size(1), cancellationToken);

                if (findLogoResponse.Documents.Any())
                    m.Tvg.Logo = findLogoResponse.Documents.First().RawUrl;
            });

            await Task.WhenAll(tasks);

            var descriptor = new BulkDescriptor();

            descriptor.IndexMany(response.Documents);

            return await _elasticConnectionClient.Client.BulkAsync(descriptor, cancellationToken);
        }

        public async Task<List<string>> ListCulturesAsync(string filter, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.SearchAsync<MediaRef>(s => s
              .Index(_elasticConnectionClient.ElasticConfig.MediaRefIndex)
              .Size(_elasticConnectionClient.ElasticConfig.MaxResultWindow)
              .From(0)
              .Aggregations(a => a
                  .Terms("unique", te => te
                      .Field(f => f.Cultures.Suffix("keyword"))
                      .Order(TermsOrder.TermAscending)
                  )
              )
           , cancellationToken);

            return response.Documents.SelectMany(x => x.Cultures).Distinct().ToList();
        }

        public async Task<long> DeleteManyAsync(string[] ids, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.DeleteByQueryAsync<MediaRef>(x => x.Query(q => q.Ids(i => i.Values(ids))));
            return response.Deleted;
        }

        public async Task<IBulkResponse> SaveAsync(List<MediaRef> mediasRef, CancellationToken cancellationToken)
        {
            var descriptor = new BulkDescriptor();
            descriptor.IndexMany(mediasRef);
            descriptor.Refresh(Elasticsearch.Net.Refresh.True);
            return await _elasticConnectionClient.Client.BulkAsync(descriptor, cancellationToken);
        }

        public async Task<ISearchResponse<MediaRef>> GroupsAsync(string filter, int? size, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                return await _elasticConnectionClient.Client.SearchAsync<MediaRef>(s => s
                 .Index(_elasticConnectionClient.ElasticConfig.MediaRefIndex)
                 .Size(size.Value)
                 .Query(x => x.Match(m => m.Field(f => f.Groups.Suffix(ElasticConfig.ELK_KEYWORD_SUFFIX)).Query(filter)))
              , cancellationToken);
            }
            else
            {
                return await _elasticConnectionClient.Client.SearchAsync<MediaRef>(s => s
                     .Index(_elasticConnectionClient.ElasticConfig.MediaRefIndex)
                     .Size(0)
                     .Aggregations(a => a
                         .Terms("groups_aggr", te => te
                            .Size(_elasticConnectionClient.ElasticConfig.MaxResultWindow)
                             .Field(f => f.Groups.Suffix(ElasticConfig.ELK_KEYWORD_SUFFIX))
                             .Order(TermsOrder.TermAscending)
                         )
                     )
                  , cancellationToken);
            }
        }
    }
}
