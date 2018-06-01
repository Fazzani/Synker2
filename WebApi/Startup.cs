using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using hfa.WebApi.Common;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using hfa.WebApi.Common.Auth;
using hfa.WebApi.Common.Filters;
using System.Net;
using Newtonsoft.Json;
using AspNet.Core.Webhooks.Receivers;
using hfa.WebApi.Services;
using hfa.WebApi.Common.Middlewares;
using hfa.Synker.Services.Xmltv;
using hfa.Synker.Services.Dal;
using hfa.Synker.Service.Services.Playlists;
using Microsoft.AspNetCore.ResponseCompression;
using hfa.PlaylistBaseLibrary.Providers;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Services.TvgMediaHandlers;
using PlaylistBaseLibrary.ChannelHandlers;
using hfa.Synker.Service.Services.Picons;
using hfa.Synker.Service.Services.MediaRefs;
using Microsoft.AspNetCore.Mvc;
using hfa.Synker.Service.Services;
using hfa.Synker.Service.Services.Scraper;
using ZNetCS.AspNetCore.Authentication.Basic;
using ZNetCS.AspNetCore.Authentication.Basic.Events;
using Microsoft.AspNetCore.Authentication;
using hfa.Synker.Service.Services.Xtream;
using hfa.Synker.Service.Entities.Auth;
using System.Security.Claims;
using System.Reflection;
using hfa.PlaylistBaseLibrary.Options;
using hfa.Brokers.Messages.Configuration;
using System.IO;
using Microsoft.Extensions.FileProviders;
using hfa.WebApi.Common.Swagger;
using Newtonsoft.Json.Converters;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace hfa.WebApi
{
    /// <summary>
    /// Web api configuration entry
    /// </summary>
    public class Startup
    {
        internal static IConfiguration Configuration;
        readonly IHostingEnvironment CurrentEnvironment;

        /// <summary>
        /// Assembly version
        /// </summary>
        public static string AssemblyVersion = typeof(Startup).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="env"></param>
        public Startup(IConfiguration configuration, IHostingEnvironment env)
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
            var connectionstring = Configuration.GetConnectionString("PlDatabase");
            var isTestEnv = string.IsNullOrEmpty(connectionstring) || CurrentEnvironment.IsEnvironment("Testing");

            services.
               AddSingleton<IElasticConnectionClient, ElasticConnectionClient>()
               .AddSingleton<IPasteBinService, PasteBinService>()
               .AddSingleton<IAuthentificationService, AuthentificationService>()
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
               .Configure<GlobalOptions>(Configuration.GetSection(nameof(GlobalOptions)))
               .Configure<PastBinOptions>(Configuration.GetSection(nameof(PastBinOptions)));

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
                var serviceProvider = services.AddDbContext<SynkerDbContext>(options =>
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
                }).BuildServiceProvider();

                //var DB = serviceProvider.GetService<SynkerDbContext>();
                //DB.Database.EnsureCreated();
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
            var loggerFactory = new LoggerFactory();

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
            );

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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, SynkerDbContext synkerDbContext)
        {
            app.UseResponseCompression();

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            app.Map("/liveness", lapp => lapp.Run(async ctx => ctx.Response.StatusCode = 200));
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
            var log = loggerFactory.CreateLogger<Startup>();

            try
            {
                app.UseCors("CorsPolicy");

                if (Configuration.GetValue<bool>("UseLoadTest"))
                {
                    app.UseMiddleware<ByPassAuthMiddleware>();
                }

                //app.UseBasicAuthentication();

                #region WebSockets

                app.UseWebSockets(new WebSocketOptions
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(120),
                    ReceiveBufferSize = 4 * 1024
                });

                app.UseMiddleware<MessageWebSocketMiddleware>();
                #endregion

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
        /// Configure Security
        /// </summary>
        /// <param name="services"></param>
        private void ConfigSecurity(IServiceCollection services)
        {
            var sp = services.BuildServiceProvider();
            // Resolve the services from the service provider
            var authService = sp.GetService<IAuthentificationService>();

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
            }).AddBasicAuthentication(
            options =>
            {
                options.Realm = "Synker";
                options.Events = new BasicAuthenticationEvents
                {
                    OnValidatePrincipal = context =>
                    {
                        var basic = new BasicAuthenticationMiddleware();
                        var principal = basic.Invoke(context.HttpContext, authService, sp.GetService<SynkerDbContext>(), sp.GetService<ILoggerFactory>());
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
        Action<HttpContext, object> genericHookAction = async (context, message) =>
        {
            CancellationToken ct = context.RequestAborted;
            var logger = context.RequestServices?.GetService<ILogger>();
            try
            {
                var messageJson = string.Empty;
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

                //Send to WebSocket
                using (var socket = new ClientWebSocket())
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    await context.Response.WriteAsync(messageJson, ct);

                    await socket.ConnectAsync(new Uri($"ws://{context.Request.Host}/ws"), ct);
                    await MessageWebSocketMiddleware.SendStringAsync(socket, messageJson);
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
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
