using PlaylistBaseLibrary.ChannelHandlers;
using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Hfa.PlaylistBaseLibrary.Entities;
using Nest;
using hfa.Synker.Service.Services.Elastic;

namespace hfa.Synker.Service.Services.TvgMediaHandlers
{
    public class ContextTvgMediaHandler : IContextTvgMediaHandler
    {
        public ContextTvgMediaHandler(IElasticConnectionClient elasticConnectionClient)
        {
            LastLang = "Ar";

            var responseMediaConfig = elasticConnectionClient.Client.SearchAsync<MediaConfiguration>(x => x.From(0).Size(1)).GetAwaiter().GetResult();
            if (responseMediaConfig.Documents.Any())
                MediaConfiguration = responseMediaConfig.Documents.FirstOrDefault();
        }

        public string LastLang { get; set; }
        public string StartChannelsHeadLineLastMatched { get; set; }
        public MediaConfiguration MediaConfiguration { get; internal set; }
    }


    public class StartChannelsHeadLinePattern
    {
        public string Pattern { get; set; }
        public TvgMedia Media { get; set; }
    }
}
