using hfa.Synker.Service.Services.Elastic;
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
        IElasticConnectionClient _elasticClient;
        public TvgMediaEpgMatcherNameHandler(IContextTvgMediaHandler contextTvgMediaHandler, IElasticConnectionClient elasticClient) : base(contextTvgMediaHandler)
        {
            _elasticClient = elasticClient;
        }

        //TODO: Passer par l'index filebeat // log_webGrabber
        public override void HandleTvgMedia(TvgMedia tvgMedia)
        {
            var result = _elasticClient.Client.SearchAsync<tvChannel>(x => x.From(0).Size(1).Query(q => q.Match(m => m.Field(f => f.displayname).Query(tvgMedia.Name)))).GetAwaiter().GetResult();
            //var result = ElasticConnectionClient.Client.SearchAsync<tvChannel>(x => x.Query(q => q.Fuzzy(m => m.Field(f => f.displayname).Fuzziness(Nest.Fuzziness.EditDistance(2)).Value(tvgMedia.Name)))).GetAwaiter().GetResult();
            if (result.Documents.Any())
            {
                tvgMedia.Tvg.Id = result.Documents.FirstOrDefault().id;
                tvgMedia.Tvg.Name = result.Documents.FirstOrDefault().displayname.FirstOrDefault();
                tvgMedia.Tvg.TvgIdentify = result.Documents.FirstOrDefault().id;
                tvgMedia.Tvg.Logo = result.Documents.FirstOrDefault().icon?.src;
            }
            if (_successor != null)
                _successor.HandleTvgMedia(tvgMedia);
        }
    }
}
