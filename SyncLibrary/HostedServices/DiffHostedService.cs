
namespace hfa.Synker.batch.HostedServices
{
    using global::Synker.Scheduled.HostedServices;
    using hfa.PlaylistBaseLibrary.Providers;
    using hfa.Synker.Service.Entities.Playlists;
    using hfa.Synker.Service.Services.Playlists;
    using hfa.Synker.Services.Dal;
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

        public string Schedule => Environment.GetEnvironmentVariable($"{nameof(DiffHostedService)}_ScheduledTask") ?? "* */6 * * *";


        public DiffHostedService(ILoggerFactory loggerFactory, IPlaylistService playlistService, SynkerDbContext synkerDbContext)
        {
            _logger = loggerFactory.CreateLogger<DiffHostedService>();
            _playlistService = playlistService;
            _dbContext = synkerDbContext;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var playlists = _dbContext.Playlist
                .Include(x => x.User)
                //.ThenInclude(u => u.Devices)
                .Where(x => x.Status == PlaylistStatus.Enabled);

            foreach (var pl in playlists)
            {
                if (pl.IsXtreamTag)
                {
                    var res = await _playlistService.DiffWithSourceAsync(() => pl, new XtreamProvider(pl.SynkConfig.Url), false, cancellationToken);
                    if (res.removed.Any() || res.tvgMedia.Any())
                    {
                        //TODO :  send notif to user with result
                        _logger.LogInformation($"Diff detected for the playlist {pl.Id} of user {pl.UserId}");
                        //TODO: Send message to Rabbit

                    }
                }
            }
        }
    }
}
