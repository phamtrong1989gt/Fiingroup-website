using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PT.Base
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class RequireWwwAttribute : Attribute, IAuthorizationFilter, IOrderedFilter
    {

        private bool? permanent;
        public bool Permanent
        {
            get => permanent ?? true;
            set => permanent = value;
        }

        private bool? ignoreLocalhost;
        public bool IgnoreLocalhost
        {
            get => ignoreLocalhost ?? true;
            set => ignoreLocalhost = value;
        }

        public int Order { get; set; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var req = context.HttpContext.Request;
            var host = req.Host;
            var isLocalHost = string.Equals(host.Host, "localhost", StringComparison.OrdinalIgnoreCase);
            if (IgnoreLocalhost && isLocalHost)
            {
                return;
            }
        }
    }

    public class UrlRequestCultureProvider : RequestCultureProvider
    {
        private static string _defaultCulture = "vi";
        
        // Cache language mappings to avoid repeated LINQ queries (thread-safe, read-only after init)
        private static readonly Lazy<Dictionary<string, string>> _cultureIdToId2Map = new(() =>
        {
            return ListData.ListLanguage?.ToDictionary(
                x => x.Id.ToLowerInvariant(),
                x => x.Id2,
                StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        });

        private static readonly Lazy<Dictionary<string, string>> _cultureId2ToIdMap = new(() =>
        {
            return ListData.ListLanguage?.ToDictionary(
                x => x.Id2,
                x => x.Id.ToLowerInvariant(),
                StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        });

        private static readonly Lazy<HashSet<string>> _validLanguages = new(() =>
        {
            return ListData.ListLanguage?.Select(x => x.Id.ToLowerInvariant()).ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "vi", "en" };
        });

        public UrlRequestCultureProvider(string defaultCulture = null)
        {
            _defaultCulture = defaultCulture ?? "vi";
        }

        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            // Get path once (avoid multiple accesses)
            var pathValue = httpContext.Request.Path.Value;
            if (string.IsNullOrEmpty(pathValue))
            {
                return Task.FromResult((ProviderCultureResult)null);
            }

            // Fast path: check static files using Span (zero-allocation)
            var pathSpan = pathValue.AsSpan();
            if (pathSpan.Length > 4) // Minimum ".xxx"
            {
                var lastDotIndex = pathSpan.LastIndexOf('.');
                if (lastDotIndex > 0 && IsStaticExtension(pathSpan.Slice(lastDotIndex)))
                {
                    return Task.FromResult((ProviderCultureResult)null);
                }
            }

            // Normalize path once
            var path = pathValue.TrimEnd('/').ToLowerInvariant();

            // Quick check for special paths (e.g., /data/, /css/, /api/)
            if (IsSpecialPathPrefix(path))
            {
                return Task.FromResult((ProviderCultureResult)null);
            }

            // Determine target language
            string targetLanguage = _defaultCulture;

            // Extract language from URL if present
            // Valid patterns: /vi/, /vi/news.html, /en/about.html
            if (path.Length > 1) // Has content after /
            {
                // Extract first segment efficiently
                var firstSlashIndex = path.IndexOf('/', 1);
                var firstSegment = firstSlashIndex > 1 
                    ? path.Substring(1, firstSlashIndex - 1) 
                    : path.Substring(1);

                // Check if first segment is a valid language
                if (_validLanguages.Value.Contains(firstSegment))
                {
                    targetLanguage = firstSegment;
                }
            }

            // Always set culture (no comparison with current culture)
            return Task.FromResult(new ProviderCultureResult(targetLanguage));
        }

        // Optimized static extension check using Span (zero allocation)
        private static bool IsStaticExtension(ReadOnlySpan<char> extension)
        {
            // Most common extensions first for early exit
            return extension.Equals(".css", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".js", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".gif", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".svg", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".ico", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".woff", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".woff2", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".ttf", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".eot", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".map", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".json", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".xml", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".txt", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".zip", StringComparison.OrdinalIgnoreCase);
        }

        // Optimized prefix check (most common paths first)
        private static bool IsSpecialPathPrefix(string path)
        {
            // Most common paths first for early exit
            return path.StartsWith("/data/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/fonts/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/_framework/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/_vs/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/swagger/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/cache/", StringComparison.OrdinalIgnoreCase) ||
                   path == "/robots.txt" ||
                   path == "/sitemap.xml" ||
                   path == "/favicon.ico";
        }
    }

    public class CustomRouter : IRouter
    {
        private readonly IRouter _defaultRouter;
        // khai báo baseSetting
        public CustomRouter(IRouter defaultRouteHandler)
        {
            _defaultRouter = defaultRouteHandler;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return _defaultRouter.GetVirtualPath(context);
        }

        public async Task RouteAsync(RouteContext context)
        {
            string path = context.HttpContext.Request.Path.Value.ToString().ToLower();
            if(path.EndsWith("/robots.txt") || path.EndsWith("sitemap.xml") || path.StartsWith("/Cache/Refresh"))
            {
                await _defaultRouter.RouteAsync(context);
                return;
            }
            else if (path.ToLower().EndsWith(".html") || path.ToLower() == "" || path.ToLower() == "/" || ListData.ListLanguage.Any(x => $"/{x.Id}" == path || $"/{x.Id}/" == path))
            {
                
                var baseSettings = (IOptions<BaseSettings>)AppHttpContext.Current.RequestServices.GetService(typeof(IOptions<BaseSettings>));
                string language = baseSettings.Value.DefaultLanguage;
                var _iLinkRepository = (ILinkRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkRepository));
                string slug = path.EndsWith(".html") ? path[0..^5] : path;
                if (slug.Length == 3)
                {
                    language = baseSettings.Value.MultipleLanguage ? slug.Substring(1, 2) : language;
                    slug = "";
                }
                else if (slug.Length > 3)
                {
                    language = baseSettings.Value.MultipleLanguage ? slug.Substring(1, 2) : language;
                    slug = baseSettings.Value.MultipleLanguage ? slug.Replace($"/{language}/", "") : slug.Substring(1);
                }
                else if (slug == "/")
                {
                    slug = "";
                }
                // Custom thêm ở đây
                // Dictionary mapping slug prefixes to actions
                var customRoutes = new Dictionary<string, string>
                {
                    { "tin-tuc-fg", "FGNews" },
                    { "news-fg", "FGNews" },
                    { "su-kien-fg", "FGEvent" },
                    { "event-fg", "FGEvent" },
                    { "ratings-fr", "FRatings" },
                };

                // Check custom routes
                foreach (var route in customRoutes)
                {
                    if (!string.IsNullOrEmpty(slug) && slug.StartsWith(route.Key))
                    {
                        // Pattern: {prefix}-{slug}-id{id}
                        var match = Regex.Match(slug, $@"^{route.Key}.*-id(\d+)$", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            int contentId = int.Parse(match.Groups[1].Value);
                            context.RouteData.Values["controller"] = "ContentPage";
                            context.RouteData.Values["action"] = route.Value;
                            context.RouteData.Values["id"] = contentId;
                            context.RouteData.Values["language"] = language;
                            context.RouteData.Values["portalId"] = baseSettings.Value.PortalId;

                            context.RouteData.Values["linkData"] = Newtonsoft.Json.JsonConvert.SerializeObject(new Link
                            {
                                Slug = slug,
                                Language = language,
                                PortalId = baseSettings.Value.PortalId,
                                Controller = "ContentPage",
                                Acction = route.Value,
                                ObjectId = contentId,
                            });
                            await _defaultRouter.RouteAsync(context);
                            return;
                        }
                    }
                }

                // Dùng cache để lưu Link object Link theo key là slug và language, nếu null thì query từ database
                //var cache = (IMemoryCache)AppHttpContext.Current.RequestServices.GetService(typeof(IMemoryCache));
                //var cacheKey = $"Link_{baseSettings.Value.PortalId}_{slug}_{language}";
                //if (!cache.TryGetValue(cacheKey, out Link link))
                //{
                //    link = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.Slug == slug && x.Language == language && x.PortalId == baseSettings.Value.PortalId);
                //}
               var link = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.Slug == slug && x.Language == language && x.PortalId == baseSettings.Value.PortalId);
                if (link != null)
                {
                    // ✅ SỬA LỖI: Thêm Size property khi set cache (vì MemoryCache có SizeLimit)
                    // Mỗi Link object ước tính ~1KB, set Size = 1 unit
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                        .SetSize(1); // 1 unit = 1KB (theo quy ước trong Startup.cs)
                    
                    //cache.Set(cacheKey, link, cacheOptions);
                    
                    if (link.Title == null || link.Title == "")
                    {
                        link.Title = link.Name;
                    }
                    // Trạng thái xóa kết hợp 301 != null tức là điều hướng đi trang khác, ngược lại tức là link này bị xóa vĩnh viễn sẽ tả về 404
                    if (link.Delete || !link.Status)
                    {
                        if (!string.IsNullOrEmpty(link.Redirect301))
                        {
                            context.RouteData.Values["controller"] = "Home";
                            context.RouteData.Values["action"] = "Page301";
                            context.RouteData.Values["url"] = link.Redirect301;
                        }
                        else
                        {
                            context.RouteData.Values["controller"] = "Home";
                            context.RouteData.Values["action"] = "Page404";
                            await _defaultRouter.RouteAsync(context);
                        }
                    }
                    else
                    {
                       
                        if (!string.IsNullOrEmpty(link.Redirect301))
                        {
                            context.RouteData.Values["controller"] = "Home";
                            context.RouteData.Values["action"] = "Page301";
                            context.RouteData.Values["url"] = link.Redirect301;
                        }
                        else
                        {
                            context.RouteData.Values["controller"] = link.Controller;
                            context.RouteData.Values["action"] = link.Acction;
                            context.RouteData.Values["language"] = link.Language;
                            context.RouteData.Values["id"] = link.ObjectId;
                            context.RouteData.Values["portalId"] = baseSettings.Value.PortalId;
                            context.RouteData.Values["parrams"] = link.Parrams;
                            context.RouteData.Values["linkData"] = Newtonsoft.Json.JsonConvert.SerializeObject(link);
                        }
                    }
                }
                else
                {
                    if (!ListData.ListLanguage.Any(x => x.Id == language))
                    {
                        language = baseSettings.Value.DefaultLanguage;
                    }
                    context.RouteData.Values["controller"] = "Home";
                    context.RouteData.Values["action"] = "Page404";
                    context.RouteData.Values["language"] = language;
                }
                await _defaultRouter.RouteAsync(context);
            }
            else
            {
                await _defaultRouter.RouteAsync(context);
            }    
        }
    }
}
