using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AspNet.Core.Webhooks
{
    public interface IWebHookHandler: IDisposable
    {
        /// <summary>
        /// Webhook Action
        /// </summary>
        void Invoke();

        /// <summary>
        /// Match webhook Request 
        /// </summary>
        /// <returns></returns>
        bool IsWebHookRequest(HttpContext httpContext);

        /// <summary>
        /// Webhook Token
        /// </summary>
        string KeyToken { get; set; }

        /// <summary>
        /// Get webhook signature
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
       string GetSignature(HttpContext httpContext);

        /// <summary>
        /// Assert matching signature
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        bool AssertSignature(HttpContext httpContext);

        /// <summary>
        /// Get body request
        /// </summary>
        /// <returns></returns>
        Task<string> RequestBody();
    }
}
