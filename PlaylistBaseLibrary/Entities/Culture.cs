namespace hfa.PlaylistBaseLibrary.Entities
{
    using MongoDB.Bson.Serialization.Attributes;
    using System;
    [Serializable]
    public class Culture
    {
        [BsonElement(nameof(Code))]
        public string Code { get; set; }

        [BsonElement(nameof(Country))]
        public string Country { get; set; }

        public override string ToString() => $"{Code} : {Country}";
    }

    [Serializable]
    public class TvgSource : Culture
    {
        [BsonElement(nameof(Site))]
        public string Site { get; set; }

        public override string ToString() => $"{Site} : {base.ToString()}";
    }

}
