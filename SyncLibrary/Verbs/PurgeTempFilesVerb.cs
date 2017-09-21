using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace Hfa.SyncLibrary.Verbs
{
    [Verb("purge", HelpText = "Purge temp files")]
    class PurgeTempFilesVerb : IOptions
    {
        public bool Verbose { get; set; }

        /// <summary>
        /// Purge Temp files
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task MainPurgeAsync(PurgeTempFilesVerb options) =>
            await Task.Run(() => Directory.GetFiles(Directory.GetCurrentDirectory(), "*.tmp").AsParallel().ForAll(File.Delete));
    }
}
