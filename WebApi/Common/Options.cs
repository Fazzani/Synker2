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

        public int TokenLifetimeInMinutes { get; set; }

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

    public class VapidKeysOptions
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }

    public class MediaServerOptions
    {
        public string StreamBaseUrl { get { return $"{Scheme}://{Host}:{PortStreaming}/"; } }
        public string StreamRtmpBaseUrl { get { return $"rtmp://{Host}:{Rtmp}/"; } }
        public string StreamWebsocketBaseUrl { get { return $"{WsScheme}://{Host}:{PortStreaming}/"; } }
        public string Scheme => IsSecure ? "https" : "http";
        public string WsScheme => IsSecure ? "wss" : "ws";
        /// <summary>
        /// Server media adress (ip or dns)
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Rtmp Port
        /// </summary>
        public uint Rtmp { get; set; } = 1935;

        /// <summary>
        /// Http  Api
        /// </summary>
        public uint Port { get; set; } = 80;

        /// <summary>
        /// Http Port Streaming
        /// </summary>
        public uint PortStreaming { get; set; } = 8000;
        
        /// <summary>
        /// Is Https
        /// </summary>
        public bool IsSecure { get; set; } = false;

        /// <summary>
        /// Auth for api access
        /// </summary>
        public BasicAuthOptions BasicAuthApiOptions { get; set; }

        /// <summary>
        /// Auth for playing or publishing a stream
        /// </summary>
        public AuthOptions Auth { get; set; }

        /// <summary>
        /// Auth for playing or publishing a stream
        /// </summary>
        public class AuthOptions
        {
            public bool Play { get; set; } = true;
            public bool Publish { get; set; } = true;
            public string Secret { get; set; } = "defaultvalue";
        }
    }

    public class BasicAuthOptions
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class MongoOptions
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
    }

}