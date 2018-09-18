namespace hfa.Synker.batch.HostedServices
{
    using global::Synker.Scheduled.HostedServices;
    using hfa.Brokers.Messages.Contracts;
    using hfa.PlaylistBaseLibrary.Providers;
    using hfa.Synker.Service.Entities.Playlists;
    using hfa.Synker.Service.Services.Playlists;
    using hfa.Synker.Services.Dal;
    using MassTransit;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class DiffHostedService : IScheduledTask
    {
        private readonly ILogger<DiffHostedService> _logger;
        private readonly IPlaylistService _playlistService;
        private readonly SynkerDbContext _dbContext;
        private readonly IBus _bus;

        public string Schedule => Environment.GetEnvironmentVariable($"{nameof(DiffHostedService)}_ScheduledTask") ?? "0 12 * * *";
        //public string EndPoint => $"http://rabbitmq.synker.ovh/diff_playlist_{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}_queue";
        //public string TraceEndPoint => $"http://rabbitmq.synker.ovh/trace_{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}_queue";

        public DiffHostedService(ILoggerFactory loggerFactory, IPlaylistService playlistService, SynkerDbContext synkerDbContext,
            IBus requestClient)
        {
            _logger = loggerFactory.CreateLogger<DiffHostedService>();
            _playlistService = playlistService;
            _dbContext = synkerDbContext;
            _bus = requestClient;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            IQueryable<Playlist> playlists = _dbContext.Playlist
                .Include(x => x.User)
                //.ThenInclude(u => u.Devices)
                .Where(x => x.Status == PlaylistStatus.Enabled);

            foreach (Playlist pl in playlists)
            {
                try
                {
                    if (pl.IsXtreamTag)
                    {
                        (System.Collections.Generic.IEnumerable<PlaylistManager.Entities.TvgMedia> tvgMedia, System.Collections.Generic.IEnumerable<PlaylistManager.Entities.TvgMedia> removed) = await _playlistService.DiffWithSourceAsync(() => pl, new XtreamProvider(pl.SynkConfig.Url), false, cancellationToken);

                        if (removed.Any() || tvgMedia.Any())
                        {
                            _logger.LogInformation($"Diff detected for the playlist {pl.Id} of user {pl.UserId}");

                            //publish message to Rabbit
                            await _bus.Publish(new DiffPlaylistEvent
                            {
                                Id = pl.Id,
                                PlaylistName = pl.Freindlyname,
                                UserId = pl.UserId,
                                RemovedMediasCount = removed.Count(),
                                RemovedMedias = removed.Take(10),
                                NewMediasCount = tvgMedia.Count(),
                                NewMedias = tvgMedia.Take(10)
                            }, cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _bus.Publish(new TraceEvent
                    {
                        Message = $"Service: {nameof(DiffHostedService)}: playlistId : {pl.Id}, Exception :{ex.Message}",
                        UserId = pl.UserId,
                        Level = TraceEvent.LevelTrace.Error
                    }, cancellationToken);
                }
            }
        }
    }
}
