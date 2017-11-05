using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.Synker.Service.Entities.MediasRef
{
    public class MediaRef
    {
        public MediaRef(string displayName, string group, string culture, string xmltv_id, string site_idAndXmltv_id, string mediaType = MediaTypes.Video)
        {
            DisplayNames = new List<string> { displayName };
            Groups = new List<string> { culture, group };
            Cultures = new List<string> { culture };
            MediaType = mediaType;
            Tvg = new Tvg { Name = displayName, Id = xmltv_id, TvgIdentify = site_idAndXmltv_id };
        }

        public MediaRef()
        {
            DisplayNames = new List<string>();
            Groups = new List<string>();
            Cultures = new List<string>();
            Tvg = new Tvg();
        }

        public List<string> DisplayNames { get; set; }
        public Tvg Tvg { get; set; }
        public List<string> Groups { get; set; }
        //TODO: GET culture code from this api https://restcountries.eu/rest/v2/name/{countryName}
        public List<string> Cultures { get; set; }
        public string MediaType { get; set; } = MediaTypes.Video;
    }

    public static class MediaTypes
    {
        public const string Video = "Video";
        public const string Audio = "Audio";
    }
}
