namespace hfa.WebApi
{
    using GreenPipes;
    using hfa.Brokers.Messages.Configuration;
    using hfa.Synker.Service;
    using MassTransit;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using RabbitMQ.Client;
    using System;
    public static class StartUpRabbitMq
    {
        public static void AddRabbitMq(this IServiceCollection services)
        {
            //services.AddScoped<DiffPlaylistConsumer>();
            //services.AddScoped<TraceConsumer>();

            services.AddMassTransit(x =>
            {
                // x.AddConsumer<DiffPlaylistConsumer>();
                // x.AddConsumer<TraceConsumer>();
            });

            services.AddSingleton(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                IOptions<RabbitMQConfiguration> rabbitConfig = Startup.Provider.GetService<IOptions<RabbitMQConfiguration>>();
                ILoggerFactory _loggerFactory = Startup.Provider.GetService<ILoggerFactory>();
                Microsoft.Extensions.Logging.ILogger _logger = _loggerFactory.CreateLogger(typeof(Startup));

                _logger.LogInformation($"Connected to rabbit host: {rabbitConfig.Value.Hostname}{rabbitConfig.Value.VirtualHost}:{rabbitConfig.Value.Port}");

                MassTransit.RabbitMqTransport.IRabbitMqHost host = cfg.Host(rabbitConfig.Value.Hostname, rabbitConfig.Value.Port, rabbitConfig.Value.VirtualHost, h =>
                {
                    h.Username(rabbitConfig.Value.Username);
                    h.Password(rabbitConfig.Value.Password);
                });

                cfg.ExchangeType = ExchangeType.Fanout;

                cfg.ReceiveEndpoint(host, ep =>
                {
                    ep.UseCircuitBreaker(cb =>
                    {
                        cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                        cb.TripThreshold = 15;
                        cb.ActiveThreshold = 10;
                        cb.ResetInterval = TimeSpan.FromMinutes(5);
                    });

                    ep.UseRateLimit(1000, TimeSpan.FromSeconds(5));
                    ep.LoadFrom(Startup.Provider);
                });
            }));

            services.AddSingleton<IPublishEndpoint>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<ISendEndpointProvider>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());

            services.AddSingleton<IHostedService, MassTransitHostedService>();
        }

    }
}
