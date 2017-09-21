using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hfa.SyncLibrary.Verbs
{
    [Verb("save", HelpText = "Save new sync encrypted configuration ")]
    class SaveNewConfigVerb : IOptions
    {
        public bool Verbose { get; set; }

        [Value(0, MetaName = "input file",
         HelpText = "Input file config to be processed.",
         Required = true)]
        public string FilePath { get; set; }

        public string CertificateName { get; set; } = "application_dev";

        [Usage(ApplicationAlias = "synker.exe")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("normal scenario", new SaveNewConfigVerb { Verbose = false });
                yield return new Example("Force update all", new SaveNewConfigVerb { FilePath = "~/configFilename.json" });
                yield return new Example("Specify certificate name for decrypt source input ", new SaveNewConfigVerb { CertificateName = "application_dev" });
                yield return new Example("supress summary", UnParserSettings.WithGroupSwitchesOnly(), new SaveNewConfigVerb { });
            }
        }

    }
}
