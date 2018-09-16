using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.Brokers.Messages.Configuration
{
    public class FirebaseConfiguration
    {
        public string Secret { get; set; }
        public string ApiKey { get; set; }
        public string AuthDomain { get; set; }
        public string DatabaseURL { get; set; }
        public string ProjectId { get; set; }
        public string StorageBucket { get; set; }
        public string MessagingSenderId { get; set; }
    }
}
