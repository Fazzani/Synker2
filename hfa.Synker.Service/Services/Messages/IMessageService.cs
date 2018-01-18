using hfa.Synker.Services.Entities.Messages;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Synker.Services.Messages
{
    public interface IMessageService
    {
        Task<HttpResponseMessage> PostAsync(object obj, string url, CancellationToken cancellationToken);
        Task SendAsync(Message message, CancellationToken cancellationToken);
        Task SendAsync(string message, MessageTypeEnum messageTypeype, string username, string password, CancellationToken cancellationToken);
        Task SendAsync(string message, MessageTypeEnum messageTypeype, CancellationToken cancellationToken);
        Task SendAsync(Message message, string username, string password, CancellationToken cancellationToken);
    }
}