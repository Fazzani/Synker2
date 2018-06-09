using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.PlaylistBaseLibrary.Common
{
    public class ApplicationAbout
    {
        public string Author { get; set; }
        public string ApplicationName { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
        public string Comments { get; set; }
        public string Version { get; set; } = Environment.GetEnvironmentVariable("VERSION");
        public string License { get; set; } = "MIT";
        public OperatingSystem OSVersion { get; set; } = Environment.OSVersion;
        public Version EnvVersion { get; set; } = Environment.Version;
    }
}
