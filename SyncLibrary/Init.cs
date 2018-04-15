using hfa.Brokers.Messages.Configuration;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Services.Notification;
using hfa.Synker.Service.Services.Playlists;
using hfa.Synker.Service.Services.TvgMediaHandlers;
using hfa.Synker.Services.Dal;
using hfa.Synker.Services.Messages;
using Hfa.SyncLibrary.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlaylistBaseLibrary.ChannelHandlers;
using RazorLight;
using SyncLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hfa.SyncLibrary
{
    public class Init
    {
        private const string DEV = "Development";
        internal static IConfiguration Configuration;
        internal static ServiceProvider ServiceProvider;

        public static RazorLightEngine Engine { get; }

        static bool IsDev(string env) => env.Equals(DEV);

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

            if (IsDev(environment))
            {
                builder.AddUserSecrets<Init>()
                    .AddJsonFile(
                        Path.Combine(AppContext.BaseDirectory, string.Format("..{0}..{0}..{0}", Path.DirectorySeparatorChar), $"appsettings.{environment}.json"),
                        optional: true
                    );
            }
            else
            {
                builder.AddJsonFile($"appsettings.{environment}.json", optional: false);
            }

            Configuration = builder.Build();

            //Config Logger
            var loggerFactory = new LoggerFactory().AddConsole();
            if (IsDev(environment))
            {
                loggerFactory.AddDebug();
            }
            else
            {
                loggerFactory.AddFile(Configuration.GetSection("Logging"));
            }

            //Register Services IOC
            ServiceProvider = new ServiceCollection()
                .AddSingleton(loggerFactory)
                .AddLogging()
                .AddOptions()
                .Configure<TvhOptions>(Configuration.GetSection(nameof(TvhOptions)))
                .Configure<ApiOptions>(Configuration.GetSection(nameof(ApiOptions)))
                .Configure<ElasticConfig>(Configuration.GetSection(nameof(ElasticConfig)))
                .Configure<RabbitMQConfiguration>(Configuration.GetSection(nameof(RabbitMQConfiguration)))
                .AddSingleton<IMessageService>(s => new MessageService(Configuration.GetValue<string>($"{nameof(ApiOptions)}:Url"), loggerFactory))
                .AddSingleton<INotificationService, NotificationService>()
                .AddSingleton<IPlaylistService, PlaylistService>()
                .AddSingleton<IElasticConnectionClient, ElasticConnectionClient>()
                .AddSingleton<IContextTvgMediaHandler, ContextTvgMediaHandler>()
                .AddDbContext<SynkerDbContext>(options => options.UseMySql(Configuration.GetConnectionString("PlDatabase")))
                .BuildServiceProvider();
        }

        internal static void Build()
        {

        }
    }
}
