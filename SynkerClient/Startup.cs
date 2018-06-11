using hfa.PlaylistBaseLibrary.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace SynkerClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IHostingEnvironment env)
        {
            var log = loggerFactory.CreateLogger<Startup>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                //{
                //    HotModuleReplacement = true
                //});
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            app.Map("/liveness", lapp => lapp.Run(async ctx => ctx.Response.StatusCode = 200));

            var appAbout = JsonConvert.SerializeObject(new ApplicationAbout
            {
                Author = "Synker corporation",
                ApplicationName = "Synker",
            });

            app.Map("/about", lapp => lapp.Run(async ctx =>
            {
                log.LogDebug("Requesting About client application...");
                ctx.Response.ContentType = "application/json";
                ctx.Response.StatusCode = 200;
                await ctx.Response.WriteAsync(appAbout);
            }));
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                  name: "spa-fallback",
                  defaults: new { controller = "Home", action = "Index" });
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501
                spa.Options.SourcePath = "ClientApp";

                //spa.UseSpaPrerendering(options =>
                // {
                //     //TODO: inject to typescript
                //     options.SupplyData = (context, data) =>
                //     {
                //         // Creates a new value called isHttpsRequest that's passed to TypeScript code
                //         data["about"] = JsonConvert.SerializeObject(new ApplicationAbout
                //         {
                //             Author = "Synker corporation",
                //             ApplicationName = "Synker",
                //         });
                //     };

                //     options.BootModulePath = $"{spa.Options.SourcePath}/dist-server/main.bundle.js";
                //     options.BootModuleBuilder = env.IsDevelopment()
                //         ? new AngularCliBuilder(npmScript: "build:ssr")
                //         : null;
                //     options.ExcludeUrls = new[] { "/sockjs-node", "/liveness", "/about" };
                // });

                if (env.IsDevelopment())
                {
                    //spa.UseAngularCliServer(npmScript: "start");
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:56810");
                }
            });
        }
    }
}
