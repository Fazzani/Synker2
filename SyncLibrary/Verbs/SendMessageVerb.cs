using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hfa.SyncLibrary.Verbs
{
    [Verb("sm", HelpText = "Send message to synker api")]
    class SendMessageVerb : IOptions
    {
        /// <summary>
        /// Message ID
        /// </summary>
        [Value(1, MetaName = "Message id or author",
        HelpText = "Message id or author.",
        Required = true)]
        [Option('a')]
        public string Author { get; set; }
        public bool Verbose { get; set; }

        [Value(0, MetaName = "Message to send",
         HelpText = "Message to send.",
         Required = true)]
        public string Message { get; set; }

        [Value(0, MetaName = "Message to send",
         HelpText = "Message to send.")]
        [Option('t')]
        public int MessageType { get; set; }

        [Usage(ApplicationAlias = "synker.exe")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Normal scenario", new SendMessageVerb { Message = "Hello", Author="GUID" });
                yield return new Example("Verbose", new SendMessageVerb { Message = "Hello", Verbose = true });
            }
        }

    }
}
