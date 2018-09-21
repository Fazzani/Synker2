//namespace hfa.WebApi.Consumers
//{
//    using hfa.Brokers.Messages.Contracts;
//    using hfa.Synker.Services;
//    using hfa.Synker.Services.Dal;
//    using hfa.WebApi.Models.Notifications;
//    using MassTransit;
//    using Microsoft.AspNetCore.Hosting;
//    using Microsoft.EntityFrameworkCore;
//    using Microsoft.Extensions.Logging;
//    using System;
//    using System.Linq;
//    using System.Net.Http;
//    using System.Threading.Tasks;
//    using Web;

//    public class DiffPlaylistConsumer : IConsumer<DiffPlaylistEvent>
//    {
//        private readonly ILogger<DiffPlaylistConsumer> _logger;
//        private readonly SynkerDbContext _dbContext;
//        private readonly string requestUri = $"api/v1/notification/push";

//        public DiffPlaylistConsumer(ILogger<DiffPlaylistConsumer> logger, SynkerDbContext context)
//        {
//            _logger = logger;
//            _dbContext = context;
//        }

//        public async Task Consume(ConsumeContext<DiffPlaylistEvent> context)
//        {
//            _logger.LogInformation(context.Message.ToString());

//            var playlist = await _dbContext.Playlist
//                .Include(pl => pl.User)
//                .FirstOrDefaultAsync(x => x.Id == context.Message.Id);

//            using (var client = new HttpClient())
//            {
//                ConfigureClient(client);
//                var response = await client.PostAsync(requestUri, new JsonContent(new BorkerMessageModel
//                {
//                    BrokerMessageType = BrokerMessageType.PlaylistDiff,
//                    Message = context.Message.ToString(),
//                    Id = playlist.Id
//                }));
//                response.EnsureSuccessStatusCode();
//            }
//        }

//        private static void ConfigureClient(HttpClient client)
//        {
//            string urls = Program.SynkerWebHostBuilder.GetSetting(WebHostDefaults.ServerUrlsKey);
//            string apiUrl = urls.Split(',').FirstOrDefault().Replace("*", "127.0.0.1");
//            client.BaseAddress = new Uri(apiUrl);
//        }
//    }
//}
