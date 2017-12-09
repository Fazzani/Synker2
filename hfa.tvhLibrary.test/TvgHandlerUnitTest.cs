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
        [Theory(DisplayName = "TvgCultureIsSet")]
        [InlineData("TN: TUNISIE NAT 1")]
        [InlineData("TN: TUNISIE NAT 1 +2")]
        [InlineData("AR: MBC DRAMA +2")]
        [InlineData("SE: TV12 FHD")]
        [InlineData("SE: C More Golf HD")]
        [InlineData("UK: BT Sports 2 FHD")]
        [InlineData("US-CA: Nfl Network")]
        [InlineData("fr:canal+ HD")]
        [InlineData("Sky Sports Football UK")]
        [InlineData("Fox Sports 5 HD NL")]
        public async Task TvgCultureIsSetTest(string channelName)
        {
            var handler = new TvgMediaCultureMatcherHandler(null);
            var tvgMedia = new TvgMedia
            {
                Name = channelName
            };

            handler.HandleTvgMedia(tvgMedia);
            Assert.True(tvgMedia.Lang != null);
        }

        [Theory(DisplayName = "TvgCultureTestIsNotset")]
        [InlineData("Bein Sports")]
        [InlineData("Bein Sports hd")]
        [InlineData("Bein Sports fhd")]
        [InlineData("Bein Sports 1080P")]
        [InlineData("MyHD: Rotana Aflam HD")]
        [InlineData("OSN VIP: FUEL HD ( 1080P )")]
        [InlineData("VOD FR: Canal Play Live (Rupture pour tous)")]
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
