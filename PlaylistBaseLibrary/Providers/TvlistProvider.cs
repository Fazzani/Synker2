using PlaylistBaseLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using PlaylistManager.Entities;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace hfa.PlaylistBaseLibrary.Providers
{
    public class TvlistProvider : FileProvider, IMediaFormatter
    {
        private const int BufferSize = 4096;

        public TvlistProvider(Uri uri) : base(uri)
        {
        }
       
        protected sealed override IEnumerable<TvgMedia> PullMediasFromProvider(StreamReader streamReader)
        {
            var listChannels = new List<TvgMedia>();
                var position = 1;
                var line = string.Empty;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    position = FillTvgMedia(listChannels, line, position);
                }
            return new Playlist<TvgMedia>(listChannels);
        }

        protected sealed override async Task<IEnumerable<TvgMedia>> PullMediasFromProviderAsync(StreamReader streamReader, CancellationToken cancellationToken)
        {
            var listChannels = new List<TvgMedia>();
            using (var stringReader = new StringReader(await streamReader.ReadToEndAsync()))
            {
                var line = string.Empty;
                var position = 1;
                while ((line = stringReader.ReadLine()) != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    position = FillTvgMedia(listChannels, line, position);
                }
            }
            return new Playlist<TvgMedia>(listChannels);
        }

        private static int FillTvgMedia(List<TvgMedia> listChannels, string line, int position)
        {
            if (line != string.Empty)
            {
                var row = line.Split(' ');
                var channel = new TvgMedia
                {
                    Name = string.Join(" ", row.Take(row.Length - 1)),
                    Url = row.Last().Trim(),
                    Position = position++,
                };
                listChannels.Add(channel);
            }

            return position;
        }

        protected sealed override string GetDataToPushed(Playlist<TvgMedia> playlist)
        {
            var sb = new StringBuilder();
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

        public string Format(Media media) => 
            $"{media.Name.Trim()}{Environment.NewLine}{media.Url}{Environment.NewLine}";

    }
}
