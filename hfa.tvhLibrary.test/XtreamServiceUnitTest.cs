using hfa.Synker.Service.Services.Xtream;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace hfa.tvhLibrary.test
{
    public class XtreamServiceUnitTest
    {
        [Theory(DisplayName = "Test Get Player INFO")]
        [InlineData("player_api", XtreamService.XtreamApiEnum.Player_Api)]
        internal void GetPlayerTest(string path_api, XtreamService.XtreamApiEnum xtreamApiEnum)
        {
            var service = new XtreamService();
            string server = "dtt.tv";
            string port = "8000";
            string username = "heni";
            string password = "fazz";
            var extraParams = "&type=m3u&output=mpegts";
            var player = service.GetApiUrl($"http://{server}:{port}/get.php?username={username}&password={password}{extraParams}", xtreamApiEnum);

            Assert.True(!string.IsNullOrEmpty(player));
            Assert.Equal(player, $"http://{server}:{port}/{path_api}.php?username={username}&password={password}");
        }
    }
}
