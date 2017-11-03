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

namespace hfa.WebApi
{
    public class Startup
    {
        internal static IConfiguration Configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.
               AddSingleton<IElasticConnectionClient, ElasticConnectionClient>()
               .AddSingleton<IPasteBinService, PasteBinService>()
               .AddSingleton<IAuthentificationService, AuthentificationService>()
               .AddScoped<IXmltvService, XmltvService>()
               .AddScoped<IPlaylistService, PlaylistService>()
               .Configure<List<PlaylistProviderOption>>(Configuration.GetSection("PlaylistProviders"))
               .Configure<ElasticConfig>(Configuration.GetSection(nameof(ElasticConfig)))
               .Configure<SecurityOptions>(Configuration.GetSection(nameof(SecurityOptions)))
               .Configure<PastBinOptions>(Configuration.GetSection(nameof(PastBinOptions)));

            var serviceProvider = services.AddDbContext<SynkerDbContext>(options => options
             .UseMySql(Configuration.GetConnectionString("PlDatabase"),
                a => a.MigrationsAssembly("hfa.WebApi")))
             .BuildServiceProvider();

            var DB = serviceProvider.GetService<SynkerDbContext>();
            DB.Database.EnsureCreated();

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

            services.AddMvc(config =>
            {
                config.Filters.Add(typeof(GlobalExceptionFilterAttribute));
            })
            .AddJsonOptions(
                options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            );

            //https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?tabs=visual-studio
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Synker API",
                    Description = "Synchronize playlists API",
                    TermsOfService = "None",
                    Contact = new Contact { Name = "Synker", Email = "contact@synker.ovh", Url = "https://www.github.com/fazzani/synker2" },
                    License = new License { Name = "Use under MIT", Url = "" },
                   
                });
                
                c.DescribeAllEnumsAsStrings();
                c.IgnoreObsoleteActions();
                c.IgnoreObsoleteProperties();
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseResponseCompression();

            loggerFactory.AddFile(Configuration.GetSection("Logging"));
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            var log = loggerFactory.CreateLogger<Startup>();

            try
            {
                app.UseCors("CorsPolicy");

                //  app.UseBasicAuthentication();

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
                    c.ShowJsonEditor();
                });
                #endregion

                app.UseWebHooks(typeof(AppveyorReceiver));
                app.UseWebHooks(typeof(GithubReceiver));

                app.UseStaticFiles();

                app.UseMvc();
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
            }).AddBasicAuthentication<BasicAuth>();

            services.AddAuthorization(authorizationOptions =>
            {
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
