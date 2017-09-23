using hfa.SyncLibrary.Global;
using Hfa.SyncLibrary.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;

namespace Hfa.SyncLibrary.Messages
{
    class MessagesService : IMessagesService
    {
        IOptions<ApplicationConfigData> _config;
        ILogger Logger;

        public MessagesService(IOptions<ApplicationConfigData> config, ILoggerFactory loggerFactory)
        {
            this._config = config;
            Logger = loggerFactory.CreateLogger(typeof(IMessagesService));
        }

        /// <summary>
        /// Send Message to api
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SendAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                var response = await PostAsync(message, _config.Value.ApiUrlMessage, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogError(response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
            }
        }

        /// <summary>
        /// Send Message to api
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SendAsync(string message, string type, CancellationToken cancellationToken) =>
             SendAsync(new Message { Content = message, Type = type, Status = MessageStatus.NotReaded, TimeStamp = DateTime.Now }, cancellationToken);

        /// <summary>
        /// Post
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="url"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostAsync(object obj, string url, CancellationToken cancellationToken)
        {
            //options.Value.ApiUrl
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("app_id", "Synker_batch_v1");

                return await client.PostAsync(url, new JsonContent(obj), cancellationToken);
            }
        }
    }
}
