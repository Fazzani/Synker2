using AspNet.Core.Webhooks.Receivers;
using GreenPipes;
using hfa.Brokers.Messages.Configuration;
using hfa.PlaylistBaseLibrary.ChannelHandlers;
using hfa.PlaylistBaseLibrary.Common;
using hfa.PlaylistBaseLibrary.Options;
using hfa.PlaylistBaseLibrary.Providers;
using hfa.Synker.Service;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Entities.Auth;
using hfa.Synker.Service.Services;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Services.MediaRefs;
using hfa.Synker.Service.Services.Picons;
using hfa.Synker.Service.Services.Playlists;
using hfa.Synker.Service.Services.Scraper;
using hfa.Synker.Service.Services.TvgMediaHandlers;
using hfa.Synker.Service.Services.Xtream;
using hfa.Synker.Services.Dal;
using hfa.Synker.Services.Xmltv;
using hfa.WebApi.Common;
using hfa.WebApi.Common.Auth;
using hfa.WebApi.Common.Filters;
using hfa.WebApi.Common.Middlewares;
using hfa.WebApi.Common.Swagger;
using hfa.WebApi.Consumers;
using hfa.WebApi.Http;
using hfa.WebApi.Hubs;
using hfa.WebApi.Services;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Polly;
using RabbitMQ.Client;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ZNetCS.AspNetCore.Authentication.Basic;
using ZNetCS.AspNetCore.Authentication.Basic.Events;

namespace hfa.WebApi
{
    /// <summary>
    /// Web api configuration entry
    /// </summary>
    public class Startup
    {
        internal static IConfiguration Configuration;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment CurrentEnvironment;
        private static IServiceCollection _serviceDescriptors;
        public static IServiceProvider Provider => _serviceDescriptors.BuildServiceProvider();

        /// <summary>
        /// Assembly version
        /// </summary>
        public static string AssemblyVersion = typeof(Startup).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="env"></param>
        public Startup(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            _serviceDescriptors = services;
            string connectionstring = Configuration.GetConnectionString("PlDatabase");
            bool isTestEnv = string.IsNullOrEmpty(connectionstring) || CurrentEnvironment.IsEnvironment("Testing");

            services.
               AddSingleton<IElasticConnectionClient, ElasticConnectionClient>()
               .AddSingleton<IPasteBinService, PasteBinService>()
               .AddScoped<IAuthentificationService, AuthentificationService>()
               .AddSingleton<IProviderFactory, ProviderFactory>()
               .AddSingleton<IContextTvgMediaHandler, ContextTvgMediaHandler>()
               .AddScoped<IXmltvService, XmltvService>()
               .AddScoped<IPiconsService, PiconsService>()
               .AddScoped<IPlaylistService, PlaylistService>()
               .AddScoped<IMediaRefService, MediaRefService>()
               .AddScoped<ISitePackService, SitePackService>()
               .AddScoped<ICommandService, CommandService>()
               .AddScoped<IXtreamService, XtreamService>()
               .AddScoped<IMediaScraper, MediaScraper>()
               .AddScoped<IWebGrabConfigService, WebGrabConfigService>()
               .AddScoped<IMessageQueueService, MessageQueueService>()
               .Configure<RabbitMQConfiguration>(Configuration.GetSection(nameof(RabbitMQConfiguration)))
               .Configure<List<PlaylistProviderOption>>(Configuration.GetSection("PlaylistProviders"))
               .Configure<ElasticConfig>(Configuration.GetSection(nameof(ElasticConfig)))
               .Configure<SecurityOptions>(Configuration.GetSection(nameof(SecurityOptions)))
               .Configure<MediaServerOptions>(Configuration.GetSection(nameof(MediaServerOptions)))
               .Configure<GlobalOptions>(Configuration.GetSection(nameof(GlobalOptions)))
               .Configure<PastBinOptions>(Configuration.GetSection(nameof(PastBinOptions)))
               .Configure<VapidKeysOptions>(Configuration.GetSection(nameof(VapidKeysOptions)));

            services.AddHttpClient<MediaServerService>(c =>
            {
                var ServerMediaOptions = Provider.GetService<IOptions<MediaServerOptions>>();
                var scheme = ServerMediaOptions.Value.IsSecure ? "https" : "http";
                c.BaseAddress = new Uri($"{scheme}://{ServerMediaOptions.Value.Host}:{ServerMediaOptions.Value.Port}/");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                c.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            })
            .AddTransientHttpErrorPolicy(p => p.RetryAsync(3))
            .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

            services.AddSingleton(typeof(HubLifetimeManager<>), typeof(DefaultHubLifetimeManager<>));
            services.AddSingleton(typeof(IHubProtocolResolver), typeof(DefaultHubProtocolResolver));
            services.AddScoped(typeof(IHubActivator<>), typeof(DefaultHubActivator<>));

            services.AddHttpContextAccessor();
            services.AddSignalR();
            ConfigureRabbitMQ(services);

            services.AddMemoryCache();

            if (isTestEnv)
            {
                services.AddDbContext<SynkerDbContext>(options =>
                {
                    options.UseInMemoryDatabase("playlist");
                });
            }
            else
            {
                services.AddDbContext<SynkerDbContext>(options =>
                {
                    options.UseNpgsql(Configuration.GetConnectionString("PlDatabase"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                        //Configuring Connection Resiliency:
                        sqlOptions.
                            EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null);
                    });

                    //// Changing default behavior when client evaluation occurs to throw.
                    //// Default in EFCore would be to log warning when client evaluation is done.
                    //options.ConfigureWarnings(warnings => warnings.Throw(
                    //RelationalEventId.QueryClientEvaluationWarning));
                });

                SynkerDbContext DB = Provider.GetService<SynkerDbContext>();
                DB.Database.EnsureCreated();
            }

            #region Compression
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression(options =>
            {
                options.MimeTypes = new[]
                {
                    // Default
                    "text/plain",
                    "text/css",
                    "application/javascript",
                    "text/html",
                    "application/xml",
                    "text/xml",
                    "application/json",
                    "text/json",
                    // Custom
                    "image/svg+xml"
                };
            });
            #endregion

            ConfigSecurity(services);

            //Logger
            LoggerFactory loggerFactory = new LoggerFactory();

            //cache
            services.AddResponseCaching(options =>
            {
                options.UseCaseSensitivePaths = true;
                options.MaximumBodySize = 1024;
            });

            //MVC
            services.AddMvc(config =>
            {
                config.Filters.Add(typeof(GlobalExceptionFilterAttribute));
                config.CacheProfiles.Add("Default",
                   new CacheProfile()
                   {
                       Duration = 60,
                       Location = ResponseCacheLocation.Any
                   });
                config.CacheProfiles.Add("Long",
                   new CacheProfile()
                   {
                       Duration = 60
                   });
                config.CacheProfiles.Add("Never",
                    new CacheProfile()
                    {
                        Location = ResponseCacheLocation.None,
                        NoStore = true
                    });
            })
            .AddJsonOptions(
                options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.Formatting = Formatting.Indented;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                }
            ).SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            });

            //https://github.com/domaindrivendev/Swashbuckle.AspNetCore
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Synker API",
                    Description = $"Synchronize playlists API {AssemblyVersion}",
                    TermsOfService = "None",
                    Contact = new Contact { Name = "Synker", Email = "contact@synker.ovh", Url = "https://www.github.com/fazzani/synker2" },
                    License = new License { Name = "Use under MIT", Url = "" },

                });

                c.OperationFilter<AuthResponsesOperationFilter>();
                c.DescribeAllEnumsAsStrings();
                c.DescribeStringEnumsInCamelCase();
                c.IgnoreObsoleteActions();
                c.IgnoreObsoleteProperties();
                c.SchemaFilter<AutoRestSchemaFilter>();

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    In = "header",
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = "apiKey"
                });

                c.AddSecurityDefinition("Basic", new BasicAuthScheme
                {
                    Description = "Please insert username/password into fields",
                    Type = "basic"
                });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            #region Webhooks
            // services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.UseGithubWebhook(() => new GithubOptions
            {
                ApiKey = Configuration.GetValue<string>("GitHubHookApiKey"),
                WebHookAction = genericHookAction
            }).UseAppVeyorWebhook(() => new AppveyorOptions
            {
                ApiKey = Configuration.GetValue<string>("AppveyorHookApiKey"),
                WebHookAction = genericHookAction
            });
            #endregion

        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="synkerDbContext"></param>
        /// <param name="applicationLifetime"></param>
        /// <param name="serviceProvider"></param>
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, ILoggerFactory loggerFactory,
            SynkerDbContext synkerDbContext, Microsoft.AspNetCore.Hosting.IApplicationLifetime applicationLifetime, IServiceProvider serviceProvider)
        {
            ILogger<Startup> log = loggerFactory.CreateLogger<Startup>();
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            app.Map("/liveness", lapp => lapp.Run(async ctx => ctx.Response.StatusCode = 200));

            app.UseHttpContext();
            app.UseResponseCompression();

            string appAbout = JsonConvert.SerializeObject(new ApplicationAbout
            {
                Author = "Synker corporation",
                ApplicationName = "Synker",
            });

            app.Map("/about", lapp => lapp.Run(async ctx =>
            {
                log.LogDebug("about");
                ctx.Response.ContentType = "application/json";
                ctx.Response.StatusCode = 200;
                await ctx.Response.WriteAsync(appAbout);
            }));
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

            synkerDbContext.Database.Migrate();

            //Chanllenge Let's Encrypt path
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @".well-known")),
                RequestPath = new PathString("/.well-known"),
                ServeUnknownFileTypes = true // serve extensionless file
            });

            loggerFactory.AddFile(Configuration.GetSection("Logging"));

            if (env.IsDevelopment())
            {
                loggerFactory.AddConsole(Configuration.GetSection("Logging"));
                loggerFactory.AddDebug();
            }

            try
            {
                app.UseCors("CorsPolicy");

                if (Configuration.GetValue<bool>("UseLoadTest"))
                {
                    app.UseMiddleware<ByPassAuthMiddleware>();
                }

                #region Swagger

                app.UseSwagger();
                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Synker API V1");
                    //c.DefaultModelExpandDepth(2);
                    //c.DefaultModelRendering(ModelRendering.Model);
                    //c.DefaultModelsExpandDepth(-1);
                    c.DisplayOperationId();
                    c.DisplayRequestDuration();
                    c.DocExpansion(DocExpansion.None);
                    c.EnableDeepLinking();
                    c.EnableFilter();
                    //c.MaxDisplayedTags(5);
                    c.ShowExtensions();
                    c.EnableValidator();
                    c.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Post, SubmitMethod.Delete, SubmitMethod.Put);
                });
                #endregion

                app.UseWebHooks(typeof(AppveyorReceiver));
                app.UseWebHooks(typeof(GithubReceiver));
                //Cache
                app.UseResponseCaching();

                app.UseStaticFiles();

                app.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=HealthCheck}/{action=Index}");
                });

                app.UseSignalR(routes =>
                {
                    routes.MapHub<NotificationHub>("/hubs/notification");
                });

            }
            catch (Exception ex)
            {
                app.Run(
                  async context =>
                  {
                      log.LogError($"{ex.Message}");

                      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                      context.Response.ContentType = "text/plain";
                      await context.Response.WriteAsync(ex.Message).ConfigureAwait(false);
                      await context.Response.WriteAsync(ex.StackTrace).ConfigureAwait(false);
                  });
            }
        }

        /// <summary>
        /// Configure RabbitMq bus
        /// </summary>
        /// <param name="services"></param>
        private void ConfigureRabbitMQ(IServiceCollection services)
        {
            services.AddScoped<DiffPlaylistConsumer>();
            services.AddScoped<TraceConsumer>();

            services.AddMassTransit(x =>
            {
                x.AddConsumer<DiffPlaylistConsumer>();
                x.AddConsumer<TraceConsumer>();
            });

            services.AddSingleton(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                IOptions<RabbitMQConfiguration> rabbitConfig = Provider.GetService<IOptions<RabbitMQConfiguration>>();
                ILoggerFactory _loggerFactory = Provider.GetService<ILoggerFactory>();
                ILogger _logger = _loggerFactory.CreateLogger(typeof(Startup));

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
                    ep.LoadFrom(Provider);
                });
            }));

            services.AddSingleton<IPublishEndpoint>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<ISendEndpointProvider>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());

            services.AddSingleton<IHostedService, MassTransitHostedService>();
        }

        /// <summary>
        /// Configure Security
        /// </summary>
        /// <param name="services"></param>
        private void ConfigSecurity(IServiceCollection services)
        {
            // Resolve the services from the service provider
            IAuthentificationService authService = Provider.GetService<IAuthentificationService>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwtBearerOptions =>
            {
                jwtBearerOptions.SaveToken = true;
                //jwtBearerOptions.BackchannelHttpHandler
                jwtBearerOptions.RequireHttpsMetadata = true;
                jwtBearerOptions.TokenValidationParameters = authService.Parameters;
                // We have to hook the OnMessageReceived event in order to
                // allow the JWT authentication handler to read the access
                // token from the query string when a WebSocket or 
                // Server-Sent Events request comes in.
                jwtBearerOptions.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/hubs/notification")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            }).AddBasicAuthentication(
            options =>
            {
                options.Realm = "Synker";
                options.Events = new BasicAuthenticationEvents
                {
                    OnValidatePrincipal = context =>
                    {
                        var basic = new BasicAuthenticationMiddleware();
                        var principal = basic.Invoke(context.HttpContext, authService, Provider.GetService<SynkerDbContext>(), Provider.GetService<ILoggerFactory>());
                        if (principal != null)
                        {
                            var ticket = new AuthenticationTicket(
                                principal,
                                new AuthenticationProperties(),
                                BasicAuthenticationDefaults.AuthenticationScheme);

                            context.Principal = principal;
                            return Task.FromResult(AuthenticateResult.Success(ticket));
                            //return Task.CompletedTask;
                        }

                        return Task.FromResult(AuthenticateResult.Fail("Authentication failed."));
                    }
                };
            });

            services.AddAuthorization(authorizationOptions =>
            {
                authorizationOptions.AddPolicy(AuthorizePolicies.ADMIN, policyBuilder => policyBuilder.RequireClaim(ClaimTypes.Role, Role.ADMIN_ROLE_NAME));
                authorizationOptions.AddPolicy(AuthorizePolicies.VIP, policyBuilder => policyBuilder.RequireClaim(AuthorizePolicies.VIP));
                authorizationOptions.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme, "Basic").RequireAuthenticatedUser().Build();
            });
        }

        #region WebHook Action
        private Action<HttpContext, object> genericHookAction = async (context, message) =>
        {
            CancellationToken ct = context.RequestAborted;
            ILogger logger = context.RequestServices?.GetService<ILogger>();
            IHubContext<NotificationHub> notifHub = context.RequestServices?.GetService<IHubContext<NotificationHub>>();
            try
            {
                string messageJson = string.Empty;
                messageJson = JsonConvert.SerializeObject(message);
                //Call webSocket to add new message (Db and trigger that for all observers)

                if (message is AppveyorWebHookMessage appveyorMessage)
                {
                    logger?.LogInformation($"New WebSoket hook Message {nameof(AppveyorWebHookMessage)} : {messageJson}");
                }
                else if (message is GithubWebHookMessage githubMessage)
                {
                    logger?.LogInformation($"New WebSoket hook Message {nameof(GithubWebHookMessage)} : {messageJson}");
                }

                if (notifHub != null)
                {
                    await notifHub.Clients.All.SendAsync(messageJson, ct);
                }
            }
            catch (WebSocketException wsex)
            {
                logger?.LogError(wsex.Message);
            }
            catch (Exception ex)
            {
                if (!context.Response.HasStarted)
                {
                    logger?.LogError(ex.Message);
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(ex.Message), ct);
                }
            }
        };

        #endregion
    }
}
