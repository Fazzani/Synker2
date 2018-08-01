namespace hfa.Synker.Service.Services
{
    using hfa.synker.entities.MediaServer;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
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
            var response = await _httpClient.GetAsync("/api/server", cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MediaServerStats>(result);
        }

        public async Task<MediaServerStreamsStats> GetServerStreamsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("/api/streams", cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MediaServerStreamsStats>(result);
        }
    }
}
