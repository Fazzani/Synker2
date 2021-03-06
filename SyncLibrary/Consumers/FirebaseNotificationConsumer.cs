﻿namespace hfa.Notification.Brokers.Consumers
{
    using hfa.Brokers.Messages.Contracts;
    using MassTransit;
    using Microsoft.Extensions.Logging;
    using System;
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
    public class FirebaseNotificationConsumer : IConsumer<TraceEvent>, IConsumer<PlaylistHealthEvent>
    {
        private readonly ILogger _logger;
        private readonly FirebaseConfiguration _firebaseOptions;

        public FirebaseNotificationConsumer(ILogger<RabbitSynchronizeConsumer> logger,
            IOptions<FirebaseConfiguration> firebaseOptions)
        {
            _logger = logger;
            _firebaseOptions = firebaseOptions.Value;
        }

        public async Task Consume(ConsumeContext<TraceEvent> context)
        {
            try
            {
                await SaveAsync(context.Message).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        private async Task SaveAsync(TraceEvent message, CancellationToken cancellationToken = default)
        {
            var firebase = new FirebaseClient(_firebaseOptions.DatabaseURL, new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(_firebaseOptions.Secret)
            });

            _logger.LogInformation($"{nameof(FirebaseNotificationConsumer)}: new message for the user {message.UserId} that has Hash => {message.UserIdHash} ");

            var notif = await firebase
              .Child(FirebaseNotifications.TableName)
              .Child($"{message.UserIdHash}")
              .PostAsync(new FirebaseNotifications.FirebaseNotification
              {
                  Date = message.CreatedDate.ToString(),
                  Level = GetLevel(message.Level),
                  Source = nameof(TraceEvent),
                  Body = message.Message,
                  Title = nameof(TraceEvent),
                  UserId = message.UserIdHash,
                  UnixTimestamp = message.UnixTimestamp
              }, true)
              .ConfigureAwait(false);

            _logger.LogInformation($"{nameof(FirebaseNotificationConsumer)}: Key for the new notification: {notif.Key}");
        }

        private string GetLevel(string traceEventlevel)
        {
            if (traceEventlevel == TraceEvent.LevelTrace.Warning)
            {
                return FirebaseNotifications.FirebaseNotification.LevelEnum.Warning;
            }
            else if (traceEventlevel == TraceEvent.LevelTrace.Error)
            {
                return FirebaseNotifications.FirebaseNotification.LevelEnum.Error;
            }
            return FirebaseNotifications.FirebaseNotification.LevelEnum.Info;
        }

        public async Task Consume(ConsumeContext<PlaylistHealthEvent> context)
        {
            try
            {
                var firebase = new FirebaseClient(_firebaseOptions.DatabaseURL, new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(_firebaseOptions.Secret)
                });

                await firebase
                 .Child($"{nameof(PlaylistHealthState).ToLowerInvariant()}/{context.Message.Id}")
                 .PutAsync(context.Message)
                 .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }
}
