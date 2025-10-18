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
    // Lớp Startup cấu hình dịch vụ và middleware pipeline cho ứng dụng
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            // Cấu hình Serilog để ghi log ra file theo mức độ
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
            // DataProtection: dùng để mã hóa/giải mã dữ liệu (cookie, token, v.v.)
            // SetApplicationName giúp chia sẻ key giữa các app cùng tên (nếu cần)
            services.AddDataProtection().SetApplicationName("admin");

            // Bộ nhớ cache trong bộ nhớ process (in-memory cache)
            services.AddMemoryCache();

            // Đăng ký routing (cần thiết cho MVC/Controllers)
            services.AddRouting();

            // Cấu hình chính sách cookie (ví dụ: SameSite, consent)
            services.Configure<CookiePolicyOptions>(options =>
            {
                // Không yêu cầu consent để set cookie
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Đăng ký DbContext với SQL Server (scoped theo request)
            services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);

            // Cấu hình Identity (user/role) sử dụng EF store
            services.AddIdentity<ApplicationUser, ApplicationRole>().AddEntityFrameworkStores<ApplicationContext>().AddDefaultTokenProviders();

            // Custom claims factory: thêm claim tuỳ chỉnh khi tạo ClaimsPrincipal
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomClaimsPrincipalFactory>();

            // Bind các cấu hình từ appsettings.json vào POCOs để dễ sử dụng qua IOptions
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

            // Gọi lại AddMemoryCache nếu cần (không gây lỗi, nhưng có thể thừa)
            services.AddMemoryCache();

            // Cấu hình chính sách mật khẩu, lockout, và yêu cầu email duy nhất
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

            // Cấu hình cookie dùng cho Authentication
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(2);
                options.LoginPath = "/Login"; // đường dẫn trang login
                options.LogoutPath = "/Logout";
                options.AccessDeniedPath = "/Admin/AccessDenied";
                options.SlidingExpiration = true; // gia hạn cookie khi có hoạt động
            });

            // Lấy cấu hình BaseSettings để sử dụng cho localization provider
            var baseSettings = Configuration.GetSection("BaseSettings").Get<BaseSettings>();

            // Cấu hình localization (culture, supported cultures, provider)
            services.Configure<RequestLocalizationOptions>(options =>
            {
                // Tạo danh sách culture được hỗ trợ (ví dụ: "vi")
                var supported = new List<CultureInfo> { new CultureInfo(baseSettings.DefaultLanguage) };
                options.DefaultRequestCulture = new RequestCulture(baseSettings.DefaultLanguage);
                options.SupportedCultures = supported;
                options.SupportedUICultures = supported;

                // UrlRequestCultureProvider: provider custom dùng segment URL để xác định culture
                options.RequestCultureProviders.Insert(0, new UrlRequestCultureProvider(baseSettings.DefaultLanguage)
                {
                    Options = options
                });
            });

            // Đăng ký các repository cho DI (scoped phù hợp với DbContext)
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
            // Đăng ký repository tổng quát cho các entity kiểu chung
            services.AddScoped(typeof(IGenericRepository<>), typeof(BaseRepository<>));

            // Cấu hình nén response (Gzip)
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                // Những mime type nên nén
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

            // Cấu hình giới hạn upload (multipart)
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 209_715_200; // ~200MB
            });

            // Cache phân tán (dùng cho session nếu cần scale-out)
            services.AddDistributedMemoryCache();

            // Cấu hình session
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = ".PhamTrong.Session";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // chỉ gửi cookie qua HTTPS
            });

            // Cấu hình MVC và localization cho view + data annotations
            // Note: EnableEndpointRouting = false dùng legacy routing (UseMvc)
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
            // Kết nối Serilog với hệ thống logging của ASP.NET Core
            loggerFactory.AddSerilog();

            // Xử lý lỗi: developer vs production
            if (env.IsDevelopment())
            {
                // Trang lỗi chi tiết khi phát triển
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Handler chung cho production; chuyển hướng mã lỗi vào trang lỗi
                app.UseExceptionHandler("/Home/Error");
                app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
                app.UseHsts(); // HTTP Strict Transport Security
            }

            // Lưu lại IServiceProvider toàn cục (sử dụng cẩn trọng: service locator anti-pattern)
            AppHttpContext.Services = app.ApplicationServices;

            // Kích hoạt response compression đã cấu hình
            app.UseResponseCompression();

            // Phục vụ file tĩnh và thêm header cache-control để cache lâu trên client/CDN
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={604800* 58}");
                }
            });

            // Cookie policy middleware
            app.UseCookiePolicy();

            // Session middleware: bắt buộc gọi trước khi dùng session trong controller
            app.UseSession();

            // Thiết lập culture mặc định sớm để model binding/validation parse ngày/number theo culture này
            var vi = new CultureInfo("vi");
            vi.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy"; // định dạng ngày ngắn
            vi.DateTimeFormat.DateSeparator = "/";
            CultureInfo.DefaultThreadCurrentCulture = vi;
            CultureInfo.DefaultThreadCurrentUICulture = vi;

            // Lấy RequestLocalizationOptions đã cấu hình trong ConfigureServices và kích hoạt
            var locOptions = app.ApplicationServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value;
            app.UseRequestLocalization(locOptions);

            // Authentication/Authorization middleware: phải nằm trước routing/controller execution
            app.UseAuthentication();
            app.UseAuthorization();

            // Routing bằng legacy MVC (có support area)
            app.UseMvc(routes =>
            {
                // Route cho area (Admin/...)
                routes.MapRoute(
                name: "areas",
                template: "Admin/{area:exists}/{controller=Home}/{action=Index}/{id?}");

                // Route mặc định
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}