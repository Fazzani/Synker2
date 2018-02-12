//using hfa.PlaylistBaseLibrary.Entities.XtreamCode;
//using hfa.PlaylistBaseLibrary.Providers;
//using hfa.Synker.Service.Entities.Playlists;
//using hfa.Synker.Service.Services.Playlists;
//using hfa.Synker.Service.Services.Xtream;
//using PlaylistManager.Entities;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Xunit;
//using System.Linq;
//using Newtonsoft.Json;
//using Microsoft.Extensions.Logging;
//using Moq;

//namespace hfa.tvhLibrary.test
//{
//    public class XtreamServiceUnitTest
//    {
//        ILoggerFactory _logger;

//        public XtreamServiceUnitTest()
//        {
//            var mock = new Mock<ILoggerFactory>();
//            _logger = mock.Object;
//        }

//        [Theory(DisplayName = "Test Get Player INFO")]
//        [InlineData("player_api", XtreamService.XtreamApiEnum.Player_Api)]
//        internal void GetPlayerTest(string path_api, XtreamService.XtreamApiEnum xtreamApiEnum)
//        {
//            var service = new XtreamService();
//            string server = "dtt.tv";
//            string port = "8000";
//            string username = "heni";
//            string password = "fazz";
//            var extraParams = "&type=m3u&output=mpegts";
//            var player = service.GetApiUrl($"http://{server}:{port}/get.php?username={username}&password={password}{extraParams}", xtreamApiEnum);

//            Assert.True(!string.IsNullOrEmpty(player));
//            Assert.Equal(player, $"http://{server}:{port}/{path_api}.php?username={username}&password={password}");
//        }

//        //[Fact(DisplayName = "diff playlist from Xtream provider")]
//        //public async Task DiffWithSourceXtreamProviderTest()
//        //{
//        //    var password = "password";
//        //    var username = "username";
//        //    var server = "test.tv";
//        //    var port = "8080";
//        //    const string addedMedia = "test7";

//        //    var service = new PlaylistService(null, null, null, _logger, null);

//        //    var medias = new List<TvgMedia> {
//        //        new TvgMedia { Name ="test1", Position= 1, Group="Group1", IsValid = true, MediaType = MediaType.LiveTv, Enabled = true, Url=$"http://{server}:{port}/live/{username}/{password}/1.ts" },
//        //        new TvgMedia { Name ="test2", Position= 2, Group="Group1", IsValid = true, MediaType = MediaType.LiveTv, Enabled = true, Url=$"http://{server}:{port}/live/{username}/{password}/2.ts" },
//        //        new TvgMedia { Name ="test3", Position= 3, Group="Group2", IsValid = true, MediaType = MediaType.LiveTv, Enabled = true, Url=$"http://{server}:{port}/live/{username}/{password}/3.ts" },
//        //        new TvgMedia { Name ="test4", Position= 4, Group="Group1", IsValid = true, MediaType = MediaType.LiveTv, Enabled = true, Url=$"http://{server}:{port}/live/{username}/{password}/4.ts" },
//        //        new TvgMedia { Name ="test5", Position= 5, Group="Group3", IsValid = true, MediaType = MediaType.LiveTv, Enabled = true, Url=$"http://{server}:{port}/live/{username}/{password}/5.ts" },
//        //    };

//        //    var pl = new Playlist
//        //    {
//        //        SynkConfig = new SynkConfig { Url = $"http://{server}:{port}/get.php" },
//        //        Medias = new JsonObject<List<TvgMedia>>(medias.Concat( new List<TvgMedia> {
//        //            new TvgMedia { Name = "test6", Position = 6, Group = "Group3", IsValid = true, MediaType = MediaType.LiveTv, Enabled = true, Url = $"http://{server}:{port}/live/{username}/{password}/7.ts" }}
//        //            ).ToList())
//        //    };

//        //    var panel = new XtreamPanel
//        //    {
//        //        User_info = new User_Info { Username = username, Password = password },
//        //        Server_info = new Server_Info { Url = server, Port = port },
//        //        Available_channels = new Available_Channels
//        //        {
//        //            Channels = medias.Concat(new List<TvgMedia> {
//        //            new TvgMedia { Name = addedMedia, Position = 7, Group = "Group2", IsValid = true, MediaType = MediaType.LiveTv, Enabled = true, Url = $"http://{server}:{port}/live/{username}/{password}/6.ts" }}
//        //            ).Select(Channels.TvgMediaToChannels).ToList()
//        //        }
//        //    };

//        //    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(panel))))
//        //    {
//        //        var res = await service.DiffWithSourceAsync(() => pl, new XtreamProvider(stream));
//        //        Assert.True(res.removed.Count() == 1);
//        //        Assert.True(res.tvgMedia.Count() == 1);
//        //        Assert.Equal(addedMedia, res.tvgMedia.FirstOrDefault().Name);
//        //    }
//        //}

//    }
//}
