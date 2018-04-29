using System;
using System.Collections.Generic;
using System.Text;
using TMDbLib.Objects.General;

namespace hfa.Synker.Service.Entities.MediaScraper
{
   public class MediaInfo
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string PosterPath { get; set; }
        public string Id { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string OriginalName { get; set; }

        public MediaType MediaType { get; set; }
    }
}
