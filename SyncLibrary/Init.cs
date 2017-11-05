using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Services.TvgMediaHandlers;
using hfa.Synker.Services.Messages;
using Hfa.SyncLibrary.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlaylistBaseLibrary.ChannelHandlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hfa.SyncLibrary
{
    public static class Init
    {
        private const string DEV = "Development";
        internal static IConfiguration Configuration;
        internal static ServiceProvider ServiceProvider;


        static bool IsDev(string env) => env.Equals(DEV);

        static Init()
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? DEV;

            //if (String.IsNullOrWhiteSpace(environment))
            //    throw new ArgumentNullException("Environment not found in ASPNETCORE_ENVIRONMENT");

            Console.WriteLine("Environment: {0}", environment);
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: true);

            if (IsDev(environment))
            {

                builder
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
                loggerFactory.AddDebug();
            else
                loggerFactory.AddFile(Configuration.GetSection("Logging"));

            //Register Services IOC
            ServiceProvider = new ServiceCollection()
                .AddSingleton(loggerFactory)
                .AddLogging()
                .AddOptions()
                .Configure<ApplicationConfigData>(Configuration)
                .Configure<ElasticConfig>(Configuration.GetSection(nameof(ElasticConfig)))
                .AddSingleton<IMessagesService, IMessagesService>()
                .AddSingleton<IElasticConnectionClient, ElasticConnectionClient>()
                .AddSingleton<IContextTvgMediaHandler, ContextTvgMediaHandler>()
                .BuildServiceProvider();
        }

        internal static void Build()
        {

        }
    }
}
