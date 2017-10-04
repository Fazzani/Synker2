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

        public TvlistProvider(Stream sr) : base(sr)
        {
        }
       
        public override IEnumerable<TvgMedia> Pull()
        {
            var listChannels = new List<TvgMedia>();
            using (var streamReader = new StreamReader(_sr, Encoding.UTF8, false, BufferSize, true))
            {
                var position = 1;
                var line = string.Empty;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    position = FillTvgMedia(listChannels, line, position);
                }
            }
            return new Playlist<TvgMedia>(listChannels);
        }

        public override async Task<IEnumerable<TvgMedia>> PullAsync(CancellationToken cancellationToken)
        {
            var listChannels = new List<TvgMedia>();
            using (var streamReader = new StreamReader(_sr, Encoding.UTF8, false, BufferSize, true))
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
            _sr.Seek(0, SeekOrigin.Begin);
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

        public override void Push(Playlist<TvgMedia> playlist)
        {
            if (playlist == null)
                throw new ArgumentNullException(nameof(playlist));
            var sb = new StringBuilder();
            var list = playlist.Where(x => x.Enabled).AsParallel().Select(x => sb.Append(x.Format(this))).ToList();
            if (list.Any())
                using (var sw = new StreamWriter(_sr, Encoding.UTF8, BufferSize, true))
                {
                    sw.Write(sb.ToString());
                }
        }

        public override async Task PushAsync(Playlist<TvgMedia> playlist, CancellationToken token)
        {
            if (playlist == null)
                throw new ArgumentNullException(nameof(playlist));

            var sb = new StringBuilder();
            var list = playlist.Where(x => x.Enabled).Select(x => sb.Append(x.Format(this))).ToList();
            if (list.Any())
                using (var sw = new StreamWriter(_sr, Encoding.UTF8, BufferSize, true))
                {
                    await sw.WriteAsync(sb.ToString());
                }
            //return null;
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
