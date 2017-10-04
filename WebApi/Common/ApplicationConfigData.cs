using System.Collections.Generic;

namespace hfa.WebApi.Common
{
    public class ApplicationConfigData
    {
        public string ElasticUrl { get; set; }

        public string DefaultIndex { get; set; }
        public string MessageIndex { get; set; }
        public string ElasticUserName { get; set; }
        public string ElasticPassword { get; set; }
    }

    public class SecurityOptions
    {
        public string AuthenticationScheme { get; set; } = "Bearer";
        public string Issuer { get; set; }
        public string Audience { get; set; }

        /// <summary>
        /// Password salt
        /// </summary>
        public string Salt { get; set; }

        public string SymmetricSecurityKey { get; set; }
        public string CertificateName { get; set; }
        public int TokenLifetimeInMinutes { get;  set; }
    }
   
    public class PlaylistProviderOption
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}