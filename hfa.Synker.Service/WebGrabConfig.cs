using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;

namespace hfa.Synker.Service.Services.Xmltv
{
    [XmlRoot(ElementName = "postprocess")]
    public class Postprocess
    {
        [XmlAttribute(AttributeName = "grab")]
        public string Grab { get; set; }
        [XmlAttribute(AttributeName = "run")]
        public string Run { get; set; }
    }

    [XmlRoot(ElementName = "retry")]
    public class Retry
    {
        [XmlAttribute(AttributeName = "time-out")]
        public string Timeout { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "channel")]
    public class SitePackChannel : IEqualityComparer<SitePackChannel>
    {
        [XmlIgnore]
        public string id { get; set; }
        [XmlAttribute(AttributeName = "update")]
        public DateTime Update { get; set; }
        [XmlAttribute(AttributeName = "site")]
        public string Site { get; set; }
        [XmlAttribute(AttributeName = "site_id")]
        public string Site_id { get; set; }
        [XmlAttribute(AttributeName = "xmltv_id")]
        public string Xmltv_id { get; set; }
        [XmlText]
        public string Channel_name { get; set; }
        [XmlIgnore]
        public string Source { get; set; }

        public string Country { get; set; }
        public string Logo { get; set; }

        public List<string> DisplayNames { get; set; }

        public SitePackMediaTypes MediaType { get; set; }

        public bool Equals(SitePackChannel x, SitePackChannel y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            // If one object null the return false
            if (x is null || y is null)
            {
                return false;
            }

            return x.GetHashCode() == y.GetHashCode();
        }

        public int GetHashCode(SitePackChannel obj)
        {
            if (obj is null)
            {
                return 0;
            }
            return obj.id.GetHashCode();
        }

        public override string ToString() => $"{MediaType}:{Channel_name} {id} {Country}";
    }

    public enum SitePackMediaTypes
    {
        Channel = 0,
        Radio = 1
    }

    [XmlRoot(ElementName = "settings")]
    public class Settings
    {
        [XmlElement(ElementName = "filename")]
        public string Filename { get; set; }
        [XmlElement(ElementName = "mode")]
        public string Mode { get; set; }
        [XmlElement(ElementName = "user-agent")]
        public string Useragent { get; set; }
        [XmlElement(ElementName = "postprocess")]
        public Postprocess Postprocess { get; set; }
        [XmlElement(ElementName = "logging")]
        public string Logging { get; set; }
        [XmlElement(ElementName = "retry")]
        public Retry Retry { get; set; }
        [XmlElement(ElementName = "skip")]
        public string Skip { get; set; }
        [XmlElement(ElementName = "timespan")]
        public string Timespan { get; set; }
        [XmlElement(ElementName = "update")]
        public string Update { get; set; }
        [XmlElement(ElementName = "channel")]
        public List<SitePackChannel> Channel { get; set; }
        public static Settings New
        {
            get => new Settings
            {
                Filename = $"Guide_{Guid.NewGuid().ToString()}.xmltv",
                Logging = "on",
                Mode = "v",
                Postprocess = new Postprocess { Grab = "y", Run = "y" },
                Retry = new Retry { Text = "2", Timeout = "2" },
                Skip = "12",
                Timespan = "2",
                Update = "f",
                Useragent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0; yie9)"
            };
        }
    }

    public class DistinctTvgSiteBySite : IEqualityComparer<SitePackChannel>
    {
        public bool Equals(SitePackChannel x, SitePackChannel y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.Site.Equals(y.Site, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(SitePackChannel obj)
        {
            if (obj is null)
            {
                return 0;
            }
            return obj.Site.GetHashCode();
        }

    }
}
