using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PT.Base;
using PT.Base.Services;
using PT.Domain.Model;
using PT.Infrastructure;
using PT.Infrastructure.Interfaces;
using PT.Infrastructure.Repositories;
using PT.Shared;
using PT.UI.SignalR;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;

namespace PT.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            
            // ✅ CẤU HÌNH SERILOG - Ghi log theo mức độ vào các file riêng biệt
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
            // ✅ DATA PROTECTION - Bảo vệ dữ liệu nhạy cảm (cookies, tokens) với tên ứng dụng duy nhất
            services.AddDataProtection().SetApplicationName("trong_dz");

            // ✅ HTTP CLIENT FACTORY - Quản lý HttpClient hiệu quả, tránh port exhaustion
            // Cấu hình timeout và các settings cơ bản
            services.AddHttpClient("default", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30); // Timeout mặc định 30 giây
                client.DefaultRequestHeaders.Add("User-Agent", "PT.UI/1.0");
            });
            
            // HttpClient cho NewsAPIService
            services.AddHttpClient<INewsAPIService, NewsAPIService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // ✅ MEMORY CACHE - Tối ưu với giới hạn phù hợp cho ứng dụng enterprise
            services.AddMemoryCache(options =>
            {
                // Giới hạn cache = 2048 units (≈ 2GB nếu mỗi unit = 1MB)
                // Tăng từ 1024 lên 2048 để phù hợp với ứng dụng lớn
                options.SizeLimit = 2048;

                // Compaction: Dọn 25% cache khi đạt giới hạn (evict items có priority thấp trước)
                options.CompactionPercentage = 0.25;

                // Quét expired items mỗi 3 phút (giảm từ 5 phút) để giải phóng memory nhanh hơn
                options.ExpirationScanFrequency = TimeSpan.FromMinutes(3);
            });

            // ✅ OUTPUT CACHING - Cache toàn bộ HTTP response (tốt hơn Response Caching)
            // Tự động bỏ qua POST requests, chỉ cache GET requests
            services.AddOutputCache(options =>
            {
                //// ===== POLICY 1: TRANG CHỦ - Cache 5 phút =====
                //// Vary theo: Language (URL path segment), Query strings
                //options.AddPolicy("HomePage", builder => builder
                //    .Expire(TimeSpan.FromSeconds(Configuration.GetValue<int>("CacheSettings:HomePageDuration", 300))) // Mặc định 300s = 5 phút
                //    .Tag("home")
                //    .SetVaryByQuery("*") // Vary theo tất cả query params (nếu có)
                //    .SetVaryByRouteValue("language") // Vary theo language trong route (vi/en)
                //);

                //// ===== POLICY 2: CATEGORY PAGES - Cache 3 phút =====
                //// Vary theo: Language, Category ID/Slug, Page number
                //options.AddPolicy("CategoryPage", builder => builder
                //    .Expire(TimeSpan.FromSeconds(Configuration.GetValue<int>("CacheSettings:CategoryPageDuration", 180))) // Mặc định 180s = 3 phút
                //    .Tag("category")
                //    .SetVaryByQuery("page", "sort", "limit") // Vary theo pagination params
                //    .SetVaryByRouteValue("language", "slug", "id")
                //);

                //// ===== POLICY 3: CONTENT/DETAIL PAGES - Cache 10 phút =====
                //// Nội dung tĩnh, ít thay đổi -> cache lâu hơn
                //options.AddPolicy("ContentPage", builder => builder
                //    .Expire(TimeSpan.FromSeconds(Configuration.GetValue<int>("CacheSettings:ContentPageDuration", 600))) // Mặc định 600s = 10 phút
                //    .Tag("content")
                //    .SetVaryByRouteValue("language", "slug", "id")
                //);

                //// ===== POLICY 4: SEARCH PAGES - Cache ngắn 30 giây =====
                //// Search results thay đổi thường xuyên -> cache ngắn
                //options.AddPolicy("SearchPage", builder => builder
                //    .Expire(TimeSpan.FromSeconds(Configuration.GetValue<int>("CacheSettings:SearchPageDuration", 30))) // Mặc định 30s
                //    .Tag("search")
                //    .SetVaryByQuery("k", "page") // Vary theo keyword và page
                //    .SetVaryByRouteValue("language")
                //);

                //// ===== POLICY 5: STATIC PAGES - Cache 1 giờ =====
                //// About, Contact (nội dung tĩnh)
                //options.AddPolicy("StaticPage", builder => builder
                //    .Expire(TimeSpan.FromSeconds(Configuration.GetValue<int>("CacheSettings:StaticPageDuration", 3600))) // Mặc định 3600s = 1 giờ
                //    .Tag("static")
                //    .SetVaryByRouteValue("language")
                //);
            });

            // ✅ RESPONSE CACHING - Cache HTTP responses ở server-side
            services.AddResponseCaching(options =>
            {
                // Giới hạn kích thước response body tối đa = 64MB
                options.MaximumBodySize = 64 * 1024 * 1024;
                
                // Không phân biệt hoa thường trong đường dẫn
                options.UseCaseSensitivePaths = false;
                
                // Giảm SizeLimit từ 100MB xuống 50MB để tránh chiếm quá nhiều RAM
                // Cache size lớn không đồng nghĩa với hiệu suất cao
                options.SizeLimit = 50 * 1024 * 1024;
            });

            // ✅ ROUTING - Cấu hình định tuyến URL
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true; // URL chữ thường (SEO friendly)
                options.LowercaseQueryStrings = false; // Query string giữ nguyên case
                options.AppendTrailingSlash = false; // Không thêm "/" cuối URL
            });

            // ✅ COOKIE POLICY - Chính sách cookie theo GDPR
            services.Configure<CookiePolicyOptions>(options =>
            {
                // Không yêu cầu consent cho non-essential cookies (điều chỉnh theo yêu cầu GDPR)
                options.CheckConsentNeeded = context => false;
                
                // SameSite=None cho phép cross-site cookies (cần thiết cho OAuth, external login)
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // ✅ DB CONTEXT - Cấu hình Entity Framework Core với SQL Server
            services.AddDbContextPool<ApplicationContext>(options => // Dùng AddDbContextPool thay vì AddDbContext để tái sử dụng context instances
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        // Retry khi gặp lỗi tạm thời (transient errors): deadlock, timeout, connection issues
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5, // Tăng từ 3 lên 5 lần retry
                            maxRetryDelay: TimeSpan.FromSeconds(10), // Tăng delay tối đa từ 5s lên 10s
                            errorNumbersToAdd: null
                        );
                        
                        // Command timeout: 60 giây (tăng từ 30s cho queries phức tạp)
                        sqlOptions.CommandTimeout(60);
                        
                        // Migration assembly (nếu cần thiết)
                        // sqlOptions.MigrationsAssembly("PT.Infrastructure");
                    }),
                poolSize: 128); // Pool size = 128 contexts (mặc định 1024, giảm xuống phù hợp)

            // ✅ IDENTITY - Hệ thống xác thực và phân quyền người dùng
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // ===== MẬT KHẨU - Tăng cường bảo mật =====
                options.Password.RequireDigit = true; // Yêu cầu chữ số
                options.Password.RequiredLength = 10; // Tăng từ 8 lên 10 ký tự
                options.Password.RequireNonAlphanumeric = true; // Tăng cường: yêu cầu ký tự đặc biệt (thay vì false)
                options.Password.RequireUppercase = true; // Yêu cầu chữ hoa
                options.Password.RequireLowercase = true; // Tăng cường: yêu cầu chữ thường (thay vì false)
                options.Password.RequiredUniqueChars = 4; // Giảm từ 6 xuống 4 ký tự unique (6 hơi khắt khe)

                // ===== LOCKOUT - Khóa tài khoản sau nhiều lần đăng nhập sai =====
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30); // Giảm từ 120 phút xuống 30 phút (hợp lý hơn)
                options.Lockout.MaxFailedAccessAttempts = 5; // Giảm từ 10 xuống 5 lần (bảo mật cao hơn)
                options.Lockout.AllowedForNewUsers = true; // Áp dụng lockout cho user mới

                // ===== USER - Cấu hình tài khoản người dùng =====
                options.User.RequireUniqueEmail = true; // Email phải unique
                
                // ===== SIGN IN - Cấu hình đăng nhập =====
                options.SignIn.RequireConfirmedEmail = false; // Không bắt buộc xác nhận email (có thể bật = true)
                options.SignIn.RequireConfirmedPhoneNumber = false; // Không bắt buộc xác nhận phone
            })
            .AddEntityFrameworkStores<ApplicationContext>()
            .AddDefaultTokenProviders(); // Token providers cho password reset, email confirmation

            // ✅ CUSTOM CLAIMS FACTORY - Thêm custom claims vào user principal
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomClaimsPrincipalFactory>();
            
            // ✅ CONFIGURATION BINDING - Bind appsettings.json sections vào strongly-typed objects
            services.Configure<BaseSettings>(Configuration.GetSection("BaseSettings"));
            services.Configure<LogSettings>(Configuration.GetSection("LogSettings"));
            services.Configure<AuthorizeSettings>(Configuration.GetSection("AuthorizeSettings"));

            // ✅ APPLICATION COOKIE - Cấu hình cookie authentication
            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true; // Chỉ truy cập qua HTTP, không qua JavaScript (chống XSS)
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Chỉ gửi qua HTTPS (production)
                options.Cookie.SameSite = SameSiteMode.Lax; // Bảo vệ chống CSRF
                options.Cookie.Name = ".PhamTrong.Auth"; // Tên cookie custom
                
                // Thời gian sống cookie: tăng từ 2 giờ lên 8 giờ để giảm phiền người dùng
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                
                // Sliding expiration: gia hạn cookie khi user active (sau 1/2 thời gian ExpireTimeSpan)
                options.SlidingExpiration = true;
                
                // Đường dẫn redirect
                options.LoginPath = "/Login";
                options.LogoutPath = "/Logout";
                options.AccessDeniedPath = "/Admin/AccessDenied";
                
                // Tự động refresh cookie khi gần hết hạn
                options.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
                };
            });

            // ✅ LOCALIZATION - Đa ngôn ngữ
            var baseSettings = Configuration.GetSection("BaseSettings").Get<BaseSettings>();
            var supportedCultures = ListData.ListLanguage.Select(x => new CultureInfo(x.Id)).ToArray();
            
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(baseSettings.DefaultLanguage);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                
                // Fallback về parent culture nếu không tìm thấy resource (vd: en-US -> en)
                options.FallBackToParentCultures = true;
                options.FallBackToParentUICultures = true;

                // Custom culture provider từ URL
                options.RequestCultureProviders.Insert(0, new UrlRequestCultureProvider(baseSettings.DefaultLanguage)
                {
                    Options = options
                });
            });

            // ✅ DEPENDENCY INJECTION - Đăng ký repositories và services
            // System repositories
            services.AddScoped<IEmailSenderRepository, EmailSenderRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleAreaRepository, RoleAreaRepository>();
            services.AddScoped<IRoleGroupRepository, RoleGroupRepository>();
            services.AddScoped<IRoleControllerRepository, RoleControllerRepository>();
            services.AddScoped<IRoleActionRepository, RoleActionRepository>();
            services.AddScoped<IRoleDetailRepository, RoleDetailRepository>();
            services.AddScoped<ILogRepository, LogRepository>();
            services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            
            // Content repositories
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ILinkRepository, LinkRepository>();
            services.AddScoped<IContentPageCategoryRepository, ContentPageCategoryRepository>();
            services.AddScoped<IContentPageRelatedRepository, ContentPageRelatedRepository>();
            services.AddScoped<IContentPageRepository, ContentPageRepository>();
            services.AddScoped<IContentPageTagRepository, ContentPageTagRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
            
            // Business repositories
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
            services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
            services.AddScoped<ISeoSettingRepository, SeoSettingRepository>();
            services.AddScoped<IBindContentSettingRepository, BindContentSettingRepository>();
            services.AddScoped<IEmailSettingRepository, EmailSettingRepository>();
            services.AddScoped<IAPILoggerService, APILoggerService>();
            services.AddScoped<INewsAPIService, NewsAPIService>();
            services.AddScoped<IParameterRepository, ParameterRepository>();

            // Services
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IAutoCssService, AutoCssService>();
            
            // Generic repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(BaseRepository<>));

            // ✅ RESPONSE COMPRESSION - Nén response để giảm băng thông
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true; // Bật compression cho HTTPS
                
                // Thứ tự providers: Brotli trước (nén tốt hơn), fallback Gzip
                options.Providers.Add<BrotliCompressionProvider>(); // Brotli: nén tốt hơn Gzip 15-20%
                options.Providers.Add<GzipCompressionProvider>();
                
                // MIME types được nén
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    "text/plain",
                    "text/css",
                    "application/javascript",
                    "text/html",
                    "application/xml",
                    "text/xml",
                    "application/json",
                    "text/json",
                    "image/svg+xml",
                    "application/atom+xml",
                    "font/woff",
                    "font/woff2",
                    "application/font-woff",
                    "application/font-woff2"
                });
            });
            
            // ✅ COMPRESSION PROVIDERS - Cấu hình mức độ nén
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                // Brotli: Optimal = cân bằng giữa tốc độ và tỉ lệ nén
                options.Level = CompressionLevel.Optimal;
            });
            
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                // Gzip: Fastest = nhanh nhất (thay vì Optimal) để giảm CPU usage
                options.Level = CompressionLevel.Fastest;
            });

            // ✅ FORM OPTIONS - Cấu hình upload file
            services.Configure<FormOptions>(options =>
            {
                // Giới hạn multipart body = 200MB (≈ 200MB upload file)
                options.MultipartBodyLengthLimit = 209_715_200;
                
                // Buffer size cho multipart (8KB mặc định)
                options.MultipartBoundaryLengthLimit = 128;
                
                // Value count limit
                options.ValueCountLimit = 1024;
            });

            // ✅ SESSION - Quản lý session state
            services.AddDistributedMemoryCache(); // Cache cho session (có thể thay bằng Redis)
            
            services.AddSession(options =>
            {
                // Session timeout: tăng từ 30 phút lên 60 phút (phù hợp với ExpireTimeSpan của cookie)
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true; // Essential cookie (không cần consent)
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Chỉ HTTPS
                options.Cookie.Name = ".PhamTrong.Session";
            });

            // ✅ MVC & RAZOR PAGES - Cấu hình ASP.NET Core MVC
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false; // Legacy routing (MVC 2.x style)
                
                // ===== CACHE PROFILES - Định nghĩa các profile cache cho ResponseCache attribute =====
                
                // Profile 1: Cache ngắn 10 giây cho pagination/dynamic content
                options.CacheProfiles.Add("Default10Seconds", new CacheProfile
                {
                    Duration = 10, // 10 giây
                    Location = ResponseCacheLocation.Any, // Cache ở browser và proxy
                    VaryByQueryKeys = new[] { "*" } // Vary theo tất cả query params
                });

                // Profile 2: Cache dài 1 năm cho static content (hình ảnh, CSS, JS)
                options.CacheProfiles.Add("StaticContent", new CacheProfile
                {
                    Duration = 31536000, // 1 năm = 365 * 24 * 60 * 60
                    Location = ResponseCacheLocation.Any
                });

                // Profile 3: Cache 1 phút, ignore query parameters
                options.CacheProfiles.Add("IgnoreQueryParams", new CacheProfile
                {
                    Duration = 60, // 60 giây
                    Location = ResponseCacheLocation.Any,
                    VaryByQueryKeys = new string[] { } // Không vary theo query
                });
                
                // Profile 4: Cache 5 phút cho trang chủ, category pages
                options.CacheProfiles.Add("Default5Minutes", new CacheProfile
                {
                    Duration = 300, // 5 phút
                    Location = ResponseCacheLocation.Any,
                    VaryByHeader = "Accept-Language" // Vary theo ngôn ngữ
                });
                
                // ===== FILTERS - Thêm global filters =====
                // options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); // CSRF protection
            })
            .AddViewLocalization(
                LanguageViewLocationExpanderFormat.Suffix, 
                opts => { opts.ResourcesPath = "Resources"; }
            )
            .AddDataAnnotationsLocalization()
            .AddNewtonsoftJson(options =>
            {
                // JSON settings: CamelCase, ignore null values
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            // ✅ SERILOG - Đăng ký Serilog vào logging pipeline
            loggerFactory.AddSerilog();
            
            // ✅ EXCEPTION HANDLING - Xử lý lỗi theo môi trường
            if (env.IsDevelopment())
            {
                // Development: Hiển thị chi tiết lỗi
                app.UseDeveloperExceptionPage();
                
                // ⭐ FORCE ENABLE CACHE TRONG DEVELOPMENT (để test)
                // Bình thường ASP.NET Core tự động set no-cache trong dev mode
                // Middleware này sẽ ghi đè no-cache header
                app.Use(async (context, next) =>
                {
                    // Xóa no-cache headers nếu có
                    context.Response.OnStarting(() =>
                    {
                        // Chỉ xử lý GET requests (Output Cache chỉ cache GET)
                        if (context.Request.Method == "GET")
                        {
                            // Xóa các headers ngăn cache
                            context.Response.Headers.Remove("Cache-Control");
                            context.Response.Headers.Remove("Pragma");
                        }
                        return Task.CompletedTask;
                    });
                    
                    await next();
                });
            }
            else
            {
                // Production: Trang lỗi tùy chỉnh
                app.UseExceptionHandler("/Home/Error");
                app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
                
                // ✅ HSTS - HTTP Strict Transport Security (bắt buộc HTTPS)
                app.UseHsts(); // MaxAge mặc định = 30 ngày, có thể tùy chỉnh trong ConfigureServices
            }

            // ✅ APP HTTP CONTEXT - Static service locator pattern (anti-pattern, nhưng cần cho legacy code)
            AppHttpContext.Services = app.ApplicationServices;

            // ===== MIDDLEWARE PIPELINE - Thứ tự quan trọng =====

            // 1. Response Compression - Phải đặt trước Static Files
            app.UseResponseCompression();

            // 2. ⭐ OUTPUT CACHING - PHẢI ĐẶT SAU Response Compression và TRƯỚC Response Caching
            // Output Caching mạnh hơn Response Caching, cache toàn bộ response
            // Tự động bỏ qua POST requests (chỉ cache GET/HEAD)
            app.UseOutputCache();

            // 3. Response Caching - Cache responses (fallback cho endpoints không dùng Output Cache)
            app.UseResponseCaching();
            
            // 4. Security Headers - Thêm các header bảo mật
            app.Use(async (context, next) =>
            {
                // X-Content-Type-Options: Ngăn MIME sniffing
                //context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                
                //// X-Frame-Options: Chống clickjacking
                //context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                
                //// X-XSS-Protection: Bật XSS filter (legacy, nhưng vẫn hữu ích)
                //context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                
                //// Referrer-Policy: Kiểm soát referrer information
                //context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                
                // Content-Security-Policy: Chống XSS, injection (cấu hình cơ bản)
                // Lưu ý: Cần test kỹ CSP vì có thể break tính năng
                // context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';");
                
                await next();
            });

            // 5. Static Files - Phục vụ files tĩnh với cache headers
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    // Cache static files 90 ngày (thay vì 1 năm để linh hoạt hơn)
                    const int durationInSeconds = 90 * 24 * 60 * 60; // 90 ngày
                    ctx.Context.Response.Headers["Cache-Control"] = $"public,max-age={durationInSeconds}";
                    ctx.Context.Response.Headers["Expires"] = DateTime.UtcNow.AddDays(90).ToString("R");
                }
            });

            // 6. Request Localization - Xác định ngôn ngữ
            var localizationOption = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(localizationOption.Value);

            // 7. Cookie Policy - Áp dụng cookie policy
            app.UseCookiePolicy();

            // 8. Session - Bật session middleware
            app.UseSession();

            // 9. Authentication - Xác thực người dùng
            app.UseAuthentication();

            // 10. MVC Routing - Định tuyến requests
            app.UseMvc(routes =>
            {
                // Custom router
                routes.Routes.Add(new CustomRouter(routes.DefaultHandler));
                
                // Admin area route
                routes.MapRoute(
                    name: "areas",
                    template: "Admin/{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );

                // Default route
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}"
                );
            });
        }
    }
}