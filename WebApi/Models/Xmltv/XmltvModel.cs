namespace hfa.WebApi.Models.Xmltv
{
    using Hfa.WebApi.Models;
    using Nest;
    using hfa.PlaylistBaseLibrary.Entities;
    public class TvChannelModel : tvChannel, IModel<tvChannel, TvChannelModel>
    {
        public string Id { get; set; }
        public TvChannelModel ToModel(IHit<tvChannel> hit)
        {
            Id = hit.Id;
            icon = hit.Source.icon;
            id = hit.Source.id;
            displayname = hit.Source.displayname;
            return this;
        }
    }
}
