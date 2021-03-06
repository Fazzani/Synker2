﻿namespace hfa.Synker.Service.Elastic
{
    using System.ComponentModel.DataAnnotations;
    public class ElasticConfig
    {
        [Required]
        public string ElasticUrl { get; set; }
        public string ElasticUserName { get; set; }
        public string ElasticPassword { get; set; }
        public string DefaultIndex { get; set; }
        public double RequestTimeout { get; set; }
        public string SitePackIndex { get; set; }
        public string MediaRefIndex { get; set; }
        public string PiconIndex { get; set; }

        public int MaxResultWindow { get; internal set; } = 1_000_000;
        public const string ELK_KEYWORD_SUFFIX = "keyword";
    }
}
