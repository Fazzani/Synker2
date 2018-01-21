using hfa.Synker.Service.Entities.Notifications;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Synker.Service.Services.Notification
{
    public class NotificationService : INotificationService
    {
        private MailOptions _options;

        public NotificationService(IOptions<MailOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="emailNotification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SendMailAsync(EmailNotification emailNotification, CancellationToken cancellationToken)
        {
            using (var client = new SmtpClient(_options.SmtpServer, _options.SmtpPort)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_options.Username, _options.Password)
            })
            {

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(emailNotification.From, emailNotification.FromDisplayName)
                };

                mailMessage.To.Add(emailNotification.To);
                mailMessage.Body = emailNotification.Body;
                mailMessage.Subject = emailNotification.Subject;
                mailMessage.IsBodyHtml = emailNotification.IsBodyHtml;

                cancellationToken.ThrowIfCancellationRequested();

                await client.SendMailAsync(mailMessage);
            }
        }

        public async Task SendPushBrowerAsync(CancellationToken cancellationToken)
        {

        }
        public async Task SendPushAsync(CancellationToken cancellationToken)
        {

        }
        public async Task SendSmsAsync(CancellationToken cancellationToken)
        {

        }
    }
}
