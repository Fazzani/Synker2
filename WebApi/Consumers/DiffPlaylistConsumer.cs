using hfa.Brokers.Messages.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Consumers
{
    public class DiffPlaylistConsumer : IConsumer<DiffPlaylistEvent>, IConsumer<TraceEvent>
    {
        private ILogger<DiffPlaylistConsumer> _logger;

        public DiffPlaylistConsumer(ILogger<DiffPlaylistConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<DiffPlaylistEvent> context)
        {
            _logger.LogInformation($"Playlist : {context.Message.Id}, New Medias : {context.Message.NewMedias.ToList().Count}");
            await context.CompleteTask;
        }

        public async Task Consume(ConsumeContext<TraceEvent> context)
        {
            _logger.LogCritical($"{context.Message.Message}");
            await context.CompleteTask;
        }
    }
}
