using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hfa.SyncLibrary.Verbs
{
    [Verb("sync", HelpText = "Sync medias from sources provided in config file to Elastic")]
    class SyncMediasElasticVerb : IOptions
    {
        public bool Verbose { get; set; }

        public bool Force { get; set; }

        public string CertificateName { get; set; } = "application_dev";

        [Value(0, MetaName = "config file",
         HelpText = "Config provider file to be processed.",
         Required = true)]
        public string FilePath { get; set; }

        [Usage(ApplicationAlias = "synker.exe")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Normal scenario", new SyncMediasElasticVerb { FilePath="~/configFileName.pll" });
                yield return new Example("Force update all", new SyncMediasElasticVerb { Force = true });
                yield return new Example("Specify certificate name for decrypt source input ", new SyncMediasElasticVerb { CertificateName = "application_dev" });
                yield return new Example("supress summary", UnParserSettings.WithGroupSwitchesOnly(), new SyncMediasElasticVerb { });
                yield return new Example("read more lines", new[] { UnParserSettings.WithGroupSwitchesOnly(), UnParserSettings.WithUseEqualTokenOnly() },
                    new SyncMediasElasticVerb { });
            }
        }
    }
}
