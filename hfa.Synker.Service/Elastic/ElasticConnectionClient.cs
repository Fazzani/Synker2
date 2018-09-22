namespace hfa.Synker.Service.Services.Elastic
{
    using Nest;
    using PlaylistBaseLibrary.Entities;
    using PlaylistManager.Entities;
    using System;
    using Microsoft.Extensions.Logging;
    using hfa.Synker.Service.Elastic;
    using hfa.Synker.Service.Services.Xmltv;
    using Microsoft.Extensions.Options;
    using hfa.Synker.Service.Services.Picons;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;

    public class ElasticConnectionClient : IElasticConnectionClient
    {
        //Updated from bash script @see synker-docker repository
        public const string SITE_PACK_PIPELINE = "sitepack_pipeline";
        public const string PICONS_RETREIVE_CHANNEL_NUMBER_PIPELINE = "picons_retreive_channel_number";
        private const string STOP_WORDS_FILE_PATH = "synkerconfig/stopwords.txt";
        private const string MAPPING_CHAR_FILTERS_FILE_PATH = "mapping_synker.txt";
        private const int max_result_window = 1_000_000;
        private ConnectionSettings _settings;
        private ElasticClient _client;
        private static object syncRoot = new Object();
        private ILogger<ElasticConnectionClient> _logger;
        static string[] stopWords = { "tnt", "hd", "sd", "fhd", "tn:", "vip:", "vip", "ca:", "usa:", "ch:", "ar", "fr", "fr:", "ar:", "1080p", "720p", "(fr)", "(ar)", "+1", "+2", "+4", "+6", "+8", "arb", "vip", "it", "my" };
        private ElasticConfig _config;
        static PropertyName keywordProperty = new PropertyName("keyword");

        public ElasticConnectionClient(IOptions<ElasticConfig> config, ILoggerFactory loggerFactory)
        {
            _config = config.Value;
            _logger = loggerFactory.CreateLogger<ElasticConnectionClient>();
        }

        private void Init()
        {
            //SitePack index
            MappingSitePackConfig(_config.SitePackIndex).Wait();

            var pipelineResponse = Client.Value.PutPipeline("default-pipeline", p => p
                .Processors(pr => pr
                    .Set<SitePackChannel>(t => t.Field(f => f.Country).Value("Default"))
                )
            );

            //Create picons channel number pipeline
            var pipeChannelNumber = Client.Value.GetPipeline(r => r.Id(new Id(PICONS_RETREIVE_CHANNEL_NUMBER_PIPELINE)));
            if (pipeChannelNumber.Pipelines == null || pipeChannelNumber.Pipelines.Count == 0)
            {
                var respPipePicons = Client.Value.PutPipeline(new Id(PICONS_RETREIVE_CHANNEL_NUMBER_PIPELINE), f => f
                 .Description("Used for retreiving channel number from picon name and add new field ch_number to store the value into")
                 .Processors(p => p.Script(s => s.Source("if(ctx.name != null) { Matcher m = /(?:[^\\+])(\\d{1,2})/.matcher(ctx.name); if(m!=null && m.find())  { ctx.ch_number = m.group(1); } }"))));

                _logger.LogDebug(respPipePicons.DebugInformation);
            }

            //Picon index
            if (!Client.Value.IndexExists(_config.PiconIndex).Exists)
                MappingPicons(_config.PiconIndex);

            //Client.Value.Map<tv>(x => x.Index("xmltv-*").AutoMap());
        }

        private void InitSettings()
        {
            _settings = new ConnectionSettings(new Uri(_config.ElasticUrl));
            _settings
                // .BasicAuthentication(_config.ElasticUserName, _config.ElasticPassword)
                .DisableDirectStreaming(true)
                .DefaultIndex(_config.DefaultIndex)
                .PrettyJson()
                .RequestTimeout(TimeSpan.FromSeconds(_config.RequestTimeout))
                .EnableHttpCompression();

#if DEBUG
            _settings.EnableDebugMode();
            _settings.OnRequestCompleted(call =>
            {
                if (call.RequestBodyInBytes != null)
                {
                    Debug.WriteLine("======================== REQUEST");
                    Debug.WriteLine(Encoding.UTF8.GetString(call.RequestBodyInBytes));
                    Debug.WriteLine("=================================");
                }
                if (call.ResponseBodyInBytes != null)
                {
                    Debug.WriteLine("======================== RESPONSE");
                    Debug.WriteLine(Encoding.UTF8.GetString(call.ResponseBodyInBytes));
                    Debug.WriteLine("=================================");
                }
            });
#endif
            _settings
                //.InferMappingFor<Message>(m => m.IndexName(_config.MessageIndex).IdProperty(p => p.Id))
                //.DefaultMappingFor<TvgMedia>(m => m.IdProperty(p => p.Id))
                //.DefaultMappingFor<tvChannel>(m => m.IdProperty(p => p.id))
                //.DefaultMappingFor<Tvg>(m => m.IdProperty(p => p.Id))
                //.DefaultMappingFor<tvProgramme>(m => m.IdProperty(p => p.Id).IndexName("xmltv-*"))
                //.DefaultMappingFor<MediaRef>(m => m.IndexName(_config.MediaRefIndex).IdProperty(p => p.Id))
                .DefaultMappingFor<Picon>(m => m.IndexName(_config.PiconIndex).IdProperty(p => p.Id))
                .DefaultMappingFor<SitePackChannel>(m => m.IndexName(_config.SitePackIndex)
                .TypeName("doc")
                .IdProperty(x => x.Id)
                //.PropertyName(x => x.Id, "_id")
                .PropertyName(x => x.Update, "update_date")
                );
        }

        private async Task MappingSitePackConfig(string indexName)
        {
            if (!Client.Value.IndexExists(_config.SitePackIndex).Exists)
            {
                _logger.LogInformation($"Index {indexName} doesn't exist so will be created");

                var response = Client.Value.CreateIndex(indexName, c => c
                              .Settings(s => s
                                       .Setting(nameof(max_result_window), max_result_window)
                                       .Analysis(_sitepackAnalysis))
                              .Mappings(m =>
                                   m.Map<SitePackChannel>(x => x
                                           .Properties(p =>
                                               p.Keyword(t => t.Name(pt => pt.MediaType))
                                                .Keyword(t => t.Name(pt => pt.Site))
                                                .Keyword(t => t.Name(pt => pt.Source))
                                                .Keyword(t => t.Name(pt => pt.Site_id))
                                                .Keyword(t => t.Name(pt => pt.Xmltv_id))
                                                .Date(t => t.Name(pt => pt.Update))
                                                .Keyword(t => t.Name(pt => pt.Country))
                                                .Text(t => t
                                                  .Name(pt => pt.DisplayNames)
                                                  .Fields(f => f.Keyword(k => k.Name(keywordProperty)))
                                                  .Analyzer("sitepack_name_analyzer")
                                                  .SearchAnalyzer("sitepack_name_analyzer"))

                                       ))
                               ));
                _logger.LogDebug(response.DebugInformation);
            }
            else
            {
                var index = await Client.Value.GetIndexAsync(indexName);
                if (!index.Indices[indexName].Settings.ContainsKey(nameof(max_result_window)))
                {
                    var responseClose = await Client.Value.CloseIndexAsync(indexName);
                    if (responseClose.IsValid)
                    {
                        _logger.LogInformation($"Applying settings for index : {indexName}");
                        var indexSettings = new IndexSettings()
                        {
                            Analysis = _sitepackAnalysis(new AnalysisDescriptor())
                        };
                        indexSettings.Add(nameof(max_result_window), max_result_window);
                        var responseSetting = await Client.Value.UpdateIndexSettingsAsync(new UpdateIndexSettingsRequest(indexName) { IndexSettings = indexSettings });

                        if (!responseSetting.IsValid)
                        {
                            _logger.LogError($"{responseSetting.ServerError.Error.ToString()}");
                            if (responseSetting.TryGetServerErrorReason(out string reason))
                            {
                                _logger.LogError($"{reason}");
                            }
                        }

                        await Client.Value.MapAsync<SitePackChannel>(x => x
                                                                      .Properties(p =>
                                                                          p.Keyword(t => t.Name(pt => pt.MediaType))
                                                                           .Keyword(t => t.Name(pt => pt.Site))
                                                                           .Keyword(t => t.Name(pt => pt.Source))
                                                                           .Keyword(t => t.Name(pt => pt.Site_id))
                                                                           .Keyword(t => t.Name(pt => pt.Xmltv_id))
                                                                           .Date(t => t.Name(pt => pt.Update))
                                                                           .Keyword(t => t.Name(pt => pt.Country))
                                                                           .Text(t => t
                                                                             .Name(pt => pt.DisplayNames)
                                                                             .Fields(f => f.Keyword(k => k.Name(keywordProperty)))
                                                                             .Analyzer("sitepack_name_analyzer")
                                                                             .SearchAnalyzer("sitepack_name_analyzer"))
                                                                       ));
                        var responseOpen = await Client.Value.OpenIndexAsync(indexName);
                    }
                }
            }

        }

        Func<AnalysisDescriptor, IAnalysis> _sitepackAnalysis = a => a
                                           .Analyzers(an => an
                                               .Custom("sitepack_name_analyzer", ca => ca
                                                   .Tokenizer("standard")
                                                   .Filters("lowercase", "standard", "asciifolding")
                                               )
                                               .Standard("standard", sd => sd.StopWords(stopWords))
                                           );

        Func<TypeMappingDescriptor<SitePackChannel>, ITypeMapping> _sitepackMapping = x => x
                                            .Properties(p =>
                                                p.Keyword(t => t.Name(pt => pt.MediaType))
                                                 .Keyword(t => t.Name(pt => pt.Site))
                                                 .Keyword(t => t.Name(pt => pt.Source))
                                                 .Keyword(t => t.Name(pt => pt.Site_id))
                                                 .Keyword(t => t.Name(pt => pt.Xmltv_id))
                                                 .Date(t => t.Name(pt => pt.Update))
                                                 .Keyword(t => t.Name(pt => pt.Country))
                                                 .Text(t => t
                                                   .Name(pt => pt.DisplayNames)
                                                   .Fields(f => f.Keyword(k => k.Name(keywordProperty)))
                                                   .Analyzer("sitepack_name_analyzer")
                                                   .SearchAnalyzer("sitepack_name_analyzer"))
                                        );

        public void MappingPicons(string indexName)
        {
            var response = Client.Value.CreateIndex(indexName, c => c
            .Settings(s => s
                     .Setting(nameof(max_result_window), _config.MaxResultWindow)
                     .Analysis(a => a
                     .CharFilters(cf => cf
                             .Mapping("mapping_picons_char_filter", x => new MappingCharFilter() { MappingsPath = MAPPING_CHAR_FILTERS_FILE_PATH })
                             .PatternReplace("picons_name_filter_regex_removeWhiteSpace", pat => pat.Pattern("(?i)^\\s+|\\s+$|\\s+(?=\\s)|\\bf?hd|\\bsd|\\bh265|\\bfull\\s?hd|\\btv|\\b\\d{2,3}0p").Replacement(string.Empty))
                             .PatternReplace("picons_name_filter_regex_quality", pat => pat.Pattern("(?i)\\bf?hd\\b|\\bsd\\b|(\\(|\\s)\\d{2,3}0p\\s?\\)?").Replacement(string.Empty))
                             .PatternReplace("picons_name_filter_regex_shift", pat => pat.Pattern("\\s\\+\\d").Replacement(string.Empty))
                             .PatternReplace("picons_name_filter_regex_replace_plus", pat => pat.Pattern("\\+").Replacement("plus"))
                         )
                         .TokenFilters(t => t.WordDelimiter("piconWordDelimiter", wd => wd.CatenateAll(true).PreserveOriginal(false).CatenateWords(true).SplitOnCaseChange(false).SplitOnNumerics(false).GenerateWordParts(false)))
                         .Analyzers(an => an
                             .Custom("picons_name_analyzer", ca => ca
                                 .Tokenizer("keyword")
                                 .CharFilters("mapping_picons_char_filter", "picons_name_filter_regex_quality", "picons_name_filter_regex_shift", "picons_name_filter_regex_replace_plus")
                                 .Filters("lowercase", "standard", "piconWordDelimiter", "asciifolding")
                             ).Custom("picons_search_name_analyzer", ca => ca
                                 .Tokenizer("keyword")
                                 .CharFilters("mapping_picons_char_filter", "picons_name_filter_regex_quality", "picons_name_filter_regex_shift", "picons_name_filter_regex_replace_plus")
                                 .Filters("lowercase", "standard", "piconWordDelimiter", "asciifolding")
                             )
                             .Custom("mediaref_name_analyzer", ca => ca
                                 .Tokenizer("standard")
                                 .Filters("lowercase", "standard", "asciifolding")
                             )
                             .Standard("standard", sd => sd.StopWords(stopWords))
                         )
                     )
                 )
            .Mappings(m =>
                 m.Map<Picon>(x => x.Properties(p =>
                                p.Keyword(t => t.Name(pt => pt.Path))
                                 .Keyword(t => t.Name(pt => pt.RawUrl))
                                 .Keyword(t => t.Name(pt => pt.Url))
                                 .Text(t => t
                                  .Name(pt => pt.Name)
                                  .Fields(f => f.Keyword(k => k.Name(keywordProperty)))
                                  .Analyzer("picons_name_analyzer")
                                  .SearchAnalyzer("picons_name_analyzer"))))
                 ));

            _logger.LogDebug(response.DebugInformation);
        }

        public void MappingPlaylistConfig()
        {
            var response = Client.Value.CreateIndex(_config.DefaultIndex, c => c
            .Settings(s => s
            .Setting(nameof(max_result_window), max_result_window)
            .Setting("analysis.char_filter.drop_specChars.type", "pattern_replace")
            .Setting("analysis.char_filter.drop_specChars.pattern", "\\bbeinsports?\\b")
            .Setting("analysis.char_filter.drop_specChars.replacement", "beIN sports")
            .Setting("analysis.char_filter.drop_specChars.flags", "CASE_INSENSITIVE|COMMENTS")
                     .Analysis(a => a
                         .CharFilters(cf => cf
                             .Mapping("channel_name_filter", mca => mca
                                 .Mappings(new[]
                                 {
                                    "low => \\u0020",
                                    "sd => \\u0020",
                                    "+2 => \\u0020",
                                    "+1 => \\u0020",
                                    "+4 => \\u0020",
                                    "+6 => \\u0020",
                                    "+8 => \\u0020",
                                    "(fr) => fr",
                                    "1080p fhd => FHD",
                                    "720p hd => HD",
                                    "arb, (arb) => ar"
                                 })
                             ).PatternReplace("channel_name_filter_regex", pat => pat.Pattern("^(.+):(.+)$").Replacement("$2"))
                         )
                         .TokenFilters(t => t.Stop("channel_name_token_filter", ss => ss.StopWordsPath(STOP_WORDS_FILE_PATH)))
                         .Analyzers(an => an
                             .Custom("channel_name_analyzer", ca => ca
                                 .CharFilters("channel_name_filter", "drop_specChars", "channel_name_filter_regex")
                                 .Tokenizer("standard")
                                 .Filters("lowercase", "standard", "channel_name_token_filter")

                             )
                             .Standard("standard", sd => sd.StopWords(stopWords))
                         )
                     )
                 )
            .Mappings(m =>
                 m.Map<TvgMedia>(x => x
                         .Properties(p =>
                             p.Keyword(t => t.Name(pt => pt.Url))
                             .Text(t => t
                                 .Name(pt => pt.Name)
                                 .Fields(f => f.Keyword(k => k.Name(keywordProperty)))
                                 .Analyzer("standard")
                                 .SearchAnalyzer("channel_name_analyzer"))
                             .Object<Tvg>(t => t.Name(n => n.Tvg).Properties(pt => pt.Keyword(k => k.Name(km => km.TvgIdentify))
                                        .Text(tx => tx.Name(txn => txn.Name).Fields(f => f.Keyword(k => k.Name(keywordProperty))))))
                     ))
                 .Map<tvChannel>(x => x
                     .Properties(p =>
                         p.Keyword(t => t.Name(pt => pt.id))
                          .Text(t => t
                             .Name(pt => pt.displayname)
                             .Fields(f => f.Keyword(k => k.Name(keywordProperty)))
                             .Analyzer("standard")
                             .SearchAnalyzer("channel_name_analyzer"))

                         )
                     )));

            //response.AssertElasticResponse();
        }

        public void DeleteDefaultIndex()
        {
            var indexExistsResponse = Client.Value.IndexExists(_config.DefaultIndex);
            if (indexExistsResponse.Exists)
                Client.Value.DeleteIndex(_config.DefaultIndex, null);
        }

        public Lazy<ElasticClient> Client
        {
            get
            {
                if (_client == null)
                {
                    lock (syncRoot)
                    {
                        if (_client == null)
                        {
                            InitSettings();
                            _client = new ElasticClient(_settings);
                            Init();
                        }

                    }
                }
                return new Lazy<ElasticClient>(() => _client);
            }
        }

        public ElasticConfig ElasticConfig => _config;
    }
}
