using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using PlaylistBaseLibrary.ChannelHandlers;
using System.Globalization;

namespace hfa.Synker.Service.Services.TvgMediaHandlers
{
    public class TvgMediaCultureMatcherHandler : TvgMediaHandler
    {
        public TvgMediaCultureMatcherHandler(IContextTvgMediaHandler contextTvgMediaHandler) : base(contextTvgMediaHandler)
        {

        }

        public override void HandleTvgMedia(TvgMedia tvgMedia)
        {
            var reg = new Regex("^(?:(?<lang>.+):)|(?:f?hd|\\d{3,4}p(?<lang>\\s\\w{2,3}))$", RegexOptions.IgnoreCase);
            var match = reg.Match(tvgMedia.Name);
            if (match.Success && match.Groups["lang"] != null && !string.IsNullOrEmpty(match.Groups["lang"].Value))
            {
                tvgMedia.Culture.Code = Common.TryGet(x =>
                {
                    var cul = new CultureInfo(x);
                    if (cul != null && !cul.EnglishName.StartsWith("Unknown Language"))
                        return cul.DisplayName;
                    return null;
                }, match.Groups["lang"].Value);
                tvgMedia.Culture.Country = Common.TryGet(x => new RegionInfo(x)?.DisplayName, match.Groups["lang"].Value);
                tvgMedia.Lang = tvgMedia.Culture.Country ?? tvgMedia.Culture.Code;

                if (tvgMedia.Lang != null)
                    tvgMedia.Tags.Add(tvgMedia.Lang);
            }

            if (_successor != null)
                _successor.HandleTvgMedia(tvgMedia);
        }
    }
}
