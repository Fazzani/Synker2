using System;

namespace hfa.WebApi.Common.Middleware
{
    public interface IWebHookService
    {
        HookOptions HookOptions { get; set; }
    }

    public class WebHookServiceDefault : IWebHookService
    {
        public HookOptions HookOptions { get; set; }
    }

    public class HookOptions
    {
        public string Action { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// DateTime
        /// </summary>
        public DateTime Dt { get; set; }

        public string Signature { get; set; }
    }
}