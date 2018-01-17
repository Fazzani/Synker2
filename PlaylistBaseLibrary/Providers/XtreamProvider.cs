using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PlaylistManager.Entities;
using System.IO;
using System.Linq;
using PlaylistBaseLibrary.Entities;
using Newtonsoft.Json;
using hfa.PlaylistBaseLibrary.Entities.XtreamCode;

namespace hfa.PlaylistBaseLibrary.Providers
{
    public class XtreamProvider : FileProvider, IMediaFormatter
    {
        public XtreamProvider(Stream sr) : base(sr)
        {
        }

        public override IEnumerable<TvgMedia> Pull()
        {
            _sr.Seek(0, SeekOrigin.Begin);
            var streamJson = new StreamReader(_sr, Encoding.UTF8, false, 4096, true).ReadToEnd();
            var xtreamPanel = JsonConvert.DeserializeObject<XtreamPanel>(streamJson);
            return xtreamPanel.Available_channels.Channels.Select(x => Channels.ToTvgMedia(x, y => xtreamPanel.GenerateUrlFrom(x)));
        }

        public override async Task<IEnumerable<TvgMedia>> PullAsync(CancellationToken cancellationToken)
        {
            _sr.Seek(0, SeekOrigin.Begin);
            var streamJson = await new StreamReader(_sr, Encoding.UTF8, false, 4096, true).ReadToEndAsync();
            var xtreamPanel = JsonConvert.DeserializeObject<XtreamPanel>(streamJson);
            return xtreamPanel.Available_channels.Channels.Select(x => Channels.ToTvgMedia(x, y => xtreamPanel.GenerateUrlFrom(x)));
        }

        public override void Push(Playlist<TvgMedia> playlist)
        {
            throw new NotImplementedException();
        }

        public override async Task PushAsync(Playlist<TvgMedia> playlist, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public override Playlist<TvgMedia> Sync(Playlist<TvgMedia> playlist)
        {
            throw new NotImplementedException();
        }

        public override Task<Playlist<TvgMedia>> SyncAsync(Playlist<TvgMedia> playlist, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public string Format(Media media) => Channels.TvgMediaToChannelsJson(media as TvgMedia);

    }
}
