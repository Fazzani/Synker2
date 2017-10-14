using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;

namespace AspNet.Core.Webhooks.Receivers
{
    public class GithubReceiver : AbstractWebHookHandler<GithubOptions, GithubWebHookMessage>
    {
        private const string Sha1Prefix = "sha1=";
        public override string KeyToken { get; set; }

        public GithubReceiver(IHttpContextAccessor httpContext, GithubOptions githubOptions) 
            : base(httpContext, githubOptions)
        {
            KeyToken = "X-Hub-Signature";
        }

        public override bool IsWebHookRequest(HttpContext context) =>
             context.Request.Method.Equals("Post", StringComparison.InvariantCultureIgnoreCase)
                && context.Request.Path.HasValue && context.Request.Path.Value.StartsWith("/webhook/github");

        public override string GetSignature(HttpContext httpContext) =>
            ((string)(HttpContext.Request.Headers.FirstOrDefault(x => x.Key.Equals(KeyToken)).Value)).Substring(Sha1Prefix.Length);

        /// <summary>
        /// Assert matching signature
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public override bool AssertSignature(HttpContext httpContext)
        {
            var signature = GetSignature(httpContext);
            if (!string.IsNullOrEmpty(signature))
            {
                var requestBody = RequestBody().GetAwaiter().GetResult();

                var hash = new HMACSHA1(Encoding.ASCII.GetBytes(_options.ApiKey))
                    .ComputeHash(Encoding.ASCII.GetBytes(requestBody))
                    .Aggregate(string.Empty, (s, e) => s + String.Format("{0:x2}", e), s => s);

                if (!hash.Equals(signature))
                {
                    throw new Exception("WebHook Signatures didn't match!");
                }
            }
            else
            {
                throw new Exception("WebHook must be signed");
            }
            return true;
        }
    }

    public class GithubOptions : WebhookOptions<GithubWebHookMessage>
    {

    }

    public class GithubWebHookMessage
    {
        public string Hook_id { get; set; }
        public dynamic Hook { get; set; }
        public dynamic Repository { get; set; }
        public dynamic Sender { get; set; }
    }

    public static partial class WebHookExtensions
    {
        public static IServiceCollection UseGithubWebhook(this IServiceCollection serviceCollection, Func<GithubOptions> githubOptions)
        {
            serviceCollection.AddSingleton(x => githubOptions?.Invoke());
            return serviceCollection.AddScoped<GithubReceiver, GithubReceiver>();
        }
    }
}
