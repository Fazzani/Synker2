﻿using Nest;
using PlaylistBaseLibrary.Entities;
using PlaylistManager.Entities;
using hfa.SyncLibrary.Global;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Hfa.SyncLibrary.Infrastructure;

namespace hfa.SyncLibrary.Common
{
    internal class ElasticConnectionClient : IElasticConnectionClient
    {
        private ConnectionSettings _settings;
        private ElasticClient _client;
        private static object syncRoot = new Object();
        IOptions<ApplicationConfigData> _config;
        ILoggerFactory _loggerFactory;
        static string[] stopWords = { "hd", "sd", "fhd", "ar", "fr", "fr:", "ar:", "1080p", "720p", "(fr)", "(ar)", "+1", "+2", "+4", "+6", "+8", "arb", "vip" };

        public ElasticConnectionClient(IOptions<ApplicationConfigData> config, ILoggerFactory loggerFactory)
        {
            _config = config;
            _loggerFactory = loggerFactory;

            _settings = new ConnectionSettings(new Uri(config.Value.ElasticUrl));
            _settings
                .BasicAuthentication(config.Value.ElasticUserName, config.Value.ElasticPassword)
                .DisableDirectStreaming(true)
                .DefaultIndex(config.Value.DefaultIndex)
                .PrettyJson()
                .RequestTimeout(TimeSpan.FromMinutes(2))
                .EnableHttpCompression();

#if DEBUG
            _settings.EnableDebugMode();
#endif

            _settings
                .InferMappingFor<TvgMedia>(m => m.IdProperty(p => p.Id))
                .InferMappingFor<tvChannel>(m => m.IdProperty(p => p.id))
                .InferMappingFor<Tvg>(m => m.IdProperty(p => p.Id));
        }

        public void MappingConfig(IOptions<ApplicationConfigData> config)
        {
            var keywordProperty = new PropertyName("keyword");

            var response = Client.CreateIndex(config.Value.DefaultIndex, c => c
            .Settings(s => s
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
                         .TokenFilters(t => t.Stop("channel_name_token_filter", ss => ss.StopWords(stopWords)))
                         .Analyzers(an => an
                             .Custom("channel_name_analyzer", ca => ca
                                 .CharFilters("channel_name_filter", "drop_specChars")
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

            response.AssertElasticResponse();
        }

        public void DeleteDefaultIndex()
        {
            var indexExistsResponse = Client.IndexExists(_config.Value.DefaultIndex);
            if (indexExistsResponse.Exists)
                Client.DeleteIndex(_config.Value.DefaultIndex, null);
        }

        public ElasticClient Client
        {
            get
            {
                if (_client == null)
                {
                    lock (syncRoot)
                    {
                        _client = new ElasticClient(_settings);
                    }
                }
                return _client;
            }
        }
    }
}