using Microsoft.Extensions.DependencyInjection;
using hfa.Brokers.Messages.Configuration;
using hfa.Brokers.Messages.Emailing;
using Hfa.SyncLibrary;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using hfa.Notification.Brokers.Emailing;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace hfa.Notification.Brokers
{
    public class Program
    {
        private const string MailQueueName = "synker.mail.queue";
        private static ILogger _logger;
        private static IOptions<RabbitMQConfiguration> _rabbitConfig;
        private static INotificationService _notifService;

        static void Main(string[] args)
        {
            Init.Build();
            _logger = Common.Logger(nameof(Program));
            _rabbitConfig = Init.ServiceProvider.GetService<IOptions<RabbitMQConfiguration>>();
            _notifService = Init.ServiceProvider.GetService<INotificationService>();

            _logger.LogInformation("starting consumption");

            var factory = new ConnectionFactory()
            {
                HostName = _rabbitConfig.Value.Hostname,
                Port = _rabbitConfig.Value.Port,
                UserName = _rabbitConfig.Value.Username,
                Password = _rabbitConfig.Value.Password
            };

            using (var connection = factory.CreateConnection())
            {
                using (var mailChannel = connection.CreateModel())
                {
                    mailChannel.QueueDeclare(queue: MailQueueName,
                                            durable: false,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

                    mailChannel.CallbackException += Channel_CallbackException;
                    var mailConsumer = new EventingBasicConsumer(mailChannel);

                    mailConsumer.Received += (model, ea) =>
                    {
                        _logger.LogInformation($"New Mail poped from the queue {MailQueueName}");
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        var mail = JsonConvert.DeserializeObject<EmailNotification>(message);
                        _notifService.SendMailAsync(mail, CancellationToken.None).GetAwaiter().GetResult();
                        _logger.LogInformation($"Mail from {mail.From} to {mail.To}");
                    };

                    mailChannel.BasicConsume(queue: MailQueueName,
                                            autoAck: true,
                                            consumer: mailConsumer);

                    do
                    {
                        Thread.Sleep(1000);
                    } while (true);

                }
            }
        }

        private static void Channel_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _logger.LogError(e.Exception, e.Exception.Message);
        }
    }
}
