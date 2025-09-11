using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PT.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
               // .UseSetting("https_port", "443")
                .ConfigureLogging((hostingContext, config) =>
                {
                    config.ClearProviders();
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("appsettings.Base.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("appsettings.Email.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("appsettings.Socket.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("appsettings.WebsiteInfo.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("appsettings.Authorize.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("appsettings.Seo.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("appsettings.BindContent.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("appsettings.ExchangeRate.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("appsettings.AdvertisingHomepage.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("appsettings.RedirectLink.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("appsettings.Paypal.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("appsettings.Log.json", optional: true, reloadOnChange: true);
                })
                .UseStartup<Startup>()
            ;
    }
}
