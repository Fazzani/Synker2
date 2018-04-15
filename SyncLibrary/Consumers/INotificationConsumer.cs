namespace hfa.Notification.Brokers.Consumers
{
    using RabbitMQ.Client;
    using System.Threading;

    public interface INotificationConsumer
    {
        void Start(IConnection connection, ManualResetEvent shutdown);
    }
}