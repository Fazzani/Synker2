using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PlaylistManager.Entities;
using System.IO;
using System.Linq;
using PlaylistBaseLibrary.Entities;
using System.ComponentModel;
using System.Linq.Expressions;

namespace hfa.PlaylistBaseLibrary.Providers
{
    public class M3uProvider : FileProvider, IMediaFormatter
    {
        public const string HeaderFile = "#EXTM3U";
        public const string HeaderUrl = "#EXTINF:";

        public M3uProvider(Uri uri) : base(uri)
        {
        }

        protected override IEnumerable<TvgMedia> PullMediasFromProvider(StreamReader streamReader)
        {
            var mediasList = new List<TvgMedia>();
            var line = streamReader.ReadLine();
            if (line != null)
            {
                var i = 1;
                var isM3u = line.Equals(HeaderFile, StringComparison.OrdinalIgnoreCase);
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (line != string.Empty && line.StartsWith(HeaderUrl))
                    {
                        var tab1 = line.Split(',');
                        var tab2 = tab1[0].Split(' ');
                        var live = tab2.FirstOrDefault().Equals(HeaderUrl + "0") || tab2.FirstOrDefault().Equals(HeaderUrl + "-1");
                        var channel = new TvgMedia
                        {
                            Name = tab1.Last().Trim(),
                            Position = i++,
                            //IsLive = live,
                            //OriginalHeaderLine = line
                        };

                        do
                        {
                            channel.Url = streamReader.ReadLine();
                        } while (string.IsNullOrWhiteSpace(channel.Url));
                        GetTvg(tab1, channel);
                        yield return channel;
                    }

                }
            }
        }

        protected override async Task<IEnumerable<TvgMedia>> PullMediasFromProviderAsync(StreamReader streamReader, CancellationToken cancellationToken)
        {
            var listChannels = new List<TvgMedia>();
            using (var stringReader = new StringReader(await streamReader.ReadToEndAsync()))
            {
                var line = stringReader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    return new Playlist<TvgMedia>(listChannels);
                var i = 1;
                var isM3u = line.Equals(HeaderFile, StringComparison.OrdinalIgnoreCase);
                while ((line = await stringReader.ReadLineAsync()) != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (line != string.Empty && line.StartsWith(HeaderUrl))
                    {
                        var tab1 = line.Split(',');
                        var tab2 = tab1[0].Split(' ');
                        var live = tab2.FirstOrDefault().Equals(HeaderUrl + "0") || tab2.FirstOrDefault().Equals(HeaderUrl + "-1");
                        var channel = new TvgMedia
                        {
                            Name = tab1.Last().Trim(),
                            Position = i++,
                            //IsLive = live,
                            //OriginalHeaderLine = line
                        };

                        do
                        {
                            channel.Url = await stringReader.ReadLineAsync();
                        } while (string.IsNullOrWhiteSpace(channel.Url));
                        channel.StreamId = channel.StreamId;
                        GetTvg(tab1, channel);
                        listChannels.Add(channel);
                    }

                }
            }
            return new Playlist<TvgMedia>(listChannels);
        }

        private static void GetTvg(string[] tab1, TvgMedia channel)
        {
            foreach (var item in tab1[0].Split(' '))
            {
                var tabTags = item.Split('=');
                if (tabTags.Length > 1)
                {
                    var value = tabTags[1].Replace("\"", "");

                    if (item.Trim().StartsWith("tvg-id"))
                        channel.Tvg.TvgIdentify = value;
                    else
                    if (item.Trim().StartsWith("tvg-logo"))
                        channel.Tvg.Logo = value;
                    else
                    if (item.Trim().StartsWith("tvg-name"))
                        channel.Tvg.Name = value;
                    else
                    if (item.Trim().StartsWith("group-title"))
                        channel.MediaGroup = new Entities.MediaGroup(value);
                    //else
                    //    channel.ExtendedProperties.Add(tabTags[0].Replace("\"", "").Replace("ext-", string.Empty), value);
                }
            }
        }

        protected sealed override string GetDataToPushed(Playlist<TvgMedia> playlist)
        {
            var sb = new StringBuilder(HeaderFile);
            sb.Append(Environment.NewLine);
            //TODO: FIX ToList (QueryProvider not working on where)
            var list = playlist.ToList().Select(x => sb.Append(x.Format(this))).ToList();
            if (list.Any())
                return sb.ToString();
            return string.Empty;
        }

        protected sealed override async Task<string> GetDataToPushedAsync(Playlist<TvgMedia> playlist, CancellationToken token)
        {
            return await Task.Run(() => GetDataToPushed(playlist));
        }

        public override Playlist<TvgMedia> Sync(Playlist<TvgMedia> playlist)
        {
            throw new NotImplementedException();
        }

        public override Task<Playlist<TvgMedia>> SyncAsync(Playlist<TvgMedia> playlist, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public string Format(Media media) => media is TvgMedia ?
         $"#EXTINF:{(Byte)media.MediaType} tvg-id=\"{((TvgMedia)media).Tvg?.Id}\" tvg-logo=\"{((TvgMedia)media).Tvg?.Logo}\" tvg-name=\"{((TvgMedia)media).Tvg?.Name}\" audio-track=\"{((TvgMedia)media).Tvg?.Audio_track}\" tvg-shift=\"{((TvgMedia)media).Tvg?.Shift}\" aspect-ratio=\"{((TvgMedia)media).Tvg?.Aspect_ratio}\" group-title=\"{media.MediaGroup?.Name}\", {media.DisplayName.Trim()}{ Environment.NewLine}{media.Url}{ Environment.NewLine}" :
            $"#EXTINF:{(Byte)media.MediaType} group-title=\"{media.MediaGroup?.Name}\", {media.DisplayName.Trim()}{Environment.NewLine}{media.Url}{Environment.NewLine}";

    }
}
