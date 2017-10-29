using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using System.Security.Claims;
using hfa.WebApi.Common.Auth;
using hfa.WebApi.Dal;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Bazinga.AspNetCore.Authentication.Basic;

namespace hfa.WebApi.Common.Middlewares
{

    public class BasicAuth : IBasicCredentialVerifier
    {
        private IAuthentificationService _authentificationService;
        private SynkerDbContext _synkerDbContext;
        private ILoggerFactory _loggerFactory;

        public BasicAuth(IAuthentificationService authentificationService, SynkerDbContext synkerDbContext, ILoggerFactory loggerFactory)
        {
            _authentificationService = authentificationService;
            _synkerDbContext = synkerDbContext;
            _loggerFactory = loggerFactory;
        }
        public  Task<bool> Authenticate(string username, string password)
        {
            var user = _synkerDbContext.Users.Include(x => x.ConnectionState).SingleOrDefault(it => it.ConnectionState.UserName == username);
            if (user != null && _authentificationService.VerifyPassword(password, user.ConnectionState.Password))
            {
                //TODO: Ajout les claims au principal

                //var claims = _authentificationService.GetClaims(user);
                //var identity = new ClaimsIdentity(claims, "Basic");
                //context.User = new ClaimsPrincipal(identity);

                user.ConnectionState.LastConnection = DateTime.UtcNow;
                _synkerDbContext.SaveChanges();

                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }

    ///// <summary>
    ///// Accepts either username or email as user identifier for sign in with Http Basic authentication
    ///// </summary>
    //public class BasicAuthenticationMiddleware
    //{
    //    public BasicAuthenticationMiddleware(RequestDelegate next)
    //    {
    //        _next = next;
    //    }

    //    private readonly RequestDelegate _next;

    //    public async Task Invoke(HttpContext context, IAuthentificationService authentificationService, SynkerDbContext synkerDbContext, ILoggerFactory loggerFactory)
    //    {
    //        if (!context.User.Identity.IsAuthenticated)
    //        {
    //            var basicAuthenticationHeader = GetBasicAuthenticationHeaderValue(context);
    //            if (basicAuthenticationHeader.IsValidBasicAuthenticationHeaderValue)
    //            {
    //                var user = synkerDbContext.Users.Include(x => x.ConnectionState).SingleOrDefault(it => it.ConnectionState.UserName == basicAuthenticationHeader.UserName);
    //                if (user != null && authentificationService.VerifyPassword(basicAuthenticationHeader.Password, user.ConnectionState.Password))
    //                {
    //                    var claims = authentificationService.GetClaims(user);
    //                    var identity = new ClaimsIdentity(claims, "Basic");
    //                    context.User = new ClaimsPrincipal(identity);
    //                }
    //                else
    //                {
    //                    context.Response.StatusCode = 401;
    //                    return;
    //                    //context.Response.Headers.Set("WWW-Authenticate", "Basic realm=\"dotnetthoughts.net\"");
    //                }
    //            }
    //        }
    //        await _next.Invoke(context);
    //    }

    //    private BasicAuthenticationHeaderValue GetBasicAuthenticationHeaderValue(HttpContext context)
    //    {
    //        var basicAuthenticationHeader = context.Request.Headers["Authorization"]
    //            .FirstOrDefault(header => header.StartsWith("Basic", StringComparison.OrdinalIgnoreCase));
    //        var decodedHeader = new BasicAuthenticationHeaderValue(basicAuthenticationHeader);
    //        return decodedHeader;
    //    }
    //}

    //public static class MiddlewareExtensions
    //{
    //    public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder builder)
    //    {
    //        return builder.UseMiddleware<BasicAuthenticationMiddleware>();
    //    }
    //}

    //public class BasicAuthenticationHeaderValue
    //{
    //    public BasicAuthenticationHeaderValue(string authenticationHeaderValue)
    //    {
    //        if (!string.IsNullOrWhiteSpace(authenticationHeaderValue))
    //        {
    //            _authenticationHeaderValue = authenticationHeaderValue;
    //            if (TryDecodeHeaderValue())
    //            {
    //                ReadAuthenticationHeaderValue();
    //            }
    //        }
    //    }

    //    private readonly string _authenticationHeaderValue;
    //    private string[] _splitDecodedCredentials;

    //    public bool IsValidBasicAuthenticationHeaderValue { get; private set; }
    //    public string UserName { get; private set; }
    //    public string Password { get; private set; }

    //    private bool TryDecodeHeaderValue()
    //    {
    //        const int headerSchemeLength = 6; // "Basic ".Length;
    //        if (_authenticationHeaderValue.Length <= headerSchemeLength)
    //        {
    //            return false;
    //        }
    //        var encodedCredentials = _authenticationHeaderValue.Substring(headerSchemeLength);
    //        try
    //        {
    //            var decodedCredentials = Convert.FromBase64String(encodedCredentials);
    //            _splitDecodedCredentials = System.Text.Encoding.ASCII.GetString(decodedCredentials).Split(':');
    //            return true;
    //        }
    //        catch (FormatException)
    //        {
    //            return false;
    //        }
    //    }

    //    private void ReadAuthenticationHeaderValue()
    //    {
    //        IsValidBasicAuthenticationHeaderValue = _splitDecodedCredentials.Length == 2
    //                                               && !string.IsNullOrWhiteSpace(_splitDecodedCredentials[0])
    //                                               && !string.IsNullOrWhiteSpace(_splitDecodedCredentials[1]);
    //        if (IsValidBasicAuthenticationHeaderValue)
    //        {
    //            UserName = _splitDecodedCredentials[0];
    //            Password = _splitDecodedCredentials[1];
    //        }
    //    }
    //}
}
