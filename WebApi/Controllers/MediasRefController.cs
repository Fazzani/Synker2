using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using Nest;
using hfa.Synker.Services.Dal;
using Microsoft.AspNetCore.Authorization;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Services.Xmltv;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Entities.MediasRef;
using static Hfa.WebApi.Controllers.Constants;
using hfa.WebApi.Models;
using Hfa.WebApi.Common;
using hfa.Synker.Service.Services.Picons;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class MediasRefController : BaseController
    {
        private const int ElasticMaxResult = 200000;

        public MediasRefController(IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
            IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
        }

        [HttpPost]
        [Route("_search")]
        public async Task<IActionResult> SearchAsync([FromBody]dynamic request, CancellationToken cancellationToken)
        {
            return await SearchAsync<MediaRef>(request.ToString(), nameof(MediaRef).ToLowerInvariant(), cancellationToken);
        }

        [HttpPost]
        [Route("groups")]
        public async Task<IActionResult> GroupsAsync([FromBody]ElasticQueryAggrRequest query, CancellationToken cancellationToken)
        {
            ISearchResponse<MediaRef> response = null;
            if (query != null)
            {
                response = await _elasticConnectionClient.Client.SearchAsync<MediaRef>(s => s
                 .Index(_elasticConfig.MediaRefIndex)
                 .Size(query.Size)
                 .Query(x => x.Match(m => m.Field(f => f.Groups.Suffix(ELK_KEYWORD_SUFFIX)).Query(query.Filter)))
              , cancellationToken);

                if (!response.IsValid)
                    return BadRequest(response.DebugInformation);

                return new OkObjectResult(response.GetResultListModel());
            }
            else
            {
                response = await _elasticConnectionClient.Client.SearchAsync<MediaRef>(s => s
                     .Index(_elasticConfig.MediaRefIndex)
                     .Size(0)
                     .Aggregations(a => a
                         .Terms("groups_aggr", te => te
                            .Size(ElasticMaxResult)
                             .Field(f => f.Groups.Suffix(ELK_KEYWORD_SUFFIX))
                             .Order(TermsOrder.TermAscending)
                         )
                     )
                  , cancellationToken);

                if (!response.IsValid)
                    return BadRequest(response.DebugInformation);

                return new OkObjectResult(response.Aggs);
            }
        }

        [HttpPost]
        [Route("synk")]
        public async Task<IActionResult> SynkAsync(CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.SearchAsync<SitePackChannel>(s => s
               .Index(_elasticConfig.SitePackIndex)
               .Size(ElasticMaxResult)
               .From(0)
               .Aggregations(a => a
                   .Terms("unique", te => te
                       .Field(f => f.Channel_name.Suffix(ELK_KEYWORD_SUFFIX))
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

            var result = await _elasticConnectionClient.Client.BulkAsync(descriptor, cancellationToken);

            if (!result.IsValid)
                return BadRequest(result.DebugInformation);

            return new OkObjectResult(result.Items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var response = await _elasticConnectionClient.Client.GetAsync(new DocumentPath<MediaRef>(id), null, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Source);
        }

        [HttpPost]
        public async Task<IActionResult> Save(List<MediaRef> mediasRef, CancellationToken cancellationToken)
        {
            var descriptor = new BulkDescriptor();
            //foreach (var item in mediasRef)
            //{
            //    descriptor.Update<MediaRef>(a => a.Doc(item));
            //}
            descriptor.UpdateMany(mediasRef, (a, o) => a.DocAsUpsert());
            descriptor.Refresh(Elasticsearch.Net.Refresh.True);

            var response = await _elasticConnectionClient.Client.BulkAsync(descriptor, cancellationToken);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.Items);
        }
    }
}
