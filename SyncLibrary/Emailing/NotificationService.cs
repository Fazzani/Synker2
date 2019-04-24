namespace hfa.Notification.Brokers.Emailing
{
    using hfa.Brokers.Messages.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using System.Net;
    using System.Net.Mail;
    using System.Threading;
    using System.Threading.Tasks;

    public class NotificationService : INotificationService
    {
        private readonly ILogger _logger;
        private readonly MailOptions _mailOptions;
        private readonly EmailSettings _sendGridOptions;

        public NotificationService(IOptions<MailOptions> options, IOptions<EmailSettings> sendGridOptions, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(NotificationService));
            _mailOptions = options.Value;
            _sendGridOptions = sendGridOptions.Value;
        }

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="emailNotification"></param>
        /// <param name="cancellationToken"></param>
        public Task SendMailAsync(EmailNotification emailNotification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(_mailOptions.ToString());

            return emailNotification.IsWithTemplate ? SendEmailTemplateAsync(emailNotification, cancellationToken) :
                SimpleSendEmailAsync(emailNotification, cancellationToken);
        }

        private Task SimpleSendEmailAsync(EmailNotification emailNotification, CancellationToken cancellationToken)
        {
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

                return client.SendMailAsync(mailMessage);
            }
        }

        /// <summary>
        /// Send email based on sendgrid template
        /// </summary>
        private async Task SendEmailTemplateAsync(EmailNotification emailNotification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Sending email with template {emailNotification}");

            var client = new SendGridClient(_sendGridOptions.SendGridApiKey);
            var msg = new SendGridMessage();
            msg.SetFrom(new EmailAddress(_sendGridOptions.SenderEmailAddress, "Synker"));
            msg.AddTo(new EmailAddress(emailNotification.To));
            msg.SetSubject(emailNotification.Subject);
            msg.SetTemplateId(emailNotification.TemplateId);
            msg.SetTemplateData(emailNotification.TemplateData);
            //msg.AddContent(MimeType.Text, message);
            //msg.AddContent(MimeType.Html, message);

            msg.SetReplyTo(new EmailAddress(_sendGridOptions.SenderEmailAddress, "Synker"));

            //TODO: Polly
            var response = await client.SendEmailAsync(msg, cancellationToken);
            _logger.LogInformation($"Sending email with template response status: {response.StatusCode}");
        }

        public Task SendPushBrowerAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task SendPushAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task SendSmsAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
