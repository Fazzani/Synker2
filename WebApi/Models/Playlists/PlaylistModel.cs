using hfa.Synker.Service.Entities.Playlists;
using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Models.Playlists
{
    public class PlaylistModel
    {
        public int UserId { get; set; }

        [MaxLength(100)]
        [Required]
        public string Freindlyname { get; set; }

        [MaxLength(10)]
        [RegularExpression(@"(28|\*) (2|\*) (7|\*) (1|\*) (1|\*)")]
        public string Cron { get; set; }

        public PlaylistStatus Status { get; set; }

        public List<TvgMedia> TvgMedias { get; set; }

        public string Url { get; set; }

        public DateTime CreatedDate { get; private set; }
        public DateTime UpdatedDate { get; private set; }
        public int Id { get; private set; }
        public Guid UniqueId { get; private set; }
        public bool SynkEpg { get; internal set; }
        public SynkGroupEnum SynkGroup { get; internal set; }
        public bool SynkLogos { get; internal set; }

        public static PlaylistModel ToModel(Playlist pl) => new PlaylistModel
        {
            Cron = pl.SynkConfig?.Cron,
            Freindlyname = pl.Freindlyname,
            Status = pl.Status,
            TvgMedias = pl.TvgMedias,
            CreatedDate = pl.CreatedDate,
            Id = pl.Id,
            UpdatedDate = pl.UpdatedDate,
            UserId = pl.UserId,
            UniqueId = pl.UniqueId,
            SynkEpg = pl.SynkConfig.SynkEpg,
            SynkGroup = pl.SynkConfig.SynkGroup,
            SynkLogos = pl.SynkConfig.SynkLogos,
            Url = pl.SynkConfig.Url
        };
    }
}
