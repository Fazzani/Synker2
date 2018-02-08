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
using System.Runtime.Loader;

namespace hfa.Notification.Brokers
{
    public class Program
    {
        private static string MailQueueName = Init.IsDev(Init.Enviroment) ? "synker.dev.mail.queue" : "synker.mail.queue";
        private static ILogger _logger;
        private static IOptions<RabbitMQConfiguration> _rabbitConfig;
        private static INotificationService _notifService;

        public static ManualResetEvent _Shutdown = new ManualResetEvent(false);
        public static ManualResetEventSlim _Complete = new ManualResetEventSlim();

        static int Main(string[] args)
        {
            Init.Build();

            var ended = new ManualResetEventSlim();
            var starting = new ManualResetEventSlim();

            // Capture SIGTERM  
            AssemblyLoadContext.Default.Unloading += Default_Unloading;

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

            try
            {
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
                            try
                            {
                                _logger.LogInformation($"New Mail poped from the queue {MailQueueName}");
                                var body = ea.Body;
                                var message = Encoding.UTF8.GetString(body);
                                var mail = JsonConvert.DeserializeObject<EmailNotification>(message);
                                _notifService.SendMailAsync(mail, CancellationToken.None).GetAwaiter().GetResult();
                                //ack message
                                mailChannel.BasicAck(ea.DeliveryTag, true);

                                _logger.LogInformation($"Mail from {mail.From} to {mail.To}");
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, e.Message);
                                mailChannel.BasicReject(ea.DeliveryTag, true);
                            }
                        };

                        mailChannel.BasicConsume(queue: MailQueueName,
                                                autoAck: false,
                                                consumer: mailConsumer);

                        while (!_Shutdown.WaitOne())
                        {
                            Thread.Sleep(1000);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            Console.Write("Exiting...");
            _Complete.Set();

            return 0;
        }

        private static void Channel_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _logger.LogError(e.Exception, e.Exception.Message);
        }

        private static void Default_Unloading(AssemblyLoadContext obj)
        {
            _logger.LogInformation($"Shutting down in response to SIGTERM.");
            _Shutdown.Set();
            _Complete.Wait();
        }
    }
}
