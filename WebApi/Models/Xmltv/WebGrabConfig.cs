using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Models.Xmltv
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
    public class SitePackChannel
    {
        [XmlAttribute(AttributeName = "update")]
        public string Update { get; set; }
        [XmlAttribute(AttributeName = "site")]
        public string Site { get; set; }
        [XmlAttribute(AttributeName = "site_id")]
        public string Site_id { get; set; }
        [XmlAttribute(AttributeName = "xmltv_id")]
        public string Xmltv_id { get; set; }
        [XmlText]
        public string Channel_name { get; set; }
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
    }
}
