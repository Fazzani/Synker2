namespace hfa.Synker.Service.Services
{
    using hfa.Brokers.Messages.Configuration;
    using hfa.Brokers.Messages.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using RabbitMQ.Client;
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class MessageQueueService : IMessageQueueService
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger _logger;
#if DEBUG
        private const string MailExchangeName = "synker.dev.mail.queue";
        private const string MailRoutingKey = "synker.dev.mail.*";
#else
        private const string MailExchangeName = "synker.mail.queue";
        private const string MailRoutingKey = "synker.mail.*";
#endif
        public MessageQueueService(IOptions<RabbitMQConfiguration> rabbitmqOptions, ILoggerFactory loggerFactory)
        {
            _connectionFactory = new ConnectionFactory()
            {
                HostName = rabbitmqOptions.Value.Hostname,
                Port = rabbitmqOptions.Value.Port,
                UserName = rabbitmqOptions.Value.Username,
                Password = rabbitmqOptions.Value.Password
            };
            _logger = loggerFactory.CreateLogger(typeof(IMessageQueueService));
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
                    var jsonified = JsonConvert.SerializeObject(emailNotification);
                    byte[] customerBuffer = Encoding.UTF8.GetBytes(jsonified);

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.ContentType = "application/json";
                    props.DeliveryMode = 2;
                    props.AppId = emailNotification.AppId;
                    props.UserId = emailNotification.UserId;
                    props.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToUnixTimestamp());

                    channel.ExchangeDeclare(MailExchangeName, ExchangeType.Direct);
                    channel.QueueDeclare(MailExchangeName, false, false, false, null);
                    channel.QueueBind(MailExchangeName, MailExchangeName, MailRoutingKey, null);

                    channel.BasicPublish(exchange: MailExchangeName,
                                         routingKey: MailRoutingKey,
                                         basicProperties: null,
                                         body: customerBuffer);

                    _logger.LogInformation($" [x] Sent {emailNotification.Subject}");
                }
            }
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
