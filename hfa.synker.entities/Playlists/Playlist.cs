namespace hfa.Synker.Service.Entities.Playlists
{
    using hfa.Synker.Service.Entities.Auth;
    using Newtonsoft.Json;
    using PlaylistManager.Entities;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    public class Playlist : EntityBase
    {
        public Playlist()
        {
            SynkConfig = new SynkConfig();
            Favorite = false;
            _tvgSites = new List<string>();
        }

        [Required]
        public Guid UniqueId { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        public int UserId { get; set; }

        [MaxLength(100)]
        [Required]
        public string Freindlyname { get; set; }

        public SynkConfig SynkConfig { get; set; }

        public PlaylistStatus Status { get; set; }

        private List<TvgMedia> _tvgMedias;
        [NotMapped]
        public List<TvgMedia> TvgMedias
        {
            get
            {
                if (_tvgMedias == null)
                    _tvgMedias = Medias == null ? null : JsonConvert.DeserializeObject<List<TvgMedia>>(Medias);
                return _tvgMedias;
            }
            set { Medias = JsonConvert.SerializeObject(value); }
        }

        [Column(TypeName = "jsonb")]
        public string Medias { get; set; }

        [Column(name: nameof(Tags), TypeName = "jsonb")]
        public string TagsString { get; set; }

        public Dictionary<string, string> _tags;
        [NotMapped]
        public Dictionary<string, string> Tags
        {
            get
            {
                if (_tags == null)
                    _tags = TagsString == null ? null : JsonConvert.DeserializeObject<Dictionary<string, string>>(TagsString);
                return _tags;
            }
            set { TagsString = JsonConvert.SerializeObject(value); }
        }

        private List<String> _tvgSites { get; set; }

        [NotMapped]
        public List<string> TvgSites
        {
            get { return _tvgSites; }
            set { _tvgSites = value; }
        }

        [Required]
        public string TvgSitesString
        {
            get { return String.Join("|", _tvgSites); }
            set { _tvgSites = value.Split('|').ToList(); }
        }

        public bool Favorite { get; set; }

        [NotMapped]
        public bool IsSynchronizable => SynkConfig != null && !string.IsNullOrEmpty(SynkConfig.Url);


        [NotMapped]
        public bool IsXtreamTag
        {
            get
            {
                return Tags != null && Tags.Any() && Tags[PlaylistTags.IsXtream] == "True";
            }
        }

        /// <summary>
        /// The import provider
        /// </summary>
        [NotMapped]
        public string ImportProviderTag
        {
            get
            {
                Tags.TryGetValue(PlaylistTags.ImportProvider, out string result);
                return result;
            }
        }

        public override int GetHashCode() => (UserId ^ Id).GetHashCode();

        public override string ToString() => $"{Id} {Freindlyname} UserId : {UserId} Status : {Status} Count : {TvgMedias?.Count()}";

    }

    public enum PlaylistStatus : byte
    {
        Enabled = 0,
        Disabled = 1
    }


    public class PlaylistTags
    {
        public const string IsXtream = "IsXtream";
        public const string ImportProvider = "Import_provider";

    }
}
