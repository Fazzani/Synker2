using PlaylistBaseLibrary.Providers.Linq;
using PlaylistManager.Entities;
using System;
using TvheadendLibrary;
using Xunit;
using System.Linq;
using TvheadendLibrary.Common;
using System.Collections.Generic;
using static TvheadendLibrary.Constants;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.tvhLibrary.test
{
    public class TvheadendServiceUnitTest
    {
        readonly TvheadendService _tvheadendService;
        private readonly Playlist<TvgMedia> _playlist;
        const string TvhUrl = "http://192.168.1.29:9981";
        const string TvhUserName = "heni";
        const string TvhPassword = "heni";

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

        [Theory(DisplayName ="List Channels")]
        [InlineData("name", "bein")]
        [InlineData("tags", "France")]
        public void UpdateMediaTest(string field, string query)
        {
            var res = _tvheadendService.Channels(new QueryParams
            {
                Filters = new List<IQueryParamsFilter> { FactoryQueryParamsFilter.CreateStringQueryParamsFilter(field, query) }
            });

            Assert.NotNull(res);
            Assert.Equal(res.Count(), 20);
        }

        [Fact(DisplayName = "Update node")]
        public async Task UpdateNode()
        {
            var res = await _tvheadendService.NodesAsync(new QueryParams
            {
                Limit = 1,
                Filters = new List<IQueryParamsFilter> { FactoryQueryParamsFilter.CreateStringQueryParamsFilter("name", "bein") }
            }, API_URLS.CHANNELS_LIST, CancellationToken.None);

            Assert.Equal(res.Count(), 1);

            var expectedMediaName = (string)res.SingleOrDefault()["name"];

            res.SingleOrDefault()["name"] = nameof(UpdateNode);
            _tvheadendService.UpdateNode(res);


            res = await _tvheadendService.NodesAsync(new QueryParams
            {
                Limit = 1,
                Filters = new List<IQueryParamsFilter> { FactoryQueryParamsFilter.CreateStringQueryParamsFilter("name", nameof(UpdateNode)) }
            }, API_URLS.CHANNELS_LIST, CancellationToken.None);

            Assert.Equal(res.Count(), 1);
            res.SingleOrDefault()["name"] = expectedMediaName;
            _tvheadendService.UpdateNode(res);
        }

        [Theory(DisplayName ="List EPG")]
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
