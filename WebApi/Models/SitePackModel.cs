namespace hfa.WebApi.Models
{
    using hfa.Synker.Service.Services.Xmltv;
    using Hfa.WebApi.Models;
    using Nest;
    public class SitePackModel : SitePackChannel, IModel<SitePackChannel, SitePackModel>
    {
        public SitePackModel ToModel(IHit<SitePackChannel> hit)
        {
            Id = hit.Id;
            Site = hit.Source.Site;
            Site_id = hit.Source.Site_id;
            Xmltv_id = hit.Source.Xmltv_id;
            Source = hit.Source.Source;
            Update = hit.Source.Update;
            Channel_name = hit.Source.Channel_name;
            Country = hit.Source.Country;
            MediaType = hit.Source.MediaType;
            DisplayNames = hit.Source.DisplayNames;
            Logo = hit.Source.Logo;
            return this;
        }
    }
}
