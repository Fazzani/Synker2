namespace PlaylistManager.Entities
{
    using hfa.PlaylistBaseLibrary.Entities;
    using MongoDB.Bson.Serialization.Attributes;
    using System;
    [Serializable]
    public class Tvg
    {
        public Tvg()
        {
            TvgSource = new TvgSource();
        }

        [BsonElement(nameof(Id))]
        public string Id { get; set; }
        [BsonElement(nameof(Logo))]
        public string Logo { get; set; }
        [BsonElement(nameof(Name))]
        public string Name { get; set; }
        [BsonElement("ti")]
        public string TvgIdentify { get; set; }
        [BsonElement("s")]
        public string Shift { get; set; }
        [BsonElement("at")]
        public string Audio_track { get; set; }
        [BsonElement("ar")]
        public string Aspect_ratio { get; set; }

        [BsonElement("tsi")]
        public string TvgSiteSource { get; set; }

        [BsonElement("ts")]
        public TvgSource TvgSource { get; set; }

        public override string ToString() => $"{Id} : {Name} {Shift} {TvgSource}";
    }

}
