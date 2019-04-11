namespace hfa.WebApi
{
    using hfa.Synker.Service.Entities.Auth;
    using hfa.Synker.Services.Dal;
    using hfa.WebApi.Common.Auth;
    using Hfa.WebApi.Controllers;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    public static class StartUpSecurity
    {
        public static void AddSecurity(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddCookie()
              .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtBearerOptions =>
              {
                  var jwtOptions = Startup.Configuration.GetSection(nameof(JwtBearerOptions)).Get<JwtBearerOptions>();
                  jwtBearerOptions.SaveToken = jwtOptions.SaveToken;
                  jwtBearerOptions.Authority = jwtOptions.Authority;
                  jwtBearerOptions.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;
                  jwtBearerOptions.Audience = jwtOptions.Audience;
                  jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                  {
                      ValidateIssuer = true,
                      ValidateAudience = true
                  };

                  var sp = services.BuildServiceProvider();
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
                   path.StartsWithSegments("/hubs/notification"))
                          {
                              // Read the token out of the query string
                              context.Token = accessToken;
                          }
                          return Task.CompletedTask;
                      },
                      OnTokenValidated = context =>
                      {
                          // Add the access_token as a claim, as we may actually need it
                          if (context.SecurityToken is JwtSecurityToken accessToken)
                          {
                              var identity = context.Principal.Identity as ClaimsIdentity;
                              var db = Startup.Provider.GetService<SynkerDbContext>();
                              var userEmail = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
                              if (userEmail != null)
                              {
                                  var dbUser = db.Users.FirstOrDefault(u => u.Email.Equals(userEmail.Value, StringComparison.CurrentCultureIgnoreCase));
                                  if (dbUser == null)
                                  {
                                      db.Users.Add(new User { Email = userEmail.Value });
                                      db.SaveChanges();
                                      dbUser = db.Users.FirstOrDefault(u => u.Email.Equals(userEmail.Value, StringComparison.CurrentCultureIgnoreCase));
                                  }
                                  identity.AddClaim(new Claim(Constants.CLAIM_LOCAL_ID, dbUser.Id.ToString(), ClaimValueTypes.Integer32));
                              }
                          }

                          return Task.CompletedTask;
                      }
                  };
              });

            services.AddAuthorization(authorizationOptions =>
            {
                authorizationOptions.AddPolicy(AuthorizePolicies.ADMIN, policyBuilder => policyBuilder.RequireRole(AuthorizePolicies.ADMIN));
                authorizationOptions.AddPolicy(AuthorizePolicies.FULLACCESS, policyBuilder => policyBuilder.RequireClaim("scope", "synkerapi.full_access"));
                authorizationOptions.AddPolicy(AuthorizePolicies.READER_ONLY, policyBuilder => policyBuilder.RequireClaim("scope", "synkerapi.read_only"));
                authorizationOptions.AddPolicy(AuthorizePolicies.READER, policyBuilder => policyBuilder.RequireAssertion(a => a.User.Claims.Where(c => c.Type == "scope").Any(scope => scope.Value == "synkerapi.full_access" || scope.Value == "synkerapi.read_only")));
                authorizationOptions.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build();
            });
        }

    }
}
