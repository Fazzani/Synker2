namespace hfa.Synker.Service
{
    using MassTransit;
    using Microsoft.Extensions.Hosting;
    using System.Threading;
    using System.Threading.Tasks;
    public class MassTransitHostedService : IHostedService
    {
        private readonly IBusControl busControl;

        public MassTransitHostedService(IBusControl busControl)
        {
            this.busControl = busControl;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await busControl.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await busControl.StopAsync(cancellationToken);
        }
    }
}
