using hfa.PlaylistBaseLibrary.Entities;
using hfa.Synker.Service.Entities.Playlists;
using Hfa.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hfa.WebApi.Models.Playlists
{
    public class PlaylistModel
    {
        private IUrlHelper _urlHelper;

        public PlaylistModel(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }

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
        public Guid UniqueId { get; set; }
        public bool SynkEpg { get; set; }
        public SynkGroupEnum SynkGroup { get; set; }
        public bool SynkLogos { get; set; }
        public List<string> TvgSites { get; set; }
        public Dictionary<string, string> Tags { get; set; }

        public NotificationTypeEnum? NotifcationTypeInsertedMedia { get; set; }

        public string PublicId
        {
            get
            {
                return Encoding.UTF8.EncodeBase64(UniqueId.ToString());
            }
        }

        public string PublicUrl
        {
            get
            {
                return _urlHelper?.Action("GetFile", "Playlists", new { Id = PublicId }, _urlHelper.ActionContext.HttpContext.Request.Scheme);
            }
        }

        public bool IsXtream { get; private set; }
        public string ImportProvider { get; private set; }

        public static PlaylistModel ToModel(Playlist pl, IUrlHelper uriHelper) => new PlaylistModel(uriHelper)
        {
            Freindlyname = pl.Freindlyname,
            Status = pl.Status,
            TvgMedias = pl?.TvgMedias,
            CreatedDate = pl.CreatedDate,
            Id = pl.Id,
            UpdatedDate = pl.UpdatedDate,
            UserId = pl.UserId,
            UniqueId = pl.UniqueId,
            SynkEpg = pl.SynkConfig.SynkEpg,
            SynkGroup = pl.SynkConfig.SynkGroup,
            SynkLogos = pl.SynkConfig.SynkLogos,
            Url = pl.SynkConfig.Url,
            TvgSites = pl.TvgSites,
            IsXtream = pl.IsXtreamTag,
            ImportProvider = pl.ImportProviderTag,
            Tags = pl.Tags,
            NotifcationTypeInsertedMedia = pl.SynkConfig?.NotifcationTypeInsertedMedia
        };

        public static PlaylistModel ToLightModel(Playlist pl, IUrlHelper uriHelper) => new PlaylistModel(uriHelper)
        {
            Id = pl.Id,
            UniqueId = pl.UniqueId,
            UserId = pl.UserId,
            Freindlyname = pl.Freindlyname,
            Url = pl.SynkConfig.Url,
            Status = pl.Status,
            CreatedDate = pl.CreatedDate,
            UpdatedDate = pl.UpdatedDate,
            SynkEpg = pl.SynkConfig.SynkEpg,
            SynkGroup = pl.SynkConfig.SynkGroup,
            SynkLogos = pl.SynkConfig.SynkLogos,
            TvgSites = pl.TvgSites,
            IsXtream = pl.IsXtreamTag,
            ImportProvider = pl.ImportProviderTag,
            Tags = pl.Tags,
            NotifcationTypeInsertedMedia = pl.SynkConfig?.NotifcationTypeInsertedMedia
        };
    }
}
