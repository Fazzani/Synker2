﻿namespace hfa.Notification.Brokers.Consumers
{
    using hfa.Synker.batch.Consumers;
    using RabbitMQ.Client;
    using System.Threading;

    public interface INotificationConsumer : IConsumer
    {
    }
}