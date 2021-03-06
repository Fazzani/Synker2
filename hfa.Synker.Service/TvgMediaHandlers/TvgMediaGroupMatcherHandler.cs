﻿
namespace hfa.Synker.Service.Services.TvgMediaHandlers
{
    using hfa.PlaylistBaseLibrary.ChannelHandlers;
    using PlaylistManager.Entities;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    public class TvgMediaGroupMatcherHandler : TvgMediaHandler
    {
        public TvgMediaGroupMatcherHandler(IContextTvgMediaHandler contextTvgMediaHandler) : base(contextTvgMediaHandler)
        {
        }

        /// <summary>
        /// Update Group and Tags selon la config sauvegardée dans l'elastic 
        /// </summary>
        /// <param name="tvgMedia"></param>
        public override void HandleTvgMedia(TvgMedia tvgMedia)
        {
            if (_contextTvgMediaHandler is ContextTvgMediaHandler context)
            {
                foreach (var tag in context.MediaConfiguration.Tags.OrderByDescending(x => x.Position))
                {
                    if (tag.ChannelPatternMatcher != null)
                        foreach (var matcher in tag.ChannelPatternMatcher)
                        {
                            var reg = new Regex(matcher, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                            if (reg.IsMatch(tvgMedia.Name) && tag.Lang != null && tag.Lang.Any())
                            {
                                tvgMedia.Lang = tag.Lang.FirstOrDefault();
                                if (tvgMedia.Tags == null)
                                    tvgMedia.Tags = new List<string>();
                                tvgMedia.Tags.Add(tag.Name);
                                break;
                            }
                        }
                }
            }
            if (_successor != null)
                _successor.HandleTvgMedia(tvgMedia);
        }
    }
}
