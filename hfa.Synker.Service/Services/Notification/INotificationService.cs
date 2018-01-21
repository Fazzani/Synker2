using hfa.Synker.Service.Entities.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Synker.Service.Services.Notification
{
    public interface INotificationService
    {
        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="emailNotification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SendMailAsync(EmailNotification emailNotification, CancellationToken cancellationToken);
        Task SendPushAsync(CancellationToken cancellationToken);
        Task SendPushBrowerAsync(CancellationToken cancellationToken);
        Task SendSmsAsync(CancellationToken cancellationToken);
    }
}