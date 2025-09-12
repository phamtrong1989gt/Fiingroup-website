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

        // Cấu hình các dịch vụ sử dụng trong ứng dụng
        public void ConfigureServices(IServiceCollection services)
        {
            // Bảo vệ dữ liệu, dùng cho các chức năng như xác thực, cookie, v.v.
            services.AddDataProtection().SetApplicationName("admin");
            // Bộ nhớ đệm trong RAM
            services.AddMemoryCache();
            // Định tuyến cho ứng dụng
            services.AddRouting();
            // Cấu hình cookie policy (chính sách cookie)
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            // Cấu hình DbContext sử dụng SQL Server
            services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);
            // Cấu hình Identity cho xác thực người dùng
            services.AddIdentity<ApplicationUser, ApplicationRole>().AddEntityFrameworkStores<ApplicationContext>().AddDefaultTokenProviders();
            // Xử lý custom claims cho người dùng
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomClaimsPrincipalFactory>();
            // Đăng ký các cấu hình từ file appsettings
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

            // Cấu hình các quy tắc về mật khẩu, khóa tài khoản, email duy nhất
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 6;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(120);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail = true;
            });

            // Cấu hình cookie xác thực
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(2);
                options.LoginPath = "/Login";
                options.LogoutPath = "/Logout";
                options.AccessDeniedPath = "/Admin/AccessDenied";
                options.SlidingExpiration = true;
            });

            var baseSettings = Configuration.GetSection("BaseSettings").Get<BaseSettings>();
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en");
                options.SupportedCultures = [new CultureInfo("en")];
                options.SupportedUICultures = [new CultureInfo("en")];
                options.RequestCultureProviders.Insert(0, new UrlRequestCultureProvider(baseSettings.DefaultLanguage)
                {
                    Options = options
                });
            });

            // Đăng ký các repository cho DI
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
            services.AddScoped<IPortalRepository, PortalRepository>();
            // Đăng ký repository tổng quát
            services.AddScoped(typeof(IGenericRepository<>), typeof(BaseRepository<>));
            // Cấu hình nén Gzip cho phản hồi
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = new string[]{
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
                    };
            });
            // Cấu hình upload file lớn
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 209_715_200;
            });
            // Bộ nhớ đệm phân tán
            services.AddDistributedMemoryCache();
            // Cấu hình session cho người dùng
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = ".PhamTrong.Session";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });
            // Cấu hình MVC cho ứng dụng (không dùng endpoint routing)
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            })
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();
        }

        // Cấu hình pipeline xử lý HTTP request
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            // Cấu hình logging sử dụng Serilog
            loggerFactory.AddSerilog();
            // Xử lý lỗi khi chạy ở môi trường phát triển hoặc production
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
            // Lưu lại dịch vụ để sử dụng toàn cục
            AppHttpContext.Services = app.ApplicationServices;
            // Kích hoạt nén Gzip cho phản hồi
            app.UseResponseCompression();
            // Cấu hình phục vụ file tĩnh và cache
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={604800* 58}");
                }
            });
            // Cấu hình cookie policy
            app.UseCookiePolicy();
            // Kích hoạt session
            app.UseSession();
            // Kích hoạt xác thực
            app.UseAuthentication();
            // Cấu hình định tuyến MVC (có hỗ trợ khu vực và route mặc định)
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                name: "areas",
                template: "Admin/{area:exists}/{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
