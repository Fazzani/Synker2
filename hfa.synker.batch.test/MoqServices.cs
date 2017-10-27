using Hfa.SyncLibrary.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.synker.batch.MoqServices
{
    internal class MoqMessageService : IMessagesService
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
    }
}
