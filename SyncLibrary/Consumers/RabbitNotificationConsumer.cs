using hfa.Brokers.Messages.Contracts;
using hfa.Brokers.Messages.Models;
using hfa.Notification.Brokers.Emailing;
using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Notification.Brokers.Consumers
{
    public class RabbitNotificationConsumer : IConsumer<DiffPlaylistEvent>, IConsumer<TraceEvent>
    {
        private readonly ILogger _logger;
        private IModel _mailChannel;
        private readonly INotificationService _notificationService;

        public RabbitNotificationConsumer(INotificationService notificationService, ILogger<NotificationConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public Task Consume(ConsumeContext<DiffPlaylistEvent> context)
        {
            try
            {
                _logger.LogInformation($"New Mail poped from the queue {context.CorrelationId}");
                var message = context.Message.ToString();
                _logger.LogInformation(message);
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

        public Task Consume(ConsumeContext<TraceEvent> context)
        {
            try
            {
                _logger.LogInformation($"New Mail poped from the queue {context.CorrelationId}");
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
