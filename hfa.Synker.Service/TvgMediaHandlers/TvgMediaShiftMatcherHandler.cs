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
    public class TvgMediaShiftMatcherHandler : TvgMediaHandler
    {
        public TvgMediaShiftMatcherHandler(IContextTvgMediaHandler contextTvgMediaHandler) : base(contextTvgMediaHandler)
        {

        }

        public override void HandleTvgMedia(TvgMedia tvgMedia)
        {
            var reg = new Regex(@"(?<channelName>[a-z\s\d{0,1}]+)(?<shift>\s\+\d{1})", RegexOptions.IgnoreCase);
            var match = reg.Match(tvgMedia.Name);
            if (match.Success && match.Groups["shift"] != null  && !string.IsNullOrEmpty(match.Groups["shift"].Value))
            {
                if (tvgMedia.Tvg == null)
                    tvgMedia.Tvg = new Tvg();
                tvgMedia.Tvg.Shift = match.Groups["shift"].Value.Trim();
            }

            if (_successor != null)
                _successor.HandleTvgMedia(tvgMedia);
        }
    }
}
