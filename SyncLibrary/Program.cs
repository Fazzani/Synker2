namespace hfa.Synker.batch
{
    using global::Synker.Scheduled.HostedServices;
    using hfa.Brokers.Messages.Configuration;
    using hfa.PlaylistBaseLibrary.ChannelHandlers;
    using hfa.Synker.batch.HostedServices;
    using hfa.Synker.Service.Services.Elastic;
    using hfa.Synker.Service.Services.Playlists;
    using hfa.Synker.Service.Services.TvgMediaHandlers;
    using hfa.Synker.Services.Dal;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(hostContext.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger();
                    var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger);

                    services.AddOptions();
                    services
                    .Configure<RabbitMQConfiguration>(hostContext.Configuration.GetSection(nameof(RabbitMQConfiguration)))
                    .AddScoped<IPlaylistService, PlaylistService>()
                    .AddSingleton<IScheduledTask, DiffHostedService>()
                    .AddSingleton<IContextTvgMediaHandler, ContextTvgMediaHandler>()
                    .AddSingleton<IElasticConnectionClient, ElasticConnectionClient>()
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
    }
}
