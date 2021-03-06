﻿namespace hfa.Synker.batch
{
    using global::Synker.Scheduled.HostedServices;
    using GreenPipes;
    using hfa.Brokers.Messages.Configuration;
    using hfa.Brokers.Messages.Contracts;
    using hfa.Notification.Brokers.Consumers;
    using hfa.Notification.Brokers.Emailing;
    using hfa.PlaylistBaseLibrary.ChannelHandlers;
    using hfa.PlaylistBaseLibrary.Options;
    using hfa.PlaylistBaseLibrary.Providers;
    using hfa.Synker.batch.HostedServices;
    using hfa.Synker.Service;
    using hfa.Synker.Service.Services;
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
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class Program
    {
        public static Action<IConfigurationBuilder> BuildConfiguration =
            builder => builder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("hostsettings.json", optional: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("environment") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();

        public static async Task<int> Main(string[] args)
        {
            var builderConfig = new ConfigurationBuilder();
            BuildConfiguration(builderConfig);

            Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builderConfig.Build())
                    .CreateLogger();

            Log.Information("Start Synker batch");

            try
            {
                IHostBuilder builder = new HostBuilder()
                .ConfigureServices(async (hostContext, services) =>
                {
                    services.AddOptions();
                    services
                    .Configure<EmailSettings>(hostContext.Configuration.GetSection(nameof(EmailSettings)))
                    .Configure<RabbitMQConfiguration>(hostContext.Configuration.GetSection(nameof(RabbitMQConfiguration)))
                    .Configure<FirebaseConfiguration>(hostContext.Configuration.GetSection(nameof(FirebaseConfiguration)))
                    .Configure<List<PlaylistProviderOption>>(hostContext.Configuration.GetSection(PlaylistProviderOption.PlaylistProvidersConfigurationKeyName))
                    .AddSingleton<IProviderFactory, ProviderFactory>()
                    .AddScoped<ISitePackService, SitePackService>()
                    .AddScoped<IPlaylistService, PlaylistService>()
                    .AddScoped<IScheduledTask, DiffHostedService>()
                    .AddScoped<IScheduledTask, PlaylistHealthHostedService>()
                    .AddSingleton<IContextTvgMediaHandler, ContextTvgMediaHandler>()
                    .AddSingleton<IElasticConnectionClient, ElasticConnectionClient>()
                    .AddSingleton<INotificationService, NotificationService>()
                    .AddScoped<RabbitNotificationConsumer, RabbitNotificationConsumer>()
                    .AddScoped<RabbitSynchronizeConsumer, RabbitSynchronizeConsumer>()
                    .AddScoped<FirebaseNotificationConsumer, FirebaseNotificationConsumer>()
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

                    await ConfigureRabbitMQAsync(services).ConfigureAwait(false);
                }).ConfigureHostConfiguration(configHost =>
                {
                    BuildConfiguration(configHost);
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((host, config) =>
                {
                    BuildConfiguration(config);
                    if (host.HostingEnvironment.EnvironmentName.Equals("Development", StringComparison.CurrentCultureIgnoreCase))
                    {
                        config.AddUserSecrets<Program>();
                    }

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                });

                await builder
                    .UseConsoleLifetime()
                    .UseSerilog()
                    .RunConsoleAsync()
                    .ConfigureAwait(false);

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Synker Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static async Task ConfigureRabbitMQAsync(IServiceCollection services, CancellationToken cancellationToken = default)
        {
            IBusControl bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                ServiceProvider sp = services.BuildServiceProvider();
                IOptions<RabbitMQConfiguration> rabbitConfig = sp.GetService<IOptions<RabbitMQConfiguration>>();
                var _logger = sp.GetService<ILogger<Program>>();

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

                    ep.Handler<DiffPlaylistEvent>(async context =>
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        await Console.Out.WriteLineAsync($"{nameof(DiffPlaylistEvent)}: {context.Message}").ConfigureAwait(false);
                        Console.ResetColor();
                    });
                    ep.Handler<TraceEvent>(async context =>
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        await Console.Out.WriteLineAsync($"{nameof(TraceEvent)}: {context.Message}").ConfigureAwait(false);
                        Console.ResetColor();
                    });

                    //Note: do no use sp variable
                    ep.Consumer(() => services.BuildServiceProvider().GetService<RabbitNotificationConsumer>());
                    ep.Consumer(() => services.BuildServiceProvider().GetService<RabbitSynchronizeConsumer>());
                    ep.Consumer(() => services.BuildServiceProvider().GetService<FirebaseNotificationConsumer>());
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
