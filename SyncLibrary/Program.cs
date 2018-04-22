using Microsoft.Extensions.DependencyInjection;
using hfa.Brokers.Messages.Configuration;
using Hfa.SyncLibrary;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using System.Runtime.Loader;
using hfa.Notification.Brokers.Consumers;
using System.Threading.Tasks;
using Hfa.SyncLibrary.Infrastructure;

namespace hfa.Synker.batch
{
    public class Program
    {
        private static string MailQueueName = Init.IsDev ? "synker.dev.mail.queue" : "synker.mail.queue";
        private static ILogger _logger;
        private static IOptions<RabbitMQConfiguration> _rabbitConfig;
        private static INotificationConsumer _notificationConsumer;
        private static IWebGrabDockerConsumer _webGrabDockerConsumer;
        public static ManualResetEvent _Shutdown = new ManualResetEvent(false);
        public static ManualResetEventSlim _Complete = new ManualResetEventSlim();

        static int Main(string[] args)
        {
            Init.Build();

            var ended = new ManualResetEventSlim();
            var starting = new ManualResetEventSlim();

            // Capture SIGTERM  
            AssemblyLoadContext.Default.Unloading += Default_Unloading;

            _logger = SyncLibrary.Global.Common.Logger(nameof(Program));
            _rabbitConfig = Init.ServiceProvider.GetService<IOptions<RabbitMQConfiguration>>();
            _notificationConsumer = Init.ServiceProvider.GetService<INotificationConsumer>();
            _webGrabDockerConsumer = Init.ServiceProvider.GetService<IWebGrabDockerConsumer>();

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
                    Task.WaitAll(Task.Run(()=> _notificationConsumer.Start(connection, _Shutdown)), 
                        Task.Run(() => _webGrabDockerConsumer.Start(connection, _Shutdown)));
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

        private static void Default_Unloading(AssemblyLoadContext obj)
        {
            _logger.LogInformation($"Shutting down in response to SIGTERM.");
            _Shutdown.Set();
            _Complete.Wait();
        }
    }
}
