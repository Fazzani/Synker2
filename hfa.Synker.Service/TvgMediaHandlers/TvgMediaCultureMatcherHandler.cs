namespace hfa.Synker.Service.Services.TvgMediaHandlers
{
    using PlaylistManager.Entities;
    using System;
    using System.Text.RegularExpressions;
    using System.Linq;
    using System.Globalization;
    using hfa.PlaylistBaseLibrary.ChannelHandlers;

    public class TvgMediaCultureMatcherHandler : TvgMediaHandler
    {
        public const string FallBackCountry = "International";

        public TvgMediaCultureMatcherHandler(IContextTvgMediaHandler contextTvgMediaHandler) : base(contextTvgMediaHandler)
        {
            
        }

        public override void HandleTvgMedia(TvgMedia tvgMedia)
        {
            var reg = new Regex("\\b(?<lang>(?!(f?h|s)d)(u|a)?\\w{2}(-\\w{2})?)\\b", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            var match = reg.Match(tvgMedia.Name);
            if (match.Success && match.Groups["lang"] != null && !string.IsNullOrEmpty(match.Groups["lang"].Value))
            {
                var lang = match.Groups["lang"].Value.Trim();
                lang = FixLang(lang);

                var (EnglishName, ThreeLetterISOLanguageName) = Common.TryGet(x =>
                {
                    var cul = new CultureInfo(x);
                    if (cul != null && !cul.EnglishName.StartsWith("Unknown Language"))
                        return (cul.EnglishName, cul.ThreeLetterISOLanguageName);
                    return (null, default);
                }, lang);

                var regionInfo = Common.TryGet(x => new RegionInfo(x)?.EnglishName, lang);
                if (EnglishName != null && !string.IsNullOrEmpty(regionInfo))
                {
                    tvgMedia.Culture.Country = regionInfo;
                    tvgMedia.Culture.Code = EnglishName;
                }

                tvgMedia.Lang = tvgMedia.Culture.Country ?? tvgMedia.Culture.Code ?? regionInfo;
                tvgMedia.Culture.Country = tvgMedia.Culture.Country ?? regionInfo ?? FallBackCountry;

                if (tvgMedia.Tvg == null)
                    tvgMedia.Tvg = new Tvg();
                if (tvgMedia.Tvg.TvgSource == null)
                    tvgMedia.Tvg.TvgSource = new PlaylistBaseLibrary.Entities.TvgSource();

                tvgMedia.Tvg.TvgSource.Country = tvgMedia.Culture.Country;
                tvgMedia.Tvg.TvgSource.Code = tvgMedia.Culture.Code;

                if (tvgMedia.Lang != null && tvgMedia.Tags != null && !tvgMedia.Tags.Any(x => x.Equals(tvgMedia.Lang)))
                    tvgMedia.Tags.Add(tvgMedia.Lang);
            }
            else
            {
                tvgMedia.Culture.Country = FallBackCountry;
                tvgMedia.Tags.Add(FallBackCountry);
            }

            if (_successor != null)
                _successor.HandleTvgMedia(tvgMedia);
        }

        private static string FixLang(string lang)
        {
            if (lang.Equals("uk", StringComparison.InvariantCultureIgnoreCase))
                return "gb";
            if (lang.Equals("ar", StringComparison.InvariantCultureIgnoreCase))
                return "ar-EG";
            if (lang.Equals("tn", StringComparison.InvariantCultureIgnoreCase))
                return "ar-tn";
            if (lang.Equals("us-ca", StringComparison.InvariantCultureIgnoreCase))
                return "fr-ca";
            return lang;
        }
    }
}
