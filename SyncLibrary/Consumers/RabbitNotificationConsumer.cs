namespace hfa.Notification.Brokers.Consumers
{
    using hfa.Brokers.Messages.Contracts;
    using hfa.Notification.Brokers.Emailing;
    using MassTransit;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Firebase.Database;
    using Firebase.Database.Query;
    using Microsoft.Extensions.Options;
    using hfa.Brokers.Messages.Configuration;
    using hfa.Brokers.Messages;

    /// <summary>
    /// save message into firebase database
    /// </summary>
    public class RabbitNotificationConsumer : IConsumer<DiffPlaylistEvent>
    {
        private readonly ILogger _logger;
        private readonly FirebaseConfiguration _firebaseOptions;

        public RabbitNotificationConsumer(INotificationService notificationService, ILogger<RabbitSynchronizeConsumer> logger,
            IOptions<FirebaseConfiguration> firebaseOptions)
        {
            _logger = logger;
            _firebaseOptions = firebaseOptions.Value;
        }

        public Task Consume(ConsumeContext<DiffPlaylistEvent> context)
        {
            try
            {
                _logger.LogInformation($"{nameof(RabbitNotificationConsumer)}: New message {context.CorrelationId} poped");
                var message = context.Message.ToString();
                if (context.Message.Changed)
                {
                    SaveAsync(context.Message).Wait();
                }
                _logger.LogInformation(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
            return context.CompleteTask;
        }

        private async Task SaveAsync(DiffPlaylistEvent message, CancellationToken cancellationToken = default)
        {
            var firebase = new FirebaseClient(_firebaseOptions.DatabaseURL, new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(_firebaseOptions.Secret)
            });

            var notif = await firebase
              .Child(FirebaseNotifications.TableName)
              .PostAsync(new FirebaseNotifications.FirebaseNotification
              {
                  Date = DateTime.UtcNow.ToShortDateString(),
                  Level = FirebaseNotifications.FirebaseNotification.LevelEnum.Info,
                  Source = "Synker Batch",
                  Body = $"The Playlist {message.Id} changment detected. {message.NewMediasCount} new medias wad founded and {message.RemovedMediasCount} was removed.",
                  Title = $"The Playlist {message.Id} changment detected"
              }, true);

            _logger.LogInformation($"{nameof(RabbitNotificationConsumer)}: Key for the new notification: {notif.Key}");
        }
    }
}
