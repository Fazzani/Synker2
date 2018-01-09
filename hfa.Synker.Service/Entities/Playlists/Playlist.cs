using hfa.Synker.Service.Entities.Auth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;

namespace hfa.Synker.Service.Entities.Playlists
{
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

        [NotMapped]
        public List<TvgMedia> TvgMedias => Medias?.Object;

        public JsonObject<List<TvgMedia>> Medias { get; set; }

        public JsonObject<string> Tags { get; set; }

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

        public override int GetHashCode() => (UserId ^ Id).GetHashCode();

        public override string ToString() => $"{Id} {Freindlyname} UserId : {UserId} Status : {Status} Count : {TvgMedias?.Count()}";
    }

    public enum PlaylistStatus : byte
    {
        Enabled = 0,
        Disabled = 1
    }
}
