using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using hfa.WebApi.Common;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using System.Threading;
using hfa.WebApi.Dal;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using hfa.WebApi.Common.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using hfa.WebApi.Common.Filters;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using AspNet.Core.Webhooks.Receivers;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
            ConfigSecurity(services);
            //Logger
            var loggerFactory = new LoggerFactory();

            services.AddMvc(config =>
            {
                config.Filters.Add(typeof(GlobalExceptionFilterAttribute));
            });

            //https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?tabs=visual-studio
            services.AddSwaggerGen(c =>
            {
                //c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Synker API",
                    Description = "Synchronize playlists API",
                    TermsOfService = "None",
                    Contact = new Contact { Name = "Fazzani Heni", Email = "fazzani.heni@outlook.fr", Url = "https://www.github.com/fazzani" },
                    License = new License { Name = "Use under MIT", Url = "" }
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

            // services.AddOptions();

            services.
                AddSingleton<IElasticConnectionClient, ElasticConnectionClient>()
                .AddScoped<IAuthentificationService, AuthentificationService>()
                //.AddScoped<IWebHookService, WebHookServiceDefault>()
                .Configure<List<PlaylistProviderOption>>(Configuration.GetSection("PlaylistProviders"))
                .Configure<ApplicationConfigData>(Configuration)
                .Configure<SecurityOptions>(Configuration.GetSection(nameof(SecurityOptions)));

            #region Webhooks
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.UseGithubWebhook(() => new GithubOptions
            {
                ApiKey = "test",
                WebHookAction = async (context, message) =>
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { source = nameof(GithubOptions), message = message }), CancellationToken.None);
                }
            }).UseAppVeyorWebhook(() => new AppveyorOptions
            {
                ApiKey = "qwertyuiopasdfghjklzxcvbnm123456",
                WebHookAction = async (context, message) =>
                                 {
                                     context.Response.ContentType = "application/json";
                                     context.Response.StatusCode = StatusCodes.Status200OK;
                                     await context.Response.WriteAsync(JsonConvert.SerializeObject(new { source = nameof(AppveyorOptions), message = message }), CancellationToken.None);
                                 }
            });
            #endregion

            var serviceProvider = services.AddDbContext<SynkerDbContext>(options => options
            .UseMySql(Configuration.GetConnectionString("PlDatabase")))
             .BuildServiceProvider();

            var DB = serviceProvider.GetService<SynkerDbContext>();
            DB.Database.EnsureCreated();

            //services.AddApiVersioning();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddFile(Configuration.GetSection("Logging"));
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            var log = loggerFactory.CreateLogger<Startup>();

            try
            {
                app.UseCors("CorsPolicy");

                #region WebSockets
                var webSocketOptions = new WebSocketOptions
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(120),
                    ReceiveBufferSize = 4 * 1024
                };

                app.UseWebSockets(webSocketOptions);

                app.Use(async (context, next) =>
                {
                    if (context.Request.Path == "/ws")
                    {
                        if (context.WebSockets.IsWebSocketRequest)
                        {
                            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                            await Echo(context, webSocket);
                        }
                        else
                        {
                            context.Response.StatusCode = 400;
                        }
                    }
                    else
                    {
                        await next();
                    }
                });
                #endregion

                #region Swagger

                app.UseSwagger();
                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Synker API V1");
                });
                #endregion

                app.UseStaticFiles();
                app.UseWebHooks(typeof(AppveyorReceiver));
                app.UseWebHooks(typeof(GithubReceiver));
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
        /// Echo websokets
        /// </summary>
        /// <param name="context"></param>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        /// <summary>
        /// Configure Security
        /// </summary>
        /// <param name="services"></param>
        private void ConfigSecurity(IServiceCollection services)
        {
            var securityOptions = new SecurityOptions();
            var jwtAppSettingOptions = Configuration.GetSection(nameof(SecurityOptions));
            jwtAppSettingOptions.Bind(securityOptions);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityOptions.SymmetricSecurityKey)),
                    ValidIssuer = securityOptions.Issuer,
                    ValidAudience = securityOptions.Audience,
                };
            });

            services.AddAuthorization(authorizationOptions =>
            {
                authorizationOptions.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build();
            });
        }
    }
}
