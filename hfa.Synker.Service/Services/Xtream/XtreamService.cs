using hfa.PlaylistBaseLibrary.Entities.XtreamCode;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Synker.Service.Services.Xtream
{
    public class XtreamService : IXtreamService
    {
        public async Task<PlayerApi> GetUserAndServerInfoAsync(string playlistUrl, CancellationToken cancellationToken) => 
            await GetFromApi<PlayerApi>(playlistUrl, XtreamApiEnum.Player_Api, cancellationToken);

        public async Task<XtreamPanel> GetPanelAsync(string playlistUrl, CancellationToken cancellationToken) =>
           await GetFromApi<XtreamPanel>(playlistUrl, XtreamApiEnum.Panel_Api, cancellationToken);

        public async Task<List<Channels>> GetVodStreamsAsync(string playlistUrl, CancellationToken cancellationToken) =>
           await GetFromApi<List<Channels>>(playlistUrl, XtreamApiEnum.VOD_Streams, cancellationToken);

        public async Task<List<Channels>> GetLiveStreamsAsync(string playlistUrl, CancellationToken cancellationToken) =>
           await GetFromApi<List<Channels>>(playlistUrl, XtreamApiEnum.LiveStreams, cancellationToken);

        public async Task<List<Channels>> GetLiveStreamsByCategoriesAsync(string playlistUrl, string categoryId, CancellationToken cancellationToken) =>
           await GetFromApi<List<Channels>>(playlistUrl, XtreamApiEnum.LiveStreamsByCategories, cancellationToken, categoryId);

        public async Task<List<Live>> GetLiveCategoriesAsync(string playlistUrl, CancellationToken cancellationToken) =>
           await GetFromApi<List<Live>>(playlistUrl, XtreamApiEnum.LiveCategories, cancellationToken);

        public async Task<List<Epg_Listings>> GetAllEpgAsync(string playlistUrl, CancellationToken cancellationToken) =>
           await GetFromApi<List<Epg_Listings>>(playlistUrl, XtreamApiEnum.AllEpg, cancellationToken);
        public async Task<List<Epg_Listings>> GetShortEpgForStreamAsync(string playlistUrl, string streamId, CancellationToken cancellationToken) =>
           await GetFromApi<List<Epg_Listings>>(playlistUrl, XtreamApiEnum.ShortEpgForStream, cancellationToken, streamId);

        public async Task<PlayerApi> GetXmltvAsync(string playlistUrl, CancellationToken cancellationToken) =>
           await GetFromApi<PlayerApi>(playlistUrl, XtreamApiEnum.Xmltv, cancellationToken);

        internal async Task<T> GetFromApi<T>(string playlistUrl, XtreamApiEnum xtreamApiEnum, CancellationToken cancellationToken, params string[] extraParams)
        {
            using (var client = new HttpClient())
            {
                var json = await client.GetStringAsync(new Uri(GetApiUrl(playlistUrl, xtreamApiEnum, extraParams)));
                cancellationToken.ThrowIfCancellationRequested();
                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        internal string GetApiUrl(string playlistUrl, XtreamApiEnum xtreamApiEnum, params string[] extraParams)
        {
            var reg = new Regex(@"^(?<portocol>https?)://(?<host>.*):(?<port>\d{2,4})/get\.php\?username=(?<username>.*)&password=(?<password>\w+)", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase).Match(playlistUrl);
            if (reg.Success)
            {
                switch (xtreamApiEnum)
                {
                    case XtreamApiEnum.Player_Api:
                        return $"{reg.Groups["portocol"]}://{reg.Groups["host"]}:{reg.Groups["port"]}/player_api.php?username={reg.Groups["username"]}&password={reg.Groups["password"]}";
                    case XtreamApiEnum.LiveCategories:
                        return $"{reg.Groups["portocol"]}://{reg.Groups["host"]}:{reg.Groups["port"]}/player_api.php?username={reg.Groups["username"]}&password={reg.Groups["password"]}&action=get_live_categories";
                    case XtreamApiEnum.LiveStreams:
                        return $"{reg.Groups["portocol"]}://{reg.Groups["host"]}:{reg.Groups["port"]}/player_api.php?username={reg.Groups["username"]}&password={reg.Groups["password"]}&action=get_live_streams";
                    case XtreamApiEnum.LiveStreamsByCategories:
                        return $"{reg.Groups["portocol"]}://{reg.Groups["host"]}:{reg.Groups["port"]}/player_api.php?username={reg.Groups["username"]}&password={reg.Groups["password"]}&action=get_live_streams&category_id={extraParams[0]}";
                    case XtreamApiEnum.Panel_Api:
                        return $"{reg.Groups["portocol"]}://{reg.Groups["host"]}:{reg.Groups["port"]}/panel_api.php?username={reg.Groups["username"]}&password={reg.Groups["password"]}";
                     case XtreamApiEnum.VOD_Streams:
                        return $"{reg.Groups["portocol"]}://{reg.Groups["host"]}:{reg.Groups["port"]}/player_api.php?username={reg.Groups["username"]}&password={reg.Groups["password"]}&action=get_vod_streams";
                    case XtreamApiEnum.ShortEpgForStream:
                        return $"{reg.Groups["portocol"]}://{reg.Groups["host"]}:{reg.Groups["port"]}/player_api.php?username={reg.Groups["username"]}&password={reg.Groups["password"]}&action=get_short_epg&stream_id={extraParams[0]}";
                    case XtreamApiEnum.AllEpg:
                        return $"{reg.Groups["portocol"]}://{reg.Groups["host"]}:{reg.Groups["port"]}/player_api.php?username={reg.Groups["username"]}&password={reg.Groups["password"]}&action=get_simple_data_table&stream_id={extraParams[0]}";
                    case XtreamApiEnum.Xmltv:
                        return $"{reg.Groups["portocol"]}://{reg.Groups["host"]}:{reg.Groups["port"]}/xmltv.php?username={reg.Groups["username"]}&password={reg.Groups["password"]}";
                    default:
                        return string.Empty;
                }
            }
            else
            {
                throw new ApplicationException("Xtream playlist format not valid");
            }
        }

        internal enum XtreamApiEnum : byte
        {
            Player_Api = 0,
            Panel_Api,
            VOD_Streams,
            LiveStreams,
            LiveStreamsByCategories,
            LiveCategories,
            AllEpg,
            
            ShortEpgForStream,
            Xmltv
        }
    }

}
