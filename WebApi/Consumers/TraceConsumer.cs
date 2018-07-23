namespace hfa.WebApi.Consumers
{
    using hfa.Brokers.Messages.Contracts;
    using hfa.Synker.Services;
    using hfa.WebApi.Models.Notifications;
    using MassTransit;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Web;

    public class TraceConsumer :  IConsumer<TraceEvent>
    {
        private readonly ILogger<TraceConsumer> _logger;
        private readonly string requestUri = $"api/v1/notification/push";

        public TraceConsumer(ILogger<TraceConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TraceEvent> context)
        {
            _logger.LogCritical(context.Message.ToString());
            using (var client = new HttpClient())
            {
                Configureclient(client);
                var response = await client.PostAsync(requestUri, new JsonContent(new BorkerMessageModel
                {
                    BrokerMessageType = BrokerMessageType.Exception,
                    Message = context.Message.ToString()
                }));
                response.EnsureSuccessStatusCode();
            }
        }

        private static void Configureclient(HttpClient client)
        {
            string urls = Program.SynkerWebHostBuilder.GetSetting(WebHostDefaults.ServerUrlsKey);
            string apiUrl = urls.Split(',').FirstOrDefault().Replace("*", "127.0.0.1");
            client.BaseAddress = new Uri(apiUrl);
        }
    }
}
