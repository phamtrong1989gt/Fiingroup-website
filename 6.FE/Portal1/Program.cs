using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PT.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
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
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

            builder.Build().Run();
        }
    }
}
