using PlaylistManager.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TvheadendLibrary;
using TvheadendLibrary.Common;
using Xunit;
using static TvheadendLibrary.Constants;

namespace hfa.tvhLibrary.test
{
    public class TvheadendServiceUnitTest
    {
        private readonly TvheadendService _tvheadendService;
        private readonly Playlist<TvgMedia> _playlist;
        private const string TvhUrl = "http://heni.freeboxos.fr:9981";
        private const string TvhUserName = "heni";
        private const string TvhPassword = "heni";

        public TvheadendServiceUnitTest()
        {
            _tvheadendService = new TvheadendService(TvhUrl, TvhUserName, TvhPassword);
            _playlist = new Playlist<TvgMedia>(_tvheadendService);
        }

        [Fact(DisplayName = "Select medias")]
        public void SelectTest()
        {
            var medias = from m in _playlist select m;
            Assert.NotNull(medias);
            Assert.NotEmpty(medias);
        }

        [Theory(DisplayName = "List Channels")]
        [InlineData("name", "bein")]
        [InlineData("tags", "France")]
        public void UpdateMediaTest(string field, string query)
        {
            var res = _tvheadendService.Channels(new QueryParams
            {
                Filters = new List<IQueryParamsFilter> { FactoryQueryParamsFilter.CreateStringQueryParamsFilter(field, query) }
            });

            Assert.NotNull(res);
            Assert.Equal(20, res.Count());
        }

        [Fact(DisplayName = "Update node")]
        public async Task UpdateNode()
        {
            var res = await _tvheadendService.NodesAsync(new QueryParams
            {
                Limit = 1,
                Filters = new List<IQueryParamsFilter> { FactoryQueryParamsFilter.CreateStringQueryParamsFilter("name", "bein") }
            }, API_URLS.CHANNELS_LIST, CancellationToken.None);

            Assert.Single(res);

            var expectedMediaName = (string)res.SingleOrDefault()["name"];

            res.SingleOrDefault()["name"] = nameof(UpdateNode);
            _tvheadendService.UpdateNode(res);

            res = await _tvheadendService.NodesAsync(new QueryParams
            {
                Limit = 1,
                Filters = new List<IQueryParamsFilter> { FactoryQueryParamsFilter.CreateStringQueryParamsFilter("name", nameof(UpdateNode)) }
            }, API_URLS.CHANNELS_LIST, CancellationToken.None);

            Assert.Single(res);
            res.SingleOrDefault()["name"] = expectedMediaName;
            _tvheadendService.UpdateNode(res);
        }

        [Theory(DisplayName = "List EPG")]
        [InlineData("name", "bein", 2)]
        [InlineData("id", "osn", 5)]
        public void ListEpg(string field, string query, int count)
        {
            var res = _tvheadendService.EPG(new QueryParams
            {
                Limit = count,
                Filters = new List<IQueryParamsFilter> { FactoryQueryParamsFilter.CreateStringQueryParamsFilter(field, query) }
            });

            Assert.Equal(count, res.Count());
        }
    }
}