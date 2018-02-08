using Hfa.SyncLibrary;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hfa.Notification.Brokers
{
    public class Common
    {
        public static ILogger Logger(string cat = "default") => _LoggerFactory.CreateLogger(cat);
        private static ILoggerFactory _LoggerFactory = null;

        static Common()
        {
            _LoggerFactory = (ILoggerFactory)Init.ServiceProvider.GetService(typeof(ILoggerFactory));
        }

    }
}
