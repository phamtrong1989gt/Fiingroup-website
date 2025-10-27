using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure;
using PT.Infrastructure.Interfaces;
using PT.Infrastructure.Repositories;
using PT.Shared;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PT.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Information).WriteTo.File("logs/info_.log", rollingInterval: RollingInterval.Day))
            .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Warning).WriteTo.File("logs/warning_.log", rollingInterval: RollingInterval.Day))
            .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Error).WriteTo.File("logs/error_.log", rollingInterval: RollingInterval.Day))
            .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Fatal).WriteTo.File("logs/fatal_.log", rollingInterval: RollingInterval.Day))
            .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Debug).WriteTo.File("logs/debug_.log", rollingInterval: RollingInterval.Day))
            .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection().SetApplicationName("rosedentalclinic");
            // Thêm MemoryCache vào container DI
            services.AddMemoryCache();
            services.AddRouting();
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);
            services.AddIdentity<ApplicationUser, ApplicationRole>().AddEntityFrameworkStores<ApplicationContext>().AddDefaultTokenProviders();
            // Add Custom Claims processor
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomClaimsPrincipalFactory>();
            services.Configure<BaseSettings>(Configuration.GetSection("BaseSettings"));
            services.Configure<PaypalSettings>(Configuration.GetSection("PaypalSettings"));
            services.Configure<LogSettings>(Configuration.GetSection("LogSettings"));
            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.Configure<SocketSettings>(Configuration.GetSection("SocketSettings"));
            services.Configure<List<SeoSettings>>(Configuration.GetSection("SeoSettings"));
            services.Configure<List<WebsiteInfoSettings>>(Configuration.GetSection("WebsiteInfoSettings"));
            services.Configure<ExchangeRateSettings>(Configuration.GetSection("ExchangeRateSettings"));
            services.Configure<BindContentSettings>(Configuration.GetSection("BindContentSettings"));
            services.Configure<List<AdvertisingHomepageSettings>>(Configuration.GetSection("AdvertisingHomepageSettings"));
            services.Configure<AuthorizeSettings>(Configuration.GetSection("AuthorizeSettings"));
            services.Configure<List<RedirectLinkSetting>>(Configuration.GetSection("RedirectLinkSettings"));
            services.AddMemoryCache();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(120);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(2);
                options.LoginPath = "/Login"; // If the LoginPath is not set here, ASP.NET Core will default to /Account/Login
                options.LogoutPath = "/Logout"; // If the LogoutPath is not set here, ASP.NET Core will default to /Account/Logout
                options.AccessDeniedPath = "/Admin/AccessDenied"; // If the AccessDeniedPath is not set here, ASP.NET Core will default to /Account/AccessDenied
                options.SlidingExpiration = true;

            });

            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //      .AddCookie(options =>
            //      {
            //          options.Cookie.HttpOnly = true;
            //          options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            //          options.Cookie.SameSite = SameSiteMode.Lax;
            //      });

            var supportedCultures = ListData.ListLanguage.Select(x => new CultureInfo(x.Id)).ToArray();
            // Lấy cấu hình từ base setting xem mặc định ngôn ngữ là gì
            var baseSettings = Configuration.GetSection("BaseSettings").Get<BaseSettings>();
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(baseSettings.DefaultLanguage);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            
                options.RequestCultureProviders.Insert(0, new UrlRequestCultureProvider(baseSettings.DefaultLanguage)
                {
                    Options = options
                });
            });

            // Adding our UrlRequestCultureProvider as first object in the list
            
            // Ngôn ngữ End
            // Add application services.
            services.AddScoped<IEmailSenderRepository, EmailSenderRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleAreaRepository, RoleAreaRepository>();
            services.AddScoped<IRoleGroupRepository, RoleGroupRepository>();
            services.AddScoped<IRoleControllerRepository, RoleControllerRepository>();
            services.AddScoped<IRoleControllerRepository, RoleControllerRepository>();
            services.AddScoped<IRoleActionRepository, RoleActionRepository>();
            services.AddScoped<IRoleDetailRepository, RoleDetailRepository>();
            services.AddScoped<ILogRepository, LogRepository>();
            services.AddScoped<IEmailSenderRepository, EmailSenderRepository>();
            services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ILinkRepository, LinkRepository>();

            services.AddScoped<IContentPageCategoryRepository, ContentPageCategoryRepository>();
            services.AddScoped<IContentPageRelatedRepository, ContentPageRelatedRepository>();
            services.AddScoped<IContentPageRepository, ContentPageRepository>();
            services.AddScoped<IContentPageTagRepository, ContentPageTagRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
            
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IStaticInformationRepository, StaticInformationRepository>();

            services.AddScoped<IMenuItemRepository, MenuItemRepository>();
            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<IBannerRepository, BannerRepository>();
            services.AddScoped<IBannerItemRepository, BannerItemRepository>();
            services.AddScoped<IServicePriceRepository, ServicePriceRepository>();
            services.AddScoped<IImageGalleryRepository, ImageGalleryRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped<ILinkReferenceRepository, LinkReferenceRepository>();
            services.AddScoped<IContentPageReferenceRepository, ContentPageReferenceRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<ITourCategoryRepository, TourCategoryRepository>();
            services.AddScoped<ITourDayRepository, TourDayRepository>();
            services.AddScoped<ITourRepository, TourRepository>();
            services.AddScoped<ITourGalleryRepository, TourGalleryRepository>();
            services.AddScoped<ITourDayGalleryRepository, TourDayGalleryRepository>();
            services.AddScoped<ITourTypeRepository, TourTypeRepository>();
            services.AddScoped<IFileDataRepository, FileDataRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
            // Đăng ký DI cho repository tổng quát
            services.AddScoped(typeof(IGenericRepository<>), typeof(BaseRepository<>));
            //Gzip
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);

            services.AddResponseCompression(options =>
            {
                //options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = [
                        "text/plain",
                        "text/css",
                        "application/javascript",
                        "text/html",
                        "application/xml",
                        "text/xml",
                        "application/json",
                        "text/json",
                        "image/svg+xml",
                        "application/atom+xml"
                    ];
            });
       
            //Content/Admin/plugins/signalr
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 209_715_200;
            });

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = ".PhamTrong.Session";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            //services.AddHttpsRedirection(options =>
            //{
            //    options.HttpsPort = 443;
            //    options.RedirectStatusCode = 301;
            //});

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                //options.Filters.Add(new RequireHttpsAttribute
                //{
                //    Permanent = true
                //});
                //options.Filters.Add(new RequireWwwAttribute
                //{
                //    IgnoreLocalhost = true,
                //    Permanent = true
                //});
             })
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix, opts => { opts.ResourcesPath = "Resources"; })
                .AddDataAnnotationsLocalization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            loggerFactory.AddSerilog();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
                app.UseHsts();
            }

            AppHttpContext.Services = app.ApplicationServices;

            //Gzip
            app.UseResponseCompression();
            //app.UseHttpsRedirection();

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={604800* 58}");
                }
            });

            var localizationOption = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(localizationOption.Value);

            app.UseCookiePolicy();
            app.UseSession();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.Routes.Add(new CustomRouter(routes.DefaultHandler));
                routes.MapRoute(
                name: "areas",
                template: "Admin/{area:exists}/{controller=Home}/{action=Index}/{id?}"
            );

             routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
