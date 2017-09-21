using hfa.SyncLibrary.Common;
using Hfa.PlaylistBaseLibrary.Entities;
using Hfa.SyncLibrary.Infrastructure;
using Hfa.SyncLibrary.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlaylistBaseLibrary.ChannelHandlers;
using SyncLibrary.TvgMediaHandlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hfa.SyncLibrary
{
    public static class Init
    {
        internal static IConfiguration Configuration;
        internal static ServiceProvider ServiceProvider;


        static bool IsDev(string env) => env.Equals("Development");

        static Init()
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (String.IsNullOrWhiteSpace(environment))
                throw new ArgumentNullException("Environment not found in ASPNETCORE_ENVIRONMENT");

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
            {
                loggerFactory.AddDebug();
                loggerFactory.AddFile(Configuration.GetSection("Logging"));

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
                .Configure<ApplicationConfigData>(Configuration)
                .AddSingleton<IMessagesService, MessagesService>()
                .AddSingleton<IContextTvgMediaHandler>(sp =>
                {
                    var responseMediaConfig = ElasticConnectionClient.Client.SearchAsync<MediaConfiguration>(x => x.From(0).Size(1)).GetAwaiter().GetResult();
                    var contextTvg = new ContextTvgMediaHandler();
                    if (responseMediaConfig.Documents.Any())
                    {
                        contextTvg.MediaConfiguration = responseMediaConfig.Documents.FirstOrDefault();
                    }
                    return contextTvg;
                })
                .BuildServiceProvider();

        }

        internal static void Build()
        {

        }
    }
}
