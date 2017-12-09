﻿using hfa.PlaylistBaseLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlaylistManager.Entities
{
    [Serializable]
    public class Tvg
    {
        public Tvg()
        {
            TvgSource = new TvgSource();
        }

        public string Id { get; set; }
        public string Logo { get; set; }
        public string Name { get; set; }
        public string TvgIdentify { get; set; }
        public string Shift { get; set; }
        public string Audio_track { get; set; }
        public string Aspect_ratio { get; set; }

        public string TvgSiteSource { get; set; }

        public TvgSource TvgSource { get; set; }

        public override string ToString() => $"{Id} : {Name} {Shift} {TvgSource}";
    }

}
