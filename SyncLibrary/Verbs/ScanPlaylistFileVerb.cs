using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hfa.SyncLibrary.Verbs
{
    [Verb("sf", HelpText = "Scan playlist for new streams")]
    internal class ScanPlaylistFileVerb : IOptions
    {
        public bool Verbose { get; set; }

        [Value(0, MetaName = "Stream Pattern",
         HelpText = "Stream pattern",
         Required = true)]
        [Option('s')]
        public string StreamPattern { get; set; }

        [Value(2, MetaName = "From stream number (min)",
        HelpText = "From stream number (min)",
        Required = false)]
        [Option('f')]
        public int From { get; set; } = 0;

        [Value(3, MetaName = "Count stream (limit)",
        HelpText = "Count stream (limit)",
        Required = true)]
        [Option('c')]
        public int Count { get; set; } = 1000;

        [Value(4, MetaName = "local playlist file path",
         HelpText = "Xmltv file Path",
         Required = false)]
        [Option('l')]
        public string LocalFilePath { get; set; }

        [Usage(ApplicationAlias = "synker.exe")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("normal scenario", new ScanPlaylistFileVerb { LocalFilePath = "playlist.m3u", StreamPattern = "http://localhost:56800/api/v1/xmltv/{0}.ts", From = 0, To = 10000, Verbose = true });
                yield return new Example("minimal scenario", new ScanPlaylistFileVerb { LocalFilePath = "playlist.m3u", StreamPattern = "http://localhost:56800/api/v1/xmltv/{0}.ts", To = 10000 });
            }
        }

    }
}
