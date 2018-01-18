using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using hfa.Synker.Services.Entities.Messages;
using hfa.Synker.Services.Messages;

namespace hfa.synker.batch.MoqServices
{
    internal class MoqMessageService : IMessageService
    {
        public async Task<HttpResponseMessage> PostAsync(object obj, string url, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
             {
                 Console.WriteLine(obj?.ToString());
                 return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
             });
        }

        public async Task SendAsync(Message message, CancellationToken cancellationToken)
        {
            await Task.Run(() => Console.WriteLine(message.Content));
        }

        public async Task SendAsync(string message, MessageTypeEnum messageTypeype, CancellationToken cancellationToken)
        {
            await Task.Run(() => Console.WriteLine(message));
        }

        public Task SendAsync(Message message, string username, string password, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task SendAsync(string message, MessageTypeEnum messageTypeype, string username, string password, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
