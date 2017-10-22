using System.Collections.Generic;

namespace hfa.WebApi.Common
{
    public class ApplicationConfigData
    {
        public string ElasticUrl { get; set; }

        public string DefaultIndex { get; set; }
        public string MessageIndex { get; set; }
        public string SitePackIndex { get; set; }
        public string ElasticUserName { get; set; }
        public string ElasticPassword { get; set; }

        public int RequestTimeout { get; set; }
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

        public string HmacSecretKey { get; set; }
        public string CertificateName { get; set; }

        /// <summary>
        /// Private key
        /// </summary>
        public string RsaPrivateKeyXML { get; set; }

        /// <summary>
        /// Public key
        /// </summary>
        public string RsaPublicKeyXML { get; set; }

        public bool UseRsa { get; set; } = true;

        public int TokenLifetimeInMinutes { get;  set; }

        public string TokenEndPoint { get; set; }
    }
   
    public class PlaylistProviderOption
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class PastBinOptions
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string UserKey { get; set; }
    }

    public class SshConnectionInfo
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PrivateKeyFilePath { get; set; }
        public string Passphrase { get; set; }
        
    }
}