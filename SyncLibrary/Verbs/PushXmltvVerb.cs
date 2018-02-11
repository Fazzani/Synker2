using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hfa.SyncLibrary.Verbs
{
    [Verb("push", HelpText = "Push xmltv to url")]
    class PushXmltvVerb : IOptions
    {
        public bool Verbose { get; set; }

        [Value(1, MetaName = "Api url",
         HelpText = "Api url for .",
         Required = true)]
        public string ApiUrl { get; set; } = "http://localhost:56800/api/v1/xmltv/UploadFromJson";

        [Value(0, MetaName = "Xmltv file path",
         HelpText = "Xmltv file Path",
         Required = true)]
        public string FilePath { get; set; } = "guide.xmltv";

        [Usage(ApplicationAlias = "synker.exe")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("normal scenario", new PushXmltvVerb { FilePath = "guide.xmltv", ApiUrl = "http://localhost:56800/api/v1/xmltv/UploadFromJson" });
            }
        }

    }
}
