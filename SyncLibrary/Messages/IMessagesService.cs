using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Hfa.SyncLibrary.Messages
{
    interface IMessagesService
    {
        Task<HttpResponseMessage> PostAsync(object obj, string url, CancellationToken cancellationToken);
        Task SendAsync(Message message, CancellationToken cancellationToken);
        Task SendAsync(string message, string id, CancellationToken cancellationToken);
    }
}