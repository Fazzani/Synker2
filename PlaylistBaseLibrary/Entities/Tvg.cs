using System;
using System.Collections.Generic;
using System.Text;

namespace PlaylistManager.Entities
{
    [Serializable]
    public class Tvg
    {
        public string Id { get; set; }
        public string Logo { get; set; }
        public string Name { get; set; }

        //public virtual ICollection<Media> Medias { get; set; }
        public string TvgIdentify { get; set; }
    }
}
