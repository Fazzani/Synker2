using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.synker.entities.MediaServer
{
    public class MediaServerStreamsStats
    {
        [JsonProperty(PropertyName = "live")]
        public Dictionary<string, Stream> Streams { get; set; }
    }

    public class Stream
    {
        public Publisher publisher { get; set; }
        public Subscriber[] subscribers { get; set; }
    }

    public class Publisher
    {
        public string app { get; set; }
        public string stream { get; set; }
        public string clientId { get; set; }
        public DateTime connectCreated { get; set; }
        public int bytes { get; set; }
        public string ip { get; set; }
        public Audio audio { get; set; }
        public Video video { get; set; }
    }

    public class Audio
    {
        public string codec { get; set; }
        public string profile { get; set; }
        public int samplerate { get; set; }
        public int channels { get; set; }
    }

    public class Video
    {
        public string codec { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string profile { get; set; }
        public float level { get; set; }
        public int fps { get; set; }
    }

    public class Subscriber
    {
        public string app { get; set; }
        public string stream { get; set; }
        public string clientId { get; set; }
        public DateTime connectCreated { get; set; }
        public int bytes { get; set; }
        public string ip { get; set; }
        public string protocol { get; set; }
    }

}
