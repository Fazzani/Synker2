using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hfa.SyncLibrary.Verbs
{
    [Verb("diff", HelpText = "Diff playlist from database for synker app")]
    class DiffPlaylistVerb : IOptions
    {
        public bool Verbose { get; set; }

        [Usage(ApplicationAlias = "synker.exe")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("normal scenario", new DiffPlaylistVerb { Verbose = false });
            }
        }

    }
}
