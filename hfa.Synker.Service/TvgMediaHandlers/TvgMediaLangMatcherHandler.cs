using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using PlaylistBaseLibrary.ChannelHandlers;

namespace hfa.Synker.Service.Services.TvgMediaHandlers
{
    public class TvgMediaLangMatcherHandler : TvgMediaHandler
    {
        public TvgMediaLangMatcherHandler(IContextTvgMediaHandler contextTvgMediaHandler) : base(contextTvgMediaHandler)
        {

        }

        public override void HandleTvgMedia(TvgMedia tvgMedia)
        {
            if (_contextTvgMediaHandler is ContextTvgMediaHandler context)
            {
                var reg = new Regex(context.MediaConfiguration.StartChannelsHeadLinePattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                if (reg.IsMatch(tvgMedia.Name) && !reg.Match(tvgMedia.Name).Groups[1].Value.Equals(tvgMedia.Name))
                {
                    context.StartChannelsHeadLineLastMatched = reg.Match(tvgMedia.Name).Groups[1].Value;
                    tvgMedia.IsValid = false;
                }
                else
                {
                    if (!string.IsNullOrEmpty(context.StartChannelsHeadLineLastMatched))
                    {
                        tvgMedia.MediaGroup = new PlaylistBaseLibrary.Entities.MediaGroup(context.StartChannelsHeadLineLastMatched);
                        tvgMedia.Tags.Add($"{{StartChannelsHeadLineLastMatched:{context.StartChannelsHeadLineLastMatched}}}");
                    }
                }
            }

            if (_successor != null)
                _successor.HandleTvgMedia(tvgMedia);
        }
    }
}
