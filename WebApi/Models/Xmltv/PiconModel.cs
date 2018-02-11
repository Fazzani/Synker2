using hfa.Synker.Service.Services.Picons;
using Hfa.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;

namespace hfa.WebApi.Models.Xmltv
{
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
