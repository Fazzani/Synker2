using hfa.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace hfa.synker.batch.test
{
    public static class StartUp
    {
        static StartUp()
        {
            WebHostBuilder = new WebHostBuilder()
                .UseContentRoot(Path.Combine(string.Format("..{0}..{0}..{0}..{0}", Path.DirectorySeparatorChar), "WebApi"))
                .UseEnvironment("Development")
                .UseStartup<Startup>();

            _testServer = new TestServer(WebHostBuilder);
        }

        private static object _lockObject = new object();
        private static TestServer _testServer;
        public static IWebHostBuilder WebHostBuilder { get; }

        private static TestServer TestServer
        {
            get
            {
                lock (_lockObject)
                {
                    if (_testServer == null)
                        _testServer = new TestServer(WebHostBuilder);
                    return _testServer;
                }
            }
        }

        public static HttpClient Client => TestServer.CreateClient();
    }
}
