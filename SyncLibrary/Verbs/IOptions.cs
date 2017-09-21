using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hfa.SyncLibrary.Verbs
{
    interface IOptions
    {

        // Omitting long name, default --verbose
        [Option('v', "verbose",
            HelpText = "Prints all messages to standard output.")]
        bool Verbose { get; set; }
    }
}
