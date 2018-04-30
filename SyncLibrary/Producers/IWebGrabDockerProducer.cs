using RabbitMQ.Client;
using System.Threading;

namespace hfa.Synker.batch.Producers
{
    public interface IWebGrabDockerProducer
    {
        void Start(IConnection connection, ManualResetEvent shutdown);
    }
}