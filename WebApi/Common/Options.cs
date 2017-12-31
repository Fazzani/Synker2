using System.Collections.Generic;

namespace hfa.WebApi.Common
{
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

    public class GlobalOptions
    {
        public string TmdbAPI { get; set; }
        public string TmdbPosterBaseUrl { get; set; }
    }
}