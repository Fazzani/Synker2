using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace AspNet.Core.Webhooks.Receivers
{
    public class AppveyorReceiver : AbstractWebHookHandler<AppveyorOptions, AppveyorWebHookMessage>
    {
        public override string KeyToken { get; set; }

        public AppveyorReceiver(IHttpContextAccessor httpContext, AppveyorOptions appveyorOptions) : base(httpContext, appveyorOptions)
        {
            KeyToken = "X-Hub-Signature";
        }

        public override bool IsWebHookRequest(HttpContext context) =>
             context.Request.Method.Equals("Post", StringComparison.InvariantCultureIgnoreCase)
                && context.Request.Path.HasValue && context.Request.Path.Value.StartsWith("/webhook/appveyor");

        public override string GetSignature(HttpContext httpContext) =>
            HttpContext.Request.Headers.FirstOrDefault(x => x.Key.Equals(KeyToken)).Value;
    }

    public class AppveyorOptions : WebhookOptions<AppveyorWebHookMessage>
    {

    }

    public class AppveyorWebHookMessage
    {
        public string Message { get; set; }
        public string Action { get; set; }

        public DateTime Created { get; set; }

        public override string ToString() => $"{Created} : {Message} : {Action}";
    }

    public static partial class WebHookExtensions
    {
        public static IServiceCollection UseAppVeyorWebhook(this IServiceCollection serviceCollection, Func<AppveyorOptions> appveyorOptions)
        {
            serviceCollection.AddSingleton(x => appveyorOptions?.Invoke());
            return serviceCollection.AddScoped<AppveyorReceiver, AppveyorReceiver>();
        }
    }
}
