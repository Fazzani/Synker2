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
            if (match.Success && match.Groups["lang"] != null  && !string.IsNullOrEmpty(match.Groups["lang"].Value))
            {
                tvgMedia.Lang = getLang(match.Groups["lang"].Value);
            }

            if (_successor != null)
                _successor.HandleTvgMedia(tvgMedia);
        }


        string getLang(string name)
        {
            var lang = string.Empty;
            try
            {
                var region = new RegionInfo(name);
                if (region != null)
                    lang = region.DisplayName;
            }
            catch (Exception)
            {
            }
            finally
            {
                try
                {
                    var cul = new CultureInfo(name);
                    if (cul != null)
                        lang = cul.DisplayName;
                }
                catch (Exception)
                {
                }
            }
            return lang;
        }
    }
}
