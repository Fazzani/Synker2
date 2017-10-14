using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Core.Webhooks;

namespace AspNet.Core.Webhooks
{
    public class WebHookMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Type _webhookHandler;

        public WebHookMiddleware(RequestDelegate next, Type webhookHandler)
        {
            _next = next;
            _webhookHandler = webhookHandler;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            using (var handler = (IWebHookHandler)context.RequestServices.GetService(_webhookHandler))
            {
                if (handler != null && handler.IsWebHookRequest(context))
                {
                    try
                    {
                        var body = await handler.RequestBody();
                        await ThrowBadRequestIfNull(context, body, CancellationToken.None);
                        handler.AssertSignature(context);
                        handler.Invoke();
                    }
                    catch (Exception ex)
                    {
                        await ThrowBadRequestIfNull(context, null, CancellationToken.None, ex.Message);
                    }
                }
            }
            await _next.Invoke(context);
        }

        private async Task ThrowBadRequestIfNull(HttpContext context, object data, CancellationToken cancellationToken, string message = "Bad request")
        {
            if (data == null)
            {
                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new { source = _webhookHandler.Name, message = message }), cancellationToken);
            }
        }
    }
}
namespace Microsoft.AspNetCore.Builder
{

    public static class WebHookMiddlewareExtensions
    {
        /// <summary>
        /// Add Webhooks support
        /// </summary>
        /// <param name="handlerType">Handler Type</param>
        /// <returns></returns>
        public static IApplicationBuilder UseWebHooks(this IApplicationBuilder builder, Type handlerType) =>
            builder.UseMiddleware<WebHookMiddleware>(handlerType);
    }
}