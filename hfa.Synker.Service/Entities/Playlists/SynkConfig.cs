using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace hfa.Synker.Service.Entities.Playlists
{
    public class SynkConfig
    {
        /// <summary>
        /// Try match logos for new inserted medias
        /// </summary>
        public bool SynkLogos { get; set; }

        /// <summary>
        /// Try match Epg for new inserted medias
        /// </summary>
        public bool SynkEpg { get; set; }

        /// <summary>
        /// Try grouping new inserted medias
        /// </summary>
        public SynkGroupEnum SynkGroup { get; set; } = SynkGroupEnum.None;

        /// <summary>
        /// Try clean media name for new inserted medias
        /// </summary>
        public bool CleanName { get; set; }

        public string Url { get; set; }

        [NotMapped]
        public Uri Uri { get { return new Uri(Url); } }

        public string Provider { get; set; } = "m3u";

        /// <summary>
        /// Notification of new inserted Medias
        /// </summary>
        public NotificationTypeEnum? NotifcationTypeInsertedMedia { get; set; }
    }

    public enum SynkGroupEnum : byte
    {
        None = 0,
        ByCountry = 1,
        ByLanguage = 2,
        Custom = 3
    }

    [Flags]
    public enum NotificationTypeEnum
    {
        PushBrowser = 1,
        PushMobile = 2,
        Email = 4,
        Sms = 8
    }
}
