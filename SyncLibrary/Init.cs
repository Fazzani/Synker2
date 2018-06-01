using PlaylistBaseLibrary.ChannelHandlers;

namespace Hfa.SyncLibrary
{
    using RazorLight;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using hfa.Brokers.Messages.Configuration;
    using hfa.Notification.Brokers.Consumers;
    using hfa.Notification.Brokers.Emailing;
    using hfa.Synker.batch.Infrastructure;
    using hfa.Synker.Service.Elastic;
    using hfa.Synker.Service.Services.Elastic;
    using hfa.Synker.Service.Services.Playlists;
    using hfa.Synker.Service.Services.TvgMediaHandlers;
    using hfa.Synker.Services.Dal;
    using hfa.Synker.Services.Messages;
    using Hfa.SyncLibrary.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using hfa.Synker.Service.Services;
    using hfa.Synker.batch.Consumers;
    using hfa.Synker.batch.Producers;
    using Serilog;

    public class Init
    {
        private const string DEV = "Development";
        internal static IConfiguration Configuration;
        internal static ServiceProvider ServiceProvider;
        internal const string Enviroment = DEV;

        public static RazorLightEngine Engine { get; }

        public static bool IsDev => (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? DEV).Equals(DEV);

        static Init()
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? DEV;

            Engine = new RazorLightEngineBuilder()
                   .UseFilesystemProject(Path.Combine(AppContext.BaseDirectory, "EmailTemplates"))
                   .UseMemoryCachingProvider()
                   .Build();

            //if (String.IsNullOrWhiteSpace(environment))
            //    throw new ArgumentNullException("Environment not found in ASPNETCORE_ENVIRONMENT");

            Console.WriteLine("Environment: {0}", environment);
            // Set up configuration sources.
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: true);

            if (IsDev)
            {
                builder.AddUserSecrets<Init>()
                    .AddJsonFile(
                        Path.Combine(AppContext.BaseDirectory, string.Format("..{0}..{0}..{0}", Path.DirectorySeparatorChar), $"appsettings.{environment}.json"),
                        optional: true
                    );
            }
            else
            {
                builder.AddJsonFile($"appsettings.{environment}.json", optional: true);
            }

            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
              .ReadFrom.Configuration(Configuration)
              .Enrich.FromLogContext()
              .WriteTo.Console()
              .CreateLogger();

            var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger);

            //Register Services IOC
            ServiceProvider = new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .AddSingleton(loggerFactory)
                .AddSingleton(Configuration)
                //.Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(TimedLogger<>)))
                .Configure<TvhOptions>(Configuration.GetSection(nameof(TvhOptions)))
                .Configure<ApiOptions>(Configuration.GetSection(nameof(ApiOptions)))
                .Configure<ElasticConfig>(Configuration.GetSection(nameof(ElasticConfig)))
                .Configure<RabbitMQConfiguration>(Configuration.GetSection(nameof(RabbitMQConfiguration)))
                .Configure<MailOptions>(Configuration.GetSection(nameof(MailOptions)))
                .AddDbContext<SynkerDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("PlDatabase")))
                .AddSingleton<IElasticConnectionClient, ElasticConnectionClient>()
                .AddSingleton<IMessageService>(s => new MessageService(Configuration.GetValue<string>($"{nameof(ApiOptions)}:Url"), loggerFactory))
                .AddSingleton<IPlaylistService, PlaylistService>()
                .AddSingleton<IContextTvgMediaHandler, ContextTvgMediaHandler>()
                .AddSingleton<IMessageQueueService, MessageQueueService>()
                .AddSingleton<INotificationConsumer, NotificationConsumer>()
                .AddSingleton<IWebGrabDockerConsumer, WebGrabDockerConsumer>()
                .AddSingleton<IWebGrabDockerProducer, WebGrabDockerProducer>()
                .AddSingleton<INotificationService, NotificationService>()
                .AddSingleton<IWebGrabConfigService, WebGrabConfigService>()
                .AddSingleton<ICommandService, CommandService>()
                .BuildServiceProvider();
        }

        internal static void Build()
        {

        }
    }
}
