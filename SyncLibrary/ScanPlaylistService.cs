using Hfa.SyncLibrary.Verbs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Synker.batch
{
    internal class ScanPlaylistService
    {
        const string ExNewLine = "#EXTINF:-1,group-title=\"NewChannels\", channel {0}{1}{2}";
        const string EXTM3U = "#EXTM3U";

        public async Task ScanAsync(ScanPlaylistFileVerb options, ILogger logger, CancellationToken token = default(CancellationToken))
        {
            await Task.Run(() =>
             {
                 var existedUrls = File.ReadAllLines(options.LocalFilePath).Where(x => !x.StartsWith("#"));
                 var lines = new ConcurrentBag<string>();

                 Enumerable.Range(options.From, options.Count)
                     .AsParallel()
                     .ForAll(i => PingAsync(existedUrls, lines, i, options.StreamPattern, logger, token).GetAwaiter().GetResult());

                 token.ThrowIfCancellationRequested();

                 logger.LogInformation("Total new urls {0}", lines.Count);

                 if (lines.Count > 0)
                     File.AppendAllLines(options.LocalFilePath, lines);
             });
        }

        private async Task PingAsync(IEnumerable<string> listUrl, ConcurrentBag<string> lines, int i, string streamPatternUrl,
            ILogger logger, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                var uri = String.Format(streamPatternUrl, i);
                if (listUrl.Any(x => x.Equals(uri)))
                    return;

                logger.LogInformation(uri);
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.101 Safari/537.36";

                using (var response = await request.GetResponseAsync())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    token.ThrowIfCancellationRequested();
                    string res = await reader.ReadLineAsync();

                    if (!string.IsNullOrEmpty(res))
                    {
                        lines.Add(string.Format(ExNewLine, i, Environment.NewLine, uri));
                        lines.Add(uri);
                        logger.LogInformation("Address: {0}", uri);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Iteration : {i}  : {e.Message}");
            }
        }
    }
}
