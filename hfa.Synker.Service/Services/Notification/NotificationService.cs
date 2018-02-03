using hfa.Brokers.Messages.Configuration;
using hfa.Brokers.Messages.Emailing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
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
        private IConnectionFactory _connectionFactory;
        private ILogger _logger;
        const string MailQueueName = "synker.mail.queue";

        public NotificationService(IOptions<RabbitMQConfiguration> rabbitmqOptions, ILoggerFactory loggerFactory)
        {
            _connectionFactory = new ConnectionFactory()
            {
                HostName = rabbitmqOptions.Value.Hostname,
                Port = rabbitmqOptions.Value.Port,
                UserName = rabbitmqOptions.Value.Username,
                Password = rabbitmqOptions.Value.Password
            };
            _logger = loggerFactory.CreateLogger(typeof(INotificationService));
        }

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="emailNotification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SendMailAsync(EmailNotification emailNotification, CancellationToken cancellationToken)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: MailQueueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                    String jsonified = JsonConvert.SerializeObject(emailNotification);
                    byte[] customerBuffer = Encoding.UTF8.GetBytes(jsonified);

                    channel.BasicPublish(exchange: "",
                                         routingKey: MailQueueName,
                                         basicProperties: null,
                                         body: customerBuffer);

                    _logger.LogInformation($" [x] Sent {emailNotification.Subject}");
                }
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
