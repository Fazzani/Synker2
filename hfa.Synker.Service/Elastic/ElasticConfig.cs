using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace hfa.Synker.Service.Elastic
{
    public class ElasticConfig
    {
        public string ElasticUrl { get;  set; }
        public string ElasticUserName { get;  set; }
        public string ElasticPassword { get;  set; }
        public string DefaultIndex { get;  set; }
        public double RequestTimeout { get;  set; }
        public string SitePackIndex { get; set; }
        public string MediaRefIndex { get; set; }
    }
}
