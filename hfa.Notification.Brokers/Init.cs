using hfa.Brokers.Messages;
using hfa.Brokers.Messages.Configuration;
using hfa.Notification.Brokers.Emailing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        internal static string Enviroment = DEV;

       public static bool IsDev(string env) => env.Equals(DEV);

        static Init()
        {
            Enviroment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? DEV;

            //if (String.IsNullOrWhiteSpace(environment))
            //    throw new ArgumentNullException("Environment not found in ASPNETCORE_ENVIRONMENT");

            Console.WriteLine("Environment: {0}", Enviroment);
            // Set up configuration sources.
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: true);

            if (IsDev(Enviroment))
            {
                builder.AddUserSecrets<Init>()
                    .AddJsonFile(
                        Path.Combine(AppContext.BaseDirectory, string.Format("..{0}..{0}..{0}", Path.DirectorySeparatorChar), $"appsettings.{Enviroment}.json"),
                        optional: true
                    );
            }
            else
            {
                builder.AddJsonFile($"appsettings.{Enviroment}.json", optional: false);
            }

            Configuration = builder.Build();

            //Config Logger
            var loggerFactory = new LoggerFactory().AddConsole();
            if (IsDev(Enviroment))
                loggerFactory.AddDebug();
            else
                loggerFactory.AddFile(Configuration.GetSection("Logging"));

            //Register Services IOC
            ServiceProvider = new ServiceCollection()
                .AddSingleton(loggerFactory)
                .AddLogging()
                .AddOptions()
                .Configure<MailOptions>(Configuration.GetSection(nameof(MailOptions)))
                .Configure<RabbitMQConfiguration>(Configuration.GetSection(nameof(RabbitMQConfiguration)))
                .AddSingleton<INotificationService, NotificationService>()
                .BuildServiceProvider();
        }

        internal static void Build()
        {

        }
    }
}
