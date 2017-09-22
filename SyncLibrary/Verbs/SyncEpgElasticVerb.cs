using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hfa.SyncLibrary.Verbs
{
    [Verb("epg", HelpText = "Sync Xmltv file to Elastic")]
    class SyncEpgElasticVerb : IOptions
    {
        public bool Verbose { get; set; }

        [Value(0, MetaName = "epg",
         HelpText = "Xmltv file to be processed.",
         Required = true)]
        public string FilePath { get; set; }

        [Usage(ApplicationAlias = "synker.exe")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Normal scenario", new SyncMediasElasticVerb { FilePath="~/guide.xmltv" });
                yield return new Example("Verbose", new SyncMediasElasticVerb { Verbose = true });
            }
        }
    }
}
