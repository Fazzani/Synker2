﻿namespace PlaylistManager.Entities
{
    using hfa.PlaylistBaseLibrary.Entities;
    using PlaylistBaseLibrary.Entities;
    using System;
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

        private string _streamId;
        public string StreamId
        {
            get
            {
                if (string.IsNullOrEmpty(_streamId))
                {
                    var tab = Url.Split('/');
                    if (tab.Length > 2)
                        _streamId = tab[tab.Length - 1].Split('.')[0];
                }
                return _streamId;
            }
            set
            {
                _streamId = value;
            }
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
