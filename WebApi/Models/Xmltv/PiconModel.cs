namespace hfa.WebApi.Models.Xmltv
{
    using hfa.Synker.Service.Services.Picons;
    using Hfa.WebApi.Models;
    using Nest;
    public class PiconModel : Picon, IModel<Picon, PiconModel>
    {
        public PiconModel ToModel(IHit<Picon> hit)
        {
            Id = hit.Id;
            this.Path = hit.Source.Path;
            Url = hit.Source.Url;
            return this;
        }
    }
}
