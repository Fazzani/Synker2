namespace hfa.Synker.batch
{
    using Microsoft.Extensions.DependencyInjection;
    using hfa.Brokers.Messages.Configuration;
    using Hfa.SyncLibrary;
    using Microsoft.Extensions.Options;
    using RabbitMQ.Client;
    using System;
    using System.Threading;
    using Microsoft.Extensions.Logging;
    using System.Runtime.Loader;
    using hfa.Notification.Brokers.Consumers;
    using System.Threading.Tasks;
    using hfa.Synker.batch.Producers;

    public class Program
    {
        private static ILogger _logger;
        private static IOptions<RabbitMQConfiguration> _rabbitConfig;
        private static INotificationConsumer _notificationConsumer;
        private static IWebGrabDockerConsumer _webGrabDockerConsumer;
        private static IWebGrabDockerProducer _webGrabDockerProducer;
        public static ManualResetEvent _Shutdown = new ManualResetEvent(false);
        public static ManualResetEventSlim _Complete = new ManualResetEventSlim();
        private static IConnection _connection;
        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

#if DEBUG
        const double _24H = 1000 * 60 * 60; // Toutes les heures
#else
        const double _24H = 1000 * 60 * 60 * 24;
#endif

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
            _webGrabDockerProducer = Init.ServiceProvider.GetService<IWebGrabDockerProducer>();

            _logger.LogInformation("starting consumption");

            var factory = new ConnectionFactory()
            {
                HostName = _rabbitConfig.Value.Hostname,
                Port = _rabbitConfig.Value.Port,
                UserName = _rabbitConfig.Value.Username,
                Password = _rabbitConfig.Value.Password,
                //ContinuationTimeout = new TimeSpan(0, 0, 60),
                RequestedConnectionTimeout = 90_000,
                VirtualHost = Init.IsDev ? "/dev" : "/"
            };

            try
            {
                _logger.LogDebug($"Rabbitmq : {_rabbitConfig.Value.Username} : {_rabbitConfig.Value.Password}");
                _connection = factory.CreateConnection();
                _connection.CallbackException += _connection_CallbackException;
                _logger.LogDebug($"Connected to rabbit host: {factory.HostName}{factory.VirtualHost}:{_rabbitConfig.Value.Port}");

                var timer = new System.Timers.Timer
                {
                    Interval = _24H
                };

                timer.Elapsed += TimerElapsedWebGrabProducer;
                timer.Start();

                Task.WaitAll(
                    Task.Run(() => _notificationConsumer.Start(_connection, _Shutdown), _cancellationTokenSource.Token),
                    Task.Run(() => _webGrabDockerConsumer.Start(_connection, _Shutdown), _cancellationTokenSource.Token),
                    _webGrabDockerProducer.StartAsync(_connection, _cancellationTokenSource.Token));
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, e.Message);
            }

            _logger.LogInformation("Batch exiting...");
            _Complete.Set();

            return 0;
        }

        private static void _connection_CallbackException(object sender, RabbitMQ.Client.Events.CallbackExceptionEventArgs e) =>
            _logger.LogError(e.Exception, e.Exception.Message);

        private static async void TimerElapsedWebGrabProducer(object sender, System.Timers.ElapsedEventArgs e) =>
            await _webGrabDockerProducer.StartAsync(_connection, _cancellationTokenSource.Token);

        private static void Default_Unloading(AssemblyLoadContext obj)
        {
            _logger?.LogWarning($"Shutting down in response to SIGTERM.");
            _Shutdown?.Set();
            _cancellationTokenSource?.Cancel();
            _connection?.Close(1, "Application Shutting down [SIGTERM received].");
            _Complete?.Wait();
        }
    }
}
