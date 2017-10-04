using PlaylistBaseLibrary.Providers;
using PlaylistBaseLibrary.Services;
using PlaylistManager.Entities;
using PlaylistManager.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static hfa.SyncLibrary.Global.Common;
using PlaylistBaseLibrary.Providers.Linq;
using System.Linq.Expressions;
using TvheadendLibrary;
using Microsoft.Extensions.Logging;
using hfa.PlaylistBaseLibrary.Providers;

namespace SyncLibrary
{
    class Program
    {
        private const string tvlistFilePath = @"C:\Users\heni.fazzani\Desktop\tvlist.txt";
        private const string m3uFilePath = @"..\..\tv_channels.m3u";
        private const string m3uFilePath2 = @"C:\Users\Heni\Downloads\airysat_original (2).m3u";
        private const string cryptedPlaylistPath = @"..\..\tv_channelsCrypted.m3u";

        public static void Main(string[] args) =>
            MainAsync(args).GetAwaiter().GetResult();

        public static async Task MainAsync(string[] args)
        {

            var ts = new CancellationTokenSource();
#if DEBUG
            var trace = new TraceSource(Assembly.GetCallingAssembly().GetName().Name);
#endif
            try
            {
                Console.WriteLine("0 : Load m3u playlist");
                Console.WriteLine("1 : Diff playlist");
                Console.WriteLine("2 : Encrypt playlist");
                Console.WriteLine("3 : Decrypt playlist");
                Console.WriteLine("4 : Load tvlist playlist");
                Console.WriteLine("5 : QueryProvider Test");
                Console.WriteLine("6 : TVH QueryProvider");
                Console.WriteLine("7 : Export tvList to M3u");
                Console.WriteLine($"ESC : to cancel and exit...{Environment.NewLine}");

                await Task.Run(async () =>
                {
                    do
                    {
                        var key = Console.ReadKey(true).Key;
                        if (key == ConsoleKey.Escape)
                            ts.Cancel();
                        else
                        {
                            if (key == ConsoleKey.NumPad0)
                                await LoadPlaylist(ts.Token);
                            else if (key == ConsoleKey.NumPad1)
                                await Diff(m3uFilePath, m3uFilePath2, ts);
                            //else if (key == ConsoleKey.NumPad2)
                            //    EncryptPlaylist(m3uFilePath2, ts);
                            //else if (key == ConsoleKey.NumPad3)
                            //    DecryptPlaylist(cryptedPlaylistPath, ts);
                            if (key == ConsoleKey.NumPad4)
                                await LoadTvlistPlaylist(tvlistFilePath, ts.Token);
                            if (key == ConsoleKey.NumPad5)
                                await QueryProvider(tvlistFilePath, ts.Token);
                            if (key == ConsoleKey.NumPad6)
                                TVHQueryProvider(ts.Token);
                            if (key == ConsoleKey.NumPad7)
                                await ExportTvListToM3u(tvlistFilePath, ts.Token);
                        }
                    } while (!ts.Token.IsCancellationRequested);
                }, ts.Token);
            }
            catch (OperationCanceledException)
            {
                Logger(nameof(SynchElastic)).LogInformation("Canceled operation");
            }
            catch (Exception e)
            {
                Logger(nameof(SynchElastic)).LogInformation(e.Message);
                throw;
            }
            finally
            {
#if DEBUG
                trace.Flush();
                trace.Close();
#endif
            }

        }

        /// <summary>
        /// Test Query Provider
        /// </summary>
        /// <param name="filePath1"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        private static async Task QueryProvider(string filePath1, CancellationToken ts)
        {
            using (var tvChannelPl = new Playlist<TvgMedia>(new M3uProvider(new FileStream(m3uFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))))
            {
                ts.ThrowIfCancellationRequested();

                var channelNames = from c in tvChannelPl
                                   where c.Name.StartsWith("bein", StringComparison.InvariantCultureIgnoreCase)
                                   select c.Name;

                //var channelNames = tvChannelPl.Select(c => c.Name);
                foreach (var cn in channelNames)
                    Console.WriteLine(cn);
            }
        }

        /// <summary>
        /// Test TVH Query Provider
        /// </summary>
        /// <param name="ts">Cancellation token</param>
        /// <returns></returns>
        private static void TVHQueryProvider(CancellationToken ts)
        {
            using (var tvChannelPl = new Playlist<TvgMedia>(new TvheadendService("http://heni.freeboxos.fr:9981/", "heni", "heni")))
            {
                ts.ThrowIfCancellationRequested();

                var channelNames = (from c in tvChannelPl
                                    orderby c.Name
                                    select c).
                                    Take(2).
                                    Skip(50);
                foreach (var cn in channelNames)
                    Console.WriteLine(cn);
            }
        }

        /// <summary>
        /// Diff two playlists
        /// </summary>
        /// <param name="filePath1"></param>
        /// <param name="filePath2"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        private static async Task Diff(string filePath1, string filePath2, CancellationTokenSource ts)
        {
            using (var tvChannelPl = new Playlist<TvgMedia>(new M3uProvider(new FileStream(filePath1, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))))
            {
                using (var airysatPl = new Playlist<TvgMedia>(new M3uProvider(new FileStream(filePath2, FileMode.Open, FileAccess.Read, FileShare.Read))))
                {
                    var playlistManager = new PlaylistManagerTraced(new PlaylistManager.Services.PlaylistManager());

                    Task.WaitAll(tvChannelPl.PullAsync(ts.Token), airysatPl.PullAsync(ts.Token));
                    var resDiffTask = playlistManager.DiffAsync(tvChannelPl, airysatPl, ts.Token);

                    await resDiffTask.ContinueWith(x =>
                    {
                        if (x.IsCompleted)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            var (removed, newest, modified) = x.Result;
                            using (var m3uRemovedMedias = new Playlist<TvgMedia>(new M3uProvider(new FileStream(@"C:\Users\Heni\Downloads\m3uRemovedMedias.m3u", FileMode.Create, FileAccess.Write, FileShare.None))) { Name = "m3uRemovedMedias", Id = 2 })
                            {
                                m3uRemovedMedias.PushAsync(removed, ts.Token).Wait();
                            }

                            Action<string> logAction = m => Logger(nameof(Program)).LogInformation(m);
                            DisplayList(removed, logAction, "Removed medias");
                            Console.ForegroundColor = ConsoleColor.Green;
                            DisplayList(newest, logAction, "Newest medias");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            DisplayList(modified, logAction, "Modified medias");
                            Console.ResetColor();
                        }
                    });
                }
            }
        }

        private static async Task<IEnumerable<TvgMedia>> LoadPlaylist(CancellationToken ts)
        {
            using (var pl = new Playlist<TvgMedia>(new M3uProvider(new FileStream(m3uFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 4096))))
                return await pl.PullAsync(ts);
        }

        private static async Task LoadTvlistPlaylist(string filePath, CancellationToken ts)
        {
            using (var tvlistFile = new Playlist<TvgMedia>(new M3uProvider(new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))))
            {
                await tvlistFile.PullAsync(ts);
            }
        }

        private static async Task ExportTvListToM3u(string filePath, CancellationToken ts)
        {
            using (var tvlistFile = new Playlist<TvgMedia>(new TvlistProvider(new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))))
            {
                var res = await tvlistFile.PullAsync(ts);
                using (var m3uRemovedMedias = new Playlist<TvgMedia>(new M3uProvider(new FileStream("result.m3u", FileMode.Create, FileAccess.Write, FileShare.None))) { Name = "m3uRemovedMedias", Id = 2 })
                {
                    m3uRemovedMedias.PushAsync(tvlistFile, ts).Wait();
                }
            }
        }

    }
}
