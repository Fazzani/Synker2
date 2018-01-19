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
using System.Net.Http;
using System.Text.RegularExpressions;

namespace hfa.PlaylistBaseLibrary.Providers
{
    public class XtreamProvider : ApiProvider, IMediaFormatter
    {
        private const string XtreamUrlPattern = @"^(?<portocol>https?)://(?<host>.*):(?<port>\d{2,4})/get\.php\?username=(?<username>.*)&password=(?<password>\w+)";
        XtreamPanel _panel;
        public XtreamProvider(string url) : base(url)
        {
        }

        public override IEnumerable<TvgMedia> Pull()
        {
            _panel = _panel ?? GetApiResultAsync(CancellationToken.None).GetAwaiter().GetResult();
            if (_panel.Available_Channels != null)
                return _panel.Available_Channels.Select(x => Channels.ToTvgMedia(x, y => _panel.GenerateUrlFrom(x)));
            return null;
        }

        public override async Task<IEnumerable<TvgMedia>> PullAsync(CancellationToken cancellationToken)
        {
            _panel = _panel ?? await GetApiResultAsync(CancellationToken.None);
            if (_panel.Available_Channels != null)
                return _panel.Available_Channels.Select(x => Channels.ToTvgMedia(x, y => _panel.GenerateUrlFrom(x)));
            return null;
        }

        private async Task<XtreamPanel> GetApiResultAsync(CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            {
                var xtreamUrl = GetApiUrl(Url);
                var json = await client.GetStringAsync(new Uri(xtreamUrl));
                json = Regex.Replace(json, "\"\\d+\":{", "{", RegexOptions.Multiline);
                json = json.Replace("\"available_channels\":{", "\"available_channels\":[");
                json = json.Replace("}}}", "}]}");
                cancellationToken.ThrowIfCancellationRequested();
                return JsonConvert.DeserializeObject<XtreamPanel>(json);
            }
        }

        internal string GetApiUrl(string playlistUrl)
        {
            var reg = new Regex(XtreamUrlPattern, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase).Match(playlistUrl);
            if (reg.Success)
            {
                return $"{reg.Groups["portocol"]}://{reg.Groups["host"]}:{reg.Groups["port"]}/panel_api.php?username={reg.Groups["username"]}&password={reg.Groups["password"]}";
            }
            else
            {
                throw new ApplicationException("Xtream playlist format not valid");
            }
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
