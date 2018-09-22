namespace hfa.Notification.Brokers.Consumers
{
    using hfa.Brokers.Messages.Contracts;
    using hfa.Notification.Brokers.Emailing;
    using MassTransit;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Firebase.Database;
    using Microsoft.Extensions.Options;
    using hfa.Brokers.Messages.Configuration;

    /// <summary>
    /// save message into firebase database
    /// </summary>
    public class RabbitNotificationConsumer : IConsumer<DiffPlaylistEvent>
    {
        private readonly ILogger _logger;
        private readonly FirebaseConfiguration _firebaseOptions;
        private readonly IBusControl _bus;

        public RabbitNotificationConsumer(INotificationService notificationService, ILogger<RabbitSynchronizeConsumer> logger,
            IOptions<FirebaseConfiguration> firebaseOptions, IBusControl bus)
        {
            _logger = logger;
            _firebaseOptions = firebaseOptions.Value;
            _bus = bus;
        }

        public async Task Consume(ConsumeContext<DiffPlaylistEvent> context)
        {
            try
            {
                _logger.LogInformation($"{nameof(RabbitNotificationConsumer)}: New message {context.CorrelationId} poped");
                var message = context.Message.ToString();
                if (context.Message.Changed)
                {
                    await SaveAsync(context.Message);
                }
                _logger.LogInformation(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        private async Task SaveAsync(DiffPlaylistEvent message, CancellationToken cancellationToken = default)
        {
            var firebase = new FirebaseClient(_firebaseOptions.DatabaseURL, new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(_firebaseOptions.Secret)
            });

            await _bus.Publish(new TraceEvent
            {
                Message = $"The Playlist {message.PlaylistName} changment detected. {message.NewMediasCount} new medias wad founded and {message.RemovedMediasCount} was removed.",
                Level = TraceEvent.LevelTrace.Info,
                UserId = message.UserId,
                Source = nameof(RabbitNotificationConsumer)
            }, CancellationToken.None);
        }
    }
}
