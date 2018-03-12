using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using hfa.WebApi;
using System.Security.Cryptography.X509Certificates;
using System.Net;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddEnvironmentVariables()
               //.AddJsonFile("certificate.json", optional: true, reloadOnChange: true)
               //.AddJsonFile($"certificate.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", reloadOnChange: true, optional: true)
               .Build();

            //var certificateSettings = config.GetSection("certificateSettings");
            //string certificateFileName = certificateSettings.GetValue<string>("filename");
            //string certificatePassword = certificateSettings.GetValue<string>("password");

            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel()
                //.UseKestrel(options =>
                //{
                //    options.Listen(IPAddress.Loopback, 56800);
                //    options.Listen(IPAddress.Loopback, 44312, listenOptions =>
                //    {
                //        listenOptions.UseHttps(certificateFileName, certificatePassword);
                //    });
                //})
                //.UseIISIntegration()
                .UseUrls("http://*:56800")
                .Build();
        }
    }
}
