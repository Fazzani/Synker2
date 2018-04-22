using System;

namespace Hfa.SyncLibrary.Infrastructure
{

    public class ConnectionStrings
    {
        public string PlDatabase { get; set; }
    }

    public class ApiOptions
    {
        public string Url { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

    }
    public class TvhOptions
    {
        public string TvhUrl { get; set; }
        public string TvhUserName { get; set; }
        public string TvhPassword { get; set; }
    }

    public class DockerOptions
    {
        public string CertPassword { get; set; }
        public string CertFilePath { get; set; }

        /// <summary>
        /// Docker daemon url
        /// </summary>
        public string Url { get; set; }

        public Uri Uri => new Uri(Url);
    }

}