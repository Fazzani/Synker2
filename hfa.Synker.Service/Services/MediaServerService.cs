namespace hfa.Synker.Service.Services
{
    using hfa.synker.entities.MediaServer;
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class MediaServerService
    {
        private readonly HttpClient _httpClient;

        public MediaServerService(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<MediaServerStats> GetServerStatsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("/api/v1/nms/server", cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MediaServerStats>(result);
        }

        public async Task<MediaServerStreamsStats> GetServerStreamsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("/api/v1/nms/streams", cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MediaServerStreamsStats>(result);
        }

        public async Task<MediaServerLiveResponse> PublishLiveAsync(string streamSource, string streamId, CancellationToken cancellationToken)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new { stream = new { url = streamSource, streamId } }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/v1/ffmpeg/live", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MediaServerLiveResponse>(result);
        }

        public async Task<MediaServerStopLiveResponse> StopLiveAsync(string streamId, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"/api/v1/ffmpeg/stop/{streamId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MediaServerStopLiveResponse>(result);
        }
    }
}
