﻿namespace hfa.Notification.Brokers.Consumers
{
    using hfa.Brokers.Messages.Contracts;
    using hfa.Synker.Service.Services.Playlists;
    using MassTransit;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Synchronize playlist from source url
    /// Add newest medias
    /// And delete removed medias
    /// </summary>
    public class RabbitSynchronizeConsumer : IConsumer<DiffPlaylistEvent>, IConsumer<TraceEvent>
    {
        private readonly ILogger _logger;
        private readonly IBus _bus;
        private readonly IPlaylistService _playlistService;

        public RabbitSynchronizeConsumer(IPlaylistService playlistService, ILogger<NotificationConsumer> logger, IBus bus)
        {
            _playlistService = playlistService;
            _logger = logger;
            _bus = bus;
        }

        public Task Consume(ConsumeContext<DiffPlaylistEvent> context)
        {
            try
            {
                _logger.LogInformation($"{nameof(RabbitSynchronizeConsumer)}: new mail poped from the queue {context.CorrelationId}. Received message: {context.Message.ToString()}");
                var playlist = _playlistService.SynkPlaylistAsync(context.Message.Id).GetAwaiter().GetResult();
                var eventMessage = $"The playlist {playlist.Id}:{playlist.Freindlyname} was synchronized. Total tvgmadias count: {playlist.TvgMedias.Count}";
                _logger.LogInformation(eventMessage);
                _bus.Publish(new TraceEvent { Message = eventMessage }, CancellationToken.None).Wait();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
            return context.CompleteTask;
        }

        public Task Consume(ConsumeContext<TraceEvent> context)
        {
            try
            {
                _logger.LogInformation($"{nameof(RabbitSynchronizeConsumer)}: new mail poped from the queue {context.CorrelationId}");
                _logger.LogInformation(context.Message.Message);
                //var mail = JsonConvert.DeserializeObject<EmailNotification>(message);
                //_notificationService.SendMailAsync(mail, CancellationToken.None).GetAwaiter().GetResult();
                //_logger.LogInformation($"Mail from {mail.From} to {mail.To}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
            return context.CompleteTask;
        }
    }
}
