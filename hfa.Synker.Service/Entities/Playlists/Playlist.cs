﻿using hfa.Synker.Service.Entities.Auth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace hfa.Synker.Service.Entities.Playlists
{
    public class Playlist : EntityBase
    {
        public Playlist()
        {
            SynkConfig = new SynkConfig();
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

        [Required]
        public string Content { get; set; }

        TvgMedia[] _tvgMedias;
        [NotMapped]
        public List<TvgMedia> TvgMedias
        {
            get
            {
                if (_tvgMedias != null)
                    return _tvgMedias.ToList();

                if (string.IsNullOrEmpty(Content))
                    return null;

                _tvgMedias = JsonConvert.DeserializeObject<TvgMedia[]>(Content);

                return _tvgMedias.ToList();
            }
        }

        Playlist<TvgMedia> _playlist;

        [NotMapped]
        public Playlist<TvgMedia> PlaylistObject
        {
            get
            {
                if (_playlist == null && TvgMedias != null)
                    _playlist = new Playlist<TvgMedia>(TvgMedias);
                return _playlist;
            }
        }

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
