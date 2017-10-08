using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.WebApi.Common.Middleware
{
    public class WebHookMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _path;
        private readonly string _keyToken;
        private readonly string _headerkey;
        private readonly string _controllerPath;
        private readonly IWebHookService _webHookService;

        public WebHookMiddleware(RequestDelegate next, string path, string controllerPath, string keyToken, string headerKey)
        {
            _next = next;
            _path = path;
            _keyToken = keyToken;
            _headerkey = headerKey;
            _controllerPath = controllerPath;
            //_webHookService = webHookService;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Request.Method.Equals("Post", StringComparison.InvariantCultureIgnoreCase)
                && context.Request.Path.HasValue && context.Request.Path.Value.StartsWith($"/{_path}"))
            {
                using (var readStream = new StreamReader(context.Request.Body, Encoding.UTF8))
                {
                    try
                    {
                        //Get Body
                        var body = await readStream.ReadToEndAsync();
                        await ThrowBadRequestIfNull(context, body, CancellationToken.None);

                        //Assert signature
                        if (context.Request.Headers.Any(x => x.Key.Equals(_headerkey)))
                        {
                            var signature = context.Request.Headers.FirstOrDefault(x => x.Key.Equals(_headerkey));
                            if (!new HMACSHA256(Encoding.ASCII.GetBytes(_keyToken))
                                .ComputeHash(Encoding.ASCII.GetBytes(body))
                                .Aggregate(string.Empty, (s, e) => s + String.Format("{0:X2}", e), s => s)
                                .Equals(signature.Value))
                            {
                                throw new Exception("WebHook Signatures didn't match!");
                            }
                        }
                        else
                        {
                            throw new Exception("WebHook must be signed");
                        }

                        //Convert to HookOptions object
                        var hookOptions = await Task.Run(() => JsonConvert.DeserializeObject<HookOptions>(body));
                        await ThrowBadRequestIfNull(context, hookOptions, CancellationToken.None);
                        //_webHookService.HookOptions = hookOptions;
                        context.Response.Redirect($"{context.Request.Scheme}://{context.Request.Host}{_controllerPath}/{hookOptions.Action}");
                    }
                    catch (Exception ex)
                    {
                        await ThrowBadRequestIfNull(context, null, CancellationToken.None, ex.Message);
                    }

                }
                Console.WriteLine($"In WebHook {nameof(WebHookMiddleware)}");
            }
            else
                await _next.Invoke(context);
            // Clean up.
        }

        private static async Task ThrowBadRequestIfNull(HttpContext context, object data, CancellationToken cancellationToken, string message = "Bad request")
        {
            if (data == null)
            {
                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new { source = nameof(WebHookMiddleware), message = message }), cancellationToken);
            }
        }
    }


    public static class WebHookMiddlewareExtensions
    {
        /// <summary>
        /// Add Webhooks support
        /// </summary>
        /// <param name="builder">AppBuilder</param>
        /// <param name="generateKeyToken">Key token used for hashing signature</param>
        /// <param name="path">Path to map for WebHooks</param>
        /// <param name="headerSignatureKey">Header for signature check name</param>
        /// <returns></returns>
        public static IApplicationBuilder UseWebHooks(this IApplicationBuilder builder, Func<string> controllerPathFunc, Func<string> generateKeyToken, string path = "webhook", string headerSignatureKey = "X-Hub-Signature") =>
            builder.UseMiddleware<WebHookMiddleware>(path, controllerPathFunc?.Invoke(), generateKeyToken?.Invoke(), headerSignatureKey);
    }
}
