﻿namespace hfa.Notification.Brokers.Consumers
{
    using hfa.Brokers.Messages.Models;
    using hfa.Notification.Brokers.Emailing;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using System;
    using System.Text;
    using System.Threading;

    public class NotificationConsumer : INotificationConsumer, IDisposable
    {
        private readonly ILogger _logger;
        private IModel _mailChannel;
        private readonly INotificationService _notificationService;
        private readonly string MailQueueName = "synker.mail.queue";
        private readonly EventHandler<CallbackExceptionEventArgs> eventChannel_CallbackException;

        public NotificationConsumer(INotificationService notificationService, ILogger<NotificationConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
            eventChannel_CallbackException = new EventHandler<CallbackExceptionEventArgs>(Channel_CallbackException);
        }

        private void Channel_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _logger.LogError(e.Exception, e.Exception.Message);
        }

        private void ReceivedMessage(object model, BasicDeliverEventArgs ea)
        {
            try
            {
                _logger.LogInformation($"{nameof(NotificationConsumer)}: New Mail poped from the queue {MailQueueName}");
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var mail = JsonConvert.DeserializeObject<EmailNotification>(message);
                _notificationService.SendMailAsync(mail, CancellationToken.None).GetAwaiter().GetResult();
                //ack message
                _mailChannel.BasicAck(ea.DeliveryTag, true);

                _logger.LogInformation($"Mail from {mail.From} to {mail.To} with template: {mail.IsWithTemplate}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                _mailChannel.BasicReject(ea.DeliveryTag, true);
            }
        }

        public void Start(IConnection connection, ManualResetEvent shutdown)
        {
            _logger.LogInformation($"Start Notification consumer");
            _mailChannel = connection.CreateModel();

            _mailChannel.QueueDeclare(queue: MailQueueName,
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);

            _mailChannel.CallbackException += eventChannel_CallbackException;
            var mailConsumer = new EventingBasicConsumer(_mailChannel);

            mailConsumer.Received += ReceivedMessage;

            _mailChannel.BasicConsume(queue: MailQueueName,
                                        autoAck: false,
                                        consumer: mailConsumer);

            while (!shutdown.WaitOne())
            {
                Thread.Sleep(1000);
            }
        }

        public void Dispose()
        {
            _mailChannel?.Close();
            _mailChannel?.Dispose();
        }
    }
}
