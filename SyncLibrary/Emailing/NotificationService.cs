using hfa.Brokers.Messages.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Notification.Brokers.Emailing
{
    public class NotificationService : INotificationService
    {
        private ILogger _logger;
        private MailOptions _mailOptions;

        public NotificationService(IOptions<MailOptions> options, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(NotificationService));
            _mailOptions = options.Value;
        }

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="emailNotification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SendMailAsync(EmailNotification emailNotification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(_mailOptions.ToString());

            using (var client = new SmtpClient(_mailOptions.SmtpServer, _mailOptions.SmtpPort)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_mailOptions.Username, _mailOptions.Password)
            })
            {

                var mailMessage = new MailMessage
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
