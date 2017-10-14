using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace AspNet.Core.Webhooks
{
    /// <summary>
    /// WebHook configuration
    /// </summary>
    public class WebhookOptions<TWebHookMessage> where TWebHookMessage : new()
    {
        public string ApiKey { get; set; }

        public Action<HttpContext, TWebHookMessage> WebHookAction { get; set; }
    }
}