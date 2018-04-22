namespace hfa.Synker.batch.Consumers
{
    using RabbitMQ.Client;
    using System.Threading;
    public interface IConsumer
    {
        void Start(IConnection connection, ManualResetEvent shutdown);
    }
}
