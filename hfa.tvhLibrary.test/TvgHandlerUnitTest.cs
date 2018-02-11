using hfa.Synker.Service.Services.Picons;
using hfa.Synker.Service.Services.TvgMediaHandlers;
using PlaylistManager.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace hfa.tvhLibrary.test
{
    public class TvgHandlerUnitTest
    {
        [Trait("Category", "TvgCulture")]
        [Theory(DisplayName = "TvgCultureIsSet")]
        [InlineData("TN: TUNISIE NAT 1", "Tunisia")]
        [InlineData("TN: TUNISIE NAT 1 +2", "Tunisia")]
        [InlineData("AR: MBC DRAMA +2", "Egypt")]
        [InlineData("SE: TV12 FHD", "Sweden")]
        [InlineData("SE: C More Golf HD", "Sweden")]
        [InlineData("UK: BT Sports 2 FHD", "United Kingdom")]
        [InlineData("US-CA: Nfl Network", "Canada")]
        [InlineData("fr:canal+ HD", "France")]
        [InlineData("Sky Sports Football UK", "United Kingdom")]
        [InlineData("Fox Sports 5 HD NL", "Netherlands")]
        [InlineData("beIN SPORTS ES", "Spain")]
        [InlineData("beIN SPORTS 1 FR HD", "France")]
        [InlineData("beIN SPORTS ES FHD", "Spain")]
        [InlineData("VOD FR: Canal Play Live (Rupture pour tous)", "France")]
        public async Task TvgCultureIsSetTest(string channelName, string lang = "")
        {
            var handler = new TvgMediaCultureMatcherHandler(null);
            var tvgMedia = new TvgMedia
            {
                Name = channelName
            };

            handler.HandleTvgMedia(tvgMedia);
            Assert.True(tvgMedia.Lang != null);
            if (!string.IsNullOrEmpty(lang))
                Assert.Equal(tvgMedia.Lang, lang);
        }

        [Trait("Category", "TvgCulture")]
        [Theory(DisplayName = "TvgCultureTestIsNotset")]
        [InlineData("Bein Sports")]
        [InlineData("Bein Sports hd")]
        [InlineData("Bein Sports sd")]
        [InlineData("Bein Sports fhd")]
        [InlineData("Bein Sports 1080P")]
        [InlineData("MyHD: Rotana Aflam HD")]
        [InlineData("OSN VIP: FUEL HD ( 1080P )")]
        public async Task TvgCultureIsNotSetTest(string channelName)
        {
            var handler = new TvgMediaCultureMatcherHandler(null);
            var tvgMedia = new TvgMedia
            {
                Name = channelName,
                Lang = string.Empty
            };

            handler.HandleTvgMedia(tvgMedia);
            Assert.True(string.IsNullOrEmpty(tvgMedia.Lang));
        }

        [Trait("Category", "TvgShift")]
        [Theory(DisplayName = "TvgShiftIsSet")]
        [InlineData("TN: TUNISIE NAT 1 +2 fhd")]
        [InlineData("AR: MBC DRAMA +2")]
        [InlineData("AR: MBC DRAMA +8")]
        public async Task TvgShiftIsSetTest(string channelName)
        {
            var handler = new TvgMediaShiftMatcherHandler(null);
            var tvgMedia = new TvgMedia
            {
                Name = channelName
            };

            handler.HandleTvgMedia(tvgMedia);
            Assert.True(!string.IsNullOrEmpty(tvgMedia.Tvg.Shift));
            Assert.True(tvgMedia.Tvg.Shift.StartsWith('+'));
        }
    }
}
