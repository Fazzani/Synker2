﻿namespace hfa.Synker.Service.Entities.MediasRef
{
    using PlaylistManager.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    public class MediaRef : IEqualityComparer<MediaRef>
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public MediaRef(string displayName, string group, string culture, string xmltv_id, string site_idAndXmltv_id, string mediaType = MediaTypes.Video)
        {
            DisplayNames = new List<string> { displayName };
            Groups = new List<string> { culture, group };
            Cultures = new List<string> { culture };
            MediaType = mediaType;
            Tvg = new Tvg { Name = displayName, Id = xmltv_id, TvgIdentify = site_idAndXmltv_id };
            _defaultSite = group;
        }

        public MediaRef()
        {
            DisplayNames = new List<string>();
            Groups = new List<string>();
            Cultures = new List<string>();
            Tvg = new Tvg();
        }

        public MediaRef(string name)
        {
            DisplayNames = new List<string> { name };
            Groups = new List<string>();
            Cultures = new List<string> { "en" };
            Tvg = new Tvg();
        }

        private string _defaultSite;
        public string DefaultSite
        {
            get
            {
                if (string.IsNullOrEmpty(_defaultSite) && Groups.Count > 1)
                {
                    _defaultSite = Groups[1];
                }
                return _defaultSite;
            }
            set { _defaultSite = value; }
        }

        public List<string> DisplayNames { get; set; }
        public Tvg Tvg { get; set; }
        public List<string> Groups { get; set; }
        //TODO: GET culture code from this api https://restcountries.eu/rest/v2/name/{countryName}
        public List<string> Cultures { get; set; }
        public string MediaType { get; set; } = MediaTypes.Video;

        public override int GetHashCode() => Id.GetHashCode();

        public override bool Equals(object obj)
        {
            var m = obj as MediaRef;
            if (m == null)
                return false;
            return GetHashCode() == m.GetHashCode();
        }

        public bool Equals(MediaRef x, MediaRef y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            // If one object null the return false
            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
            {
                return false;
            }

            return x.GetHashCode() == y.GetHashCode();
        }

        public int GetHashCode(MediaRef obj)
        {
            if (object.ReferenceEquals(obj, null))
            {
                return 0;
            }

            return obj.DisplayNames.FirstOrDefault().GetHashCode() ^ obj.Cultures.FirstOrDefault().GetHashCode() ^ obj.MediaType.GetHashCode();
        }

        public override string ToString() => $"{DisplayNames.FirstOrDefault()} : {Cultures.FirstOrDefault()}";
    }

    public static class MediaTypes
    {
        public const string Video = "Video";
        public const string Audio = "Audio";
    }
}
