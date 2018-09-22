namespace hfa.Synker.batch.HostedServices
{
    using global::Synker.Scheduled.HostedServices;
    using hfa.Brokers.Messages.Contracts;
    using hfa.Synker.Service.Entities.Playlists;
    using hfa.Synker.Service.Services.Playlists;
    using hfa.Synker.Services.Dal;
    using MassTransit;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class PlaylistHealthHostedService : IScheduledTask
    {
        private readonly ILogger<PlaylistHealthHostedService> _logger;
        private readonly IPlaylistService _playlistService;
        private readonly SynkerDbContext _dbContext;
        private readonly IBus _bus;

        public string Schedule => Environment.GetEnvironmentVariable($"{nameof(PlaylistHealthHostedService)}_ScheduledTask") ?? "*/15 * * * *";

        public PlaylistHealthHostedService(ILoggerFactory loggerFactory, IPlaylistService playlistService, SynkerDbContext synkerDbContext,
            IBus requestClient)
        {
            _logger = loggerFactory.CreateLogger<PlaylistHealthHostedService>();
            _playlistService = playlistService;
            _dbContext = synkerDbContext;
            _bus = requestClient;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                IQueryable<Playlist> playlists = _dbContext.Playlist;
                playlists.AsParallel().WithCancellation(cancellationToken).ForAll(async pl =>
                {
                    try
                    {
                        var response = await _playlistService.HealthAsync(pl, cancellationToken);
                        await _bus.Publish(new PlaylistHealthEvent
                        {
                            Id = pl.Id,
                            PlaylistName = pl.Freindlyname,
                            IsOnline = response.IsOnline,
                            MediaCount = pl.TvgMedias.Count
                        }, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                        await _bus.Publish(new TraceEvent
                        {
                            Message = $"Service: {nameof(PlaylistHealthHostedService)}: playlistId : {pl.Id}, Exception :{ex.Message}",
                            UserId = pl.UserId,
                            Level = TraceEvent.LevelTrace.Error
                        }, cancellationToken);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await _bus.Publish(new TraceEvent
                {
                    Message = $"Service: {nameof(PlaylistHealthHostedService)}: Exception :{ex.Message}",
                    Level = TraceEvent.LevelTrace.Error,
                }, cancellationToken);
            }
        }
    }
}
