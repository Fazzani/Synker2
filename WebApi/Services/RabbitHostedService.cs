namespace hfa.WebApi.Services
{
    using MassTransit;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class RabbitHostedService : IHostedService
    {
        private readonly IBusControl busControl;

        public RabbitHostedService(IBusControl busControl)
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
