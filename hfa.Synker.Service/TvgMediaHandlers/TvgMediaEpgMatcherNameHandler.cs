using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Entities.MediasRef;
using hfa.Synker.Service.Services.Elastic;
using Microsoft.Extensions.Options;
using PlaylistBaseLibrary.ChannelHandlers;
using PlaylistBaseLibrary.Entities;
using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace hfa.Synker.Service.Services.TvgMediaHandlers
{
    public class TvgMediaEpgMatcherNameHandler : TvgMediaHandler
    {
        private IElasticConnectionClient _elasticClient;
        private ElasticConfig _elasticConfig;

        public TvgMediaEpgMatcherNameHandler(IContextTvgMediaHandler contextTvgMediaHandler, IElasticConnectionClient elasticClient,
            IOptions<ElasticConfig> elasticConfig) : base(contextTvgMediaHandler)
        {
            _elasticClient = elasticClient;
            _elasticConfig = elasticConfig.Value;
        }

        //TODO: Passer par l'index filebeat // log_webGrabber
        public override void HandleTvgMedia(TvgMedia tvgMedia)
        {
            var result = _elasticClient.Client.Value.Search<MediaRef>(x => x.Index(_elasticConfig.MediaRefIndex).From(0).Size(1)
            .Query(q => q.Match(m => m.Field(f => f.DisplayNames).Query(tvgMedia.Name))));
            //var result = ElasticConnectionClient.Client.SearchAsync<tvChannel>(x => x.Query(q => q.Fuzzy(m => m.Field(f => f.displayname).Fuzziness(Nest.Fuzziness.EditDistance(2)).Value(tvgMedia.Name)))).GetAwaiter().GetResult();
            if (result.Documents.Any())
            {
                tvgMedia.Tvg = result.Documents.FirstOrDefault().Tvg;
                tvgMedia.MediaGroup = new PlaylistBaseLibrary.Entities.MediaGroup(result.Documents.FirstOrDefault().Groups.FirstOrDefault());
                tvgMedia.Lang = result.Documents.FirstOrDefault().Cultures.FirstOrDefault();
            }
            if (_successor != null)
                _successor.HandleTvgMedia(tvgMedia);
        }
    }
}
