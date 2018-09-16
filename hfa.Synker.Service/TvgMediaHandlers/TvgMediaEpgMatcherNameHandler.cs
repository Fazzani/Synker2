namespace hfa.Synkerk.Service.Services.TvgMediaHandlers
{
    using hfa.PlaylistBaseLibrary.ChannelHandlers;
    using hfa.Synker.Service.Services;
    using PlaylistManager.Entities;
    using System.Linq;
    using System.Threading;

    public class TvgMediaEpgMatcherNameHandler : TvgMediaHandler
    {
        private ISitePackService _sitePackService;

        public TvgMediaEpgMatcherNameHandler(IContextTvgMediaHandler contextTvgMediaHandler, ISitePackService sitePackService) : base(contextTvgMediaHandler)
        {
            _sitePackService = sitePackService;
        }

        public override void HandleTvgMedia(TvgMedia tvgMedia)
        {
            var result = _sitePackService.MatchMediaNameAndCountryAsync(tvgMedia.DisplayName, tvgMedia.Culture?.Country, CancellationToken.None).GetAwaiter().GetResult();

            if (result != null)
            {
                tvgMedia.Tvg = new Tvg
                {
                    Name = result.Xmltv_id,
                    Id = result.Id,
                    Logo = result.Logo,
                    TvgIdentify = result.Xmltv_id,
                    TvgSiteSource = result.Site,
                    TvgSource = new PlaylistBaseLibrary.Entities.TvgSource { Site = result.Site, Code = result.Site_id, Country = result.Country }
                };
                tvgMedia.Lang = result.Country;
            }
            if (_successor != null)
                _successor.HandleTvgMedia(tvgMedia);
        }
    }
}
