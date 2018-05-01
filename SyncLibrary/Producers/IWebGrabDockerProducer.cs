using RabbitMQ.Client;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Synker.batch.Producers
{
    public interface IWebGrabDockerProducer
    {
        Task StartAsync(IConnection connection, CancellationToken cancellationToken);
    }
}