using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Services.Elastic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Synker.Service.Services.Picons
{
    public class PiconsService : IPiconsService
    {
        private IElasticConnectionClient _elasticConnectionClient;
        private ILogger _logger;
        private IOptions<ElasticConfig> _elasticConfig;

        public PiconsService(IElasticConnectionClient elasticConnectionClient, ILoggerFactory loggerFactory, IOptions<ElasticConfig> elasticConfig)
        {
            _elasticConnectionClient = elasticConnectionClient;
            _logger = loggerFactory.CreateLogger(nameof(PiconsService));
            _elasticConfig = elasticConfig;
        }

        public async Task<IEnumerable<Picon>> GetPiconsFromGithubRepoAsync(SynkPiconConfig synkPiconConfig, CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(synkPiconConfig.ApiUrl);
                client.DefaultRequestHeaders.Add("user-agent", "node.js");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var responseCommits = await client.GetAsync(synkPiconConfig.BranchesApiUrl);
                if (responseCommits.IsSuccessStatusCode)
                {
                    var contentComits = await responseCommits.Content.ReadAsStringAsync();
                    dynamic commitsObj = (JArray)JsonConvert.DeserializeObject(contentComits);

                    var responseTrees = await client.GetAsync(synkPiconConfig.TreeApiUrl(commitsObj[0].commit.sha.Value));

                    if (responseTrees.IsSuccessStatusCode)
                    {
                        var logoChannelsTree = await responseTrees.Content.ReadAsStringAsync();
                        JObject contentTree = JsonConvert.DeserializeObject<JObject>(logoChannelsTree);

                        var responseTree = await client.GetAsync(synkPiconConfig.TreeApiUrl(contentTree["tree"][0]["sha"].Value<string>()));
                        if (responseTree.IsSuccessStatusCode)
                        {
                            var piconsTree = await responseTree.Content.ReadAsStringAsync();
                            var picons = JsonConvert.DeserializeObject<GithubApiResponse>(piconsTree);
                            return picons.Picons;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Synk picons
        /// </summary>
        /// <param name="picons"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IBulkResponse> SynkAsync(IEnumerable<Picon> picons, CancellationToken cancellationToken)
        {
            var descriptor = new BulkDescriptor();
            descriptor.CreateMany(picons.Distinct(new Picon()), (a, o) => a.Document(o).Id(o.Id));
            descriptor.Refresh(Elasticsearch.Net.Refresh.True);

            var response = await _elasticConnectionClient.Client.BulkAsync(descriptor, cancellationToken);

            return response;
        }
    }
}
