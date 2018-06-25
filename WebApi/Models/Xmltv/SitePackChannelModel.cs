namespace hfa.WebApi.Models.Xmltv
{
    using hfa.Synker.Service.Services.Xmltv;
    using Hfa.WebApi.Models;
    using Nest;
    public class SitePackChannelModel : SitePackChannel, IModel<SitePackChannel, SitePackChannelModel>
    {
        public SitePackChannelModel ToModel(IHit<SitePackChannel> hit)
        {
            Id = hit.Source.Id;
            Channel_name = hit.Source.Source;
            Site = hit.Source.Site;
            Site_id = hit.Source.Site_id;
            Source = hit.Source.Source;
            Update = hit.Source.Update;
            Xmltv_id = hit.Source.Xmltv_id;
            return this;
        }
    }
}
