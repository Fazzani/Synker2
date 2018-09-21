namespace hfa.Notification.Brokers.Consumers
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
    public class RabbitSynchronizeConsumer : IConsumer<DiffPlaylistEvent>
    {
        private readonly ILogger _logger;
        private readonly IBusControl _bus;
        private readonly IPlaylistService _playlistService;

        public RabbitSynchronizeConsumer(IPlaylistService playlistService, ILogger<RabbitSynchronizeConsumer> logger, IBusControl bus)
        {
            _playlistService = playlistService;
            _logger = logger;
            _bus = bus;
        }

        public async Task Consume(ConsumeContext<DiffPlaylistEvent> context)
        {
            try
            {
                _logger.LogInformation($"{nameof(RabbitSynchronizeConsumer)}: new message poped from the queue {context.CorrelationId}. Received message: {context.Message.ToString()}");
                var playlist = await _playlistService.SynkPlaylistAsync(context.Message.Id);
                var eventMessage = $"The playlist {playlist.Id}:{playlist.Freindlyname} was synchronized. Total media count: {playlist.TvgMedias.Count}";

               await _bus.Publish(new TraceEvent
                {
                    Message = eventMessage,
                    CreatedDate = DateTime.UtcNow,
                    Level = TraceEvent.LevelTrace.Info,
                    UserId = playlist.UserId
                }, CancellationToken.None);

                _logger.LogInformation(eventMessage);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }
}
