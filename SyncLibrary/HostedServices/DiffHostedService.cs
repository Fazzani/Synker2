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
    using PlaylistManager.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class DiffHostedService : IScheduledTask
    {
        private readonly ILogger<DiffHostedService> _logger;
        private readonly IPlaylistService _playlistService;
        private readonly SynkerDbContext _dbContext;
        private readonly IBus _bus;
        private readonly IProviderFactory _providerFactory;

        public string Schedule => Environment.GetEnvironmentVariable($"{nameof(DiffHostedService)}_ScheduledTask") ?? "0 12 * * *";
        //public string EndPoint => $"http://rabbitmq.synker.ovh/diff_playlist_{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}_queue";
        //public string TraceEndPoint => $"http://rabbitmq.synker.ovh/trace_{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}_queue";

        public DiffHostedService(ILogger<DiffHostedService> logger, IPlaylistService playlistService, SynkerDbContext synkerDbContext,
            IBus requestClient, IProviderFactory providerFactory)
        {
            _logger = logger;
            _playlistService = playlistService;
            _dbContext = synkerDbContext;
            _bus = requestClient;
            _providerFactory = providerFactory;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            IQueryable<Playlist> playlists = _dbContext.Playlist
                .Include(x => x.User)
                //.ThenInclude(u => u.Devices)
                .Where(x => x.Status == PlaylistStatus.Enabled);

            playlists.AsParallel().WithCancellation(cancellationToken).ForAll(async pl =>
            {
                try
                {
                    PlaylistProvider<Playlist<TvgMedia>, TvgMedia> provider = null;
                    try
                    {
                        provider = _providerFactory.CreateInstance(pl.SynkConfig.Uri, pl.SynkConfig.Provider);
                    }
                    catch (Exception)
                    {
                        throw new ApplicationException("Can't resolve playlist provider");
                    }

                    (IEnumerable<TvgMedia> tvgMedia, IEnumerable<TvgMedia> removed) =
                            await _playlistService.DiffWithSourceAsync(() => pl, provider, false, cancellationToken);

                    if (removed.Any() || tvgMedia.Any())
                    {
                        _logger.LogInformation($"Diff detected for the playlist {pl.Id} of user {pl.UserId}");

                        //publish message to Rabbit
                        await _bus.Publish(new DiffPlaylistEvent
                        {
                            Id = pl.Id,
                            PlaylistName = pl.Freindlyname,
                            UserId = pl.User.Email,
                            RemovedMediasCount = removed.Count(),
                            RemovedMedias = removed.Take(10),
                            NewMediasCount = tvgMedia.Count(),
                            NewMedias = tvgMedia.Take(10)
                        }, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    await _bus.Publish(new TraceEvent
                    {
                        Message = $"playlistId : {pl.Id}, Exception :{ex.Message}",
                        UserId = pl.User.Email,
                        Level = TraceEvent.LevelTrace.Error,
                        Source = nameof(DiffHostedService)
                    }, cancellationToken);
                }
            }
                );
        }
    }
}
