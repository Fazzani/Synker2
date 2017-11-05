﻿using Nest;
using PlaylistBaseLibrary.Entities;
using PlaylistManager.Entities;
using System;
using Microsoft.Extensions.Logging;
using hfa.Synker.Services.Entities.Messages;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Services.Xmltv;
using Microsoft.Extensions.Options;
using hfa.Synker.Service.Entities.MediasRef;

namespace hfa.Synker.Service.Services.Elastic
{
    public class ElasticConnectionClient : IElasticConnectionClient
    {
        // internal const string SitePackIndexName = "sitepack";
        
        private ConnectionSettings _settings;
        private ElasticClient _client;
        private static object syncRoot = new Object();
        ILoggerFactory _loggerFactory;
        static string[] stopWords = { "hd", "sd", "fhd", "ar", "fr", "fr:", "ar:", "1080p", "720p", "(fr)", "(ar)", "+1", "+2", "+4", "+6", "+8", "arb", "vip" };
        private ElasticConfig _config;

        public ElasticConnectionClient(IOptions<ElasticConfig> config, ILoggerFactory loggerFactory)
        {
            _config = config.Value;
            _loggerFactory = loggerFactory;

            _settings = new ConnectionSettings(new Uri(_config.ElasticUrl));
            _settings
                .BasicAuthentication(_config.ElasticUserName, _config.ElasticPassword)
                .DisableDirectStreaming(true)
                .DefaultIndex(_config.DefaultIndex)
                .PrettyJson()
                .RequestTimeout(TimeSpan.FromSeconds(_config.RequestTimeout))
                .EnableHttpCompression();

#if DEBUG
            _settings.EnableDebugMode();
#endif
            _settings
                //.InferMappingFor<Message>(m => m.IndexName(_config.MessageIndex).IdProperty(p => p.Id))
                .InferMappingFor<TvgMedia>(m => m.IdProperty(p => p.Id))
                .InferMappingFor<tvChannel>(m => m.IdProperty(p => p.id))
                .InferMappingFor<Tvg>(m => m.IdProperty(p => p.Id))
                .InferMappingFor<tvProgramme>(m => m.IdProperty(p => p.Id).IndexName("xmltv-*"))
                .InferMappingFor<MediaRef>(m => m.IndexName(_config.MediaRefIndex))
                .InferMappingFor<SitePackChannel>(m => m.IndexName(_config.SitePackIndex).IdProperty(p => p.id));

            if (!Client.IndexExists(_config.DefaultIndex).Exists)
                MappingPlaylistConfig();
            if (!Client.IndexExists(_config.MediaRefIndex).Exists)
                MappingMediaRefConfig(_config.MediaRefIndex);

            Client.Map<tv>(x => x.Index("xmltv-*").AutoMap());
            //Client.Map<tvProgrammeAudio>(x => x.AutoMap());
            //Client.Map<tvProgrammeSubtitles>(x => x.AutoMap());
            //Client.Map<tvProgrammeSubtitles>(x => x.AutoMap());
            //Client.Map<tvProgrammeSubtitles>(x => x.AutoMap());
        }

        public void MappingMediaRefConfig(string indexName)
        {
            var keywordProperty = new PropertyName("keyword");
            var response = Client.CreateIndex(indexName, c => c
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
                 m.Map<MediaRef>(x => x
                         .Properties(p =>
                             p.Keyword(t => t.Name(pt => pt.MediaType))
                              .Text(t => t
                                .Name(pt => pt.DisplayNames)
                                .Fields(f => f.Keyword(k => k.Name(keywordProperty)))
                                .Analyzer("standard")
                                .SearchAnalyzer("channel_name_analyzer"))
                             .Object<Tvg>(t => t.Name(n => n.Tvg).Properties(pt => pt.Keyword(k => k.Name(km => km.TvgIdentify))
                                        .Text(tx => tx.Name(txn => txn.Name).Fields(f => f.Keyword(k => k.Name(keywordProperty))))))
                     ))
                 ));
        }

        public void MappingPlaylistConfig()
        {
            var keywordProperty = new PropertyName("keyword");
            var response = Client.CreateIndex(_config.DefaultIndex, c => c
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

            //response.AssertElasticResponse();
        }

        public void DeleteDefaultIndex()
        {
            var indexExistsResponse = Client.IndexExists(_config.DefaultIndex);
            if (indexExistsResponse.Exists)
                Client.DeleteIndex(_config.DefaultIndex, null);
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

        public ElasticConfig ElasticConfig => _config;
    }
}
