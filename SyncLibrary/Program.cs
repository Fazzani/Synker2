namespace hfa.Synker.batch
{
    using global::Synker.Scheduled.HostedServices;
    using GreenPipes;
    using hfa.Brokers.Messages.Configuration;
    using hfa.Brokers.Messages.Contracts;
    using hfa.Notification.Brokers.Consumers;
    using hfa.Notification.Brokers.Emailing;
    using hfa.PlaylistBaseLibrary.ChannelHandlers;
    using hfa.Synker.batch.HostedServices;
    using hfa.Synker.Service;
    using hfa.Synker.Service.Services.Elastic;
    using hfa.Synker.Service.Services.Playlists;
    using hfa.Synker.Service.Services.TvgMediaHandlers;
    using hfa.Synker.Services.Dal;
    using MassTransit;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using RabbitMQ.Client;
    using Serilog;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHostBuilder builder = new HostBuilder()
                .ConfigureServices(async (hostContext, services) =>
                {
                    Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(hostContext.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger();
                    ILoggerFactory loggerFactory = new LoggerFactory().AddSerilog(Log.Logger);

                    services.AddOptions();
                    services
                    .Configure<RabbitMQConfiguration>(hostContext.Configuration.GetSection(nameof(RabbitMQConfiguration)))
                    .AddScoped<IPlaylistService, PlaylistService>()
                    .AddSingleton<IScheduledTask, DiffHostedService>()
                    .AddSingleton<IContextTvgMediaHandler, ContextTvgMediaHandler>()
                    .AddSingleton<IElasticConnectionClient, ElasticConnectionClient>()
                    .AddSingleton<INotificationService, NotificationService>()
                    .AddSingleton<RabbitNotificationConsumer, RabbitNotificationConsumer>()
                    .AddSingleton(loggerFactory)
                    .AddLogging(loggingBuilder =>
                         loggingBuilder.AddSerilog(dispose: true))
                    .AddScheduler((sender, a) =>
                     {
                         Console.Write(a.Exception.Message);
                         a.SetObserved();
                     });

                    services.AddDbContext<SynkerDbContext>(options =>
                    {
                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("PlDatabase"),
                        sqlOptions =>
                        {
                            //Configuring Connection Resiliency:
                            sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null);
                        });
                    });

                    await ConfigureRabbitMQAsync(services);
                })
                .ConfigureAppConfiguration((host, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: true);
                    config.AddJsonFile($"appsettings.{host.HostingEnvironment.EnvironmentName}.json", optional: true);
                    config.AddEnvironmentVariables();
                    config.AddUserSecrets<Program>();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureHostConfiguration(config =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });

            await builder.RunConsoleAsync();
        }

        private static async Task ConfigureRabbitMQAsync(IServiceCollection services, CancellationToken cancellationToken = default)
        {
            IBusControl bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                ServiceProvider sp = services.BuildServiceProvider();
                IOptions<RabbitMQConfiguration> rabbitConfig = sp.GetService<IOptions<RabbitMQConfiguration>>();
                ILoggerFactory _loggerFactory = sp.GetService<ILoggerFactory>();
                Microsoft.Extensions.Logging.ILogger _logger = _loggerFactory.CreateLogger(typeof(Program));

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

                    ep.Handler<DiffPlaylistEvent>(context =>
                    {
                        return Console.Out.WriteLineAsync($"Received: {context.Message}");
                    });

                    ep.Consumer(() => sp.GetService<RabbitNotificationConsumer>());
                });
            });

            services.AddSingleton<IPublishEndpoint>(bus);
            services.AddSingleton<ISendEndpointProvider>(bus);
            services.AddSingleton<IBusControl>(bus);
            services.AddSingleton<IBus>(bus);

            services.AddScoped<IHostedService, MassTransitHostedService>();
            await Task.CompletedTask;
        }
    }
}
