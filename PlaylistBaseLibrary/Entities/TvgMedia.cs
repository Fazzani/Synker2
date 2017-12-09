using hfa.PlaylistBaseLibrary.Entities;
using PlaylistBaseLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlaylistManager.Entities
{
    [Serializable]
    public class TvgMedia : Media
    {
        public TvgMedia(string name, string url) : base(name, url)
        {
            Tvg = new Tvg();
        }

        public TvgMedia()
        {
            Tvg = new Tvg();
            Culture = new Culture();
        }

        //public List<string> Urls { get; set; }
        public Culture Culture { get; set; }

        public Tvg Tvg { get; set; }

        public override string Format(IMediaFormatter mediaFormatter) => mediaFormatter.Format(this);

        public override string ToString() => base.ToString() + $" {Tvg.Name}";

        public static TvgMedia CreateForUpdate()
        {
            var res = new TvgMedia
            {
                Id = null
            };
            return res;
        }
    }
}
