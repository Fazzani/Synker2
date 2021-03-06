﻿namespace hfa.Synker.Service.Services.TvgMediaHandlers
{
    using PlaylistManager.Entities;
    using System.Collections.Generic;
    using System.Linq;
    using Hfa.PlaylistBaseLibrary.Entities;
    using hfa.Synker.Service.Services.Elastic;
    using hfa.PlaylistBaseLibrary.ChannelHandlers;

    public class ContextTvgMediaHandler : IContextTvgMediaHandler
    {
        private MediaConfiguration _mediaConfig;
        private IElasticConnectionClient _elasticConnectionClient;

        public ContextTvgMediaHandler(IElasticConnectionClient elasticConnectionClient)
        {
            LastLang = "Ar";
            _elasticConnectionClient = elasticConnectionClient;
        }

        public string LastLang { get; set; }

        public string StartChannelsHeadLineLastMatched { get; set; }

        public MediaConfiguration MediaConfiguration
        {
            get
            {
                if (_mediaConfig == null)
                {
                    var responseMediaConfig = _elasticConnectionClient.Client.Value.SearchAsync<MediaConfiguration>(x => x.From(0).Size(1)).GetAwaiter().GetResult();
                    if (responseMediaConfig.Documents.Any())
                        _mediaConfig = responseMediaConfig.Documents.FirstOrDefault();
                    else
                        _mediaConfig = new MediaConfiguration();
                }
                return _mediaConfig;
            }
        }

        public IList<FixChannelName> FixChannelNames
        {
            get
            {
                return new List<FixChannelName> { new FixChannelName { Order = 0, Pattern = @"(.*)(?::|\||\])(.*)", ReplaceBy = "$2" } };
            }
        }

    }

    public class StartChannelsHeadLinePattern
    {
        public string Pattern { get; set; }
        public TvgMedia Media { get; set; }
    }
}
