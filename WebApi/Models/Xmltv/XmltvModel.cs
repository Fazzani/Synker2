using Hfa.WebApi.Models;
using PlaylistBaseLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;

namespace hfa.WebApi.Models.Xmltv
{
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
