namespace hfa.Synker.Service.Entities.MediaScraper
{
    using System;
    using TMDbLib.Objects.General;

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
