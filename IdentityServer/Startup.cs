// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Quickstart.UI;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Claims;

namespace IdentityServer
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            services.AddTransient<IProfileService, SynkerProfileService>();

            services.Configure<IISOptions>(options =>
            {
                options.AutomaticAuthentication = false;
                options.AuthenticationDisplayName = "Windows";
            });

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
                .AddTestUsers(TestUsers.Users);

            // in-memory, json config
            builder.AddInMemoryIdentityResources(Configuration.GetSection("IdentityResources"));
            builder.AddInMemoryApiResources(Configuration.GetSection("ApiResources"));
            builder.AddInMemoryClients(Configuration.GetSection("clients"));

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }

            builder.AddProfileService<SynkerProfileService>();

            services.AddAuthentication()
                .AddGoogle("Google", options =>
                 {
                     options.ClientId = Configuration.GetValue<string>("StsAuthentificationConfiguration:Google:ClientId");
                     options.ClientSecret = Configuration.GetValue<string>("StsAuthentificationConfiguration:Google:Secret");

                     options.Scope.Add("profile");

                     options.ClaimActions.MapJsonKey(JwtClaimTypes.Subject, "id");
                     options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                     options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_Name");
                     options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_Name");
                     options.ClaimActions.MapJsonKey(JwtClaimTypes.Picture, "picture");
                     options.ClaimActions.MapJsonKey(ClaimTypes.Locality, "locale");
                     options.ClaimActions.MapJsonKey(JwtClaimTypes.Profile, "urn:google:profile");
                     options.ClaimActions.MapCustomJson(JwtClaimTypes.Gender, jobject =>

                        jobject.TryGetValue("gender", StringComparison.InvariantCultureIgnoreCase, out JToken gender)
                             ? gender.Value<string>().Equals("male", StringComparison.InvariantCultureIgnoreCase) ? "Mr" : "Mrs"
                             : string.Empty
                         );

                     options.SaveTokens = true;
                 });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}