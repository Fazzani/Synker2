namespace hfa.Synker.Service.Entities.Playlists
{
    using MongoDB.Bson.Serialization.Attributes;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    public class SynkConfig
    {
        /// <summary>
        /// Try match logos for new inserted medias
        /// </summary>
        [BsonElement(nameof(SynkLogos))]
        public bool SynkLogos { get; set; }

        /// <summary>
        /// Try match Epg for new inserted medias
        /// </summary>
        [BsonElement(nameof(SynkEpg))]
        public bool SynkEpg { get; set; }

        /// <summary>
        /// Try grouping new inserted medias
        /// </summary>
        [BsonElement(nameof(SynkGroup))]
        public SynkGroupEnum SynkGroup { get; set; } = SynkGroupEnum.None;

        /// <summary>
        /// Try clean media name for new inserted medias
        /// </summary>
        [BsonElement(nameof(CleanName))]
        public bool CleanName { get; set; }

        [BsonElement(nameof(Url))]
        public string Url { get; set; }

        [NotMapped]
        public Uri Uri { get { return new Uri(Url); } }

        [BsonElement(nameof(Provider))]
        public string Provider { get; set; } = "m3u";

        /// <summary>
        /// Notification of new inserted Medias
        /// </summary>
        [BsonElement(nameof(NotifcationTypeInsertedMedia))]
        public NotificationTypeEnum? NotifcationTypeInsertedMedia { get; set; }

        [BsonElement(nameof(AutoSynchronize))]
        public bool AutoSynchronize { get; set; } = true;
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
