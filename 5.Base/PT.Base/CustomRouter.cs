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
        private static  string _defaultCulture = "vi";
        public UrlRequestCultureProvider(string defaultCulture = null)
        {
            _defaultCulture = defaultCulture;
        }

       
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }
            var path = httpContext.Request.Path.ToString().ToLower();
            if (path.StartsWith("/admin"))
            {
                return Task.FromResult(new ProviderCultureResult("vi"));
            }   
            else if(path == "/" || path == "")
            {
                return Task.FromResult(new ProviderCultureResult(_defaultCulture));
            }    
            else
            {
                var pathSegments = httpContext.Request.Path.Value.Split('/');
                if (pathSegments.Length <= 1)
                {
                    return Task.FromResult(new ProviderCultureResult(_defaultCulture));
                }
                else
                {
                    string checkSegment = pathSegments[1].ToLower();
                    if (string.IsNullOrEmpty(checkSegment))
                    {
                        return Task.FromResult(new ProviderCultureResult(_defaultCulture));
                    }
                    else
                    {
                        return Task.FromResult(new ProviderCultureResult(_defaultCulture));
                    }
                }    
            }    
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
            if (path.ToLower().EndsWith(".html") || path.ToLower() == "" ||  path.ToLower() == "/" || ListData.ListLanguage.Any(x=> $"/{x.Id}" == path || $"/{x.Id}/" == path))
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
                // Dùng cache để lưu Link object Link theo key là slug và language, nếu null thì query từ database
                var cache = (IMemoryCache)AppHttpContext.Current.RequestServices.GetService(typeof(IMemoryCache));
                var cacheKey = $"Link_{slug}_{language}";

                if (!cache.TryGetValue(cacheKey, out Link link))
                {
                    link = await _iLinkRepository.SingleOrDefaultAsync(true, x=>x.Slug == slug && x.Language == language);
                }

                if (link != null)
                {
                    // Lưu 6 tiếng
                    cache.Set(cacheKey, link, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60 * 6)));
                    context.RouteData.Values["controller"] = link.Controller;
                    context.RouteData.Values["action"] = link.Acction;
                    context.RouteData.Values["language"] = link.Language;
                    context.RouteData.Values["id"] = link.ObjectId;
                    context.RouteData.Values["parrams"] = link.Parrams;
                    context.RouteData.Values["linkData"] = Newtonsoft.Json.JsonConvert.SerializeObject(link);
                }
                else
                {
                    if (!ListData.ListLanguage.Any(x => x.Id == language))
                    {
                        language = baseSettings.Value.DefaultLanguage;
                    }
                    context.RouteData.Values["controller"] = "Home";
                    context.RouteData.Values["action"] = "Page404";
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
