using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using PT.Domain.Model;
using PT.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PT.Infrastructure.Interfaces
{
    public interface IPortalRepository : IGenericRepository<Portal>
    {
        Task<string> GetFullPathAsync(int portalId, string slug, string language = null, bool multipleLanguage = false);
        Task<string> GetFullPathAsync(int portalId, string slug, IEnumerable<Portal> portals, string language = null, bool multipleLanguage = false);
        // Trigger remote cache refresh by calling the portal's configured RefeshCacheAPI endpoint.
        Task<bool> TriggerRemoteCacheRefreshAsync(int portalId, ModuleType type, string code, string language);
        Task<bool> TriggerRemoteCacheRefreshAsync(Portal portal, ModuleType type, string code, string language);
        Task<bool> TriggerRemoteCacheRefreshByKeyAsync(int portalId, string code);
    }
    public class PortalRepository : BaseRepository<Portal>, IPortalRepository
    {
        private readonly ApplicationContext _context;
        private readonly IWebHostEnvironment _env;
        public PortalRepository(ApplicationContext context, IWebHostEnvironment env) : base(context)
        {
            _context = context;
            _env = env;
        }

        public async Task<string> GetFullPathAsync(int portalId, string slug, string language = null, bool multipleLanguage = false)
        {
            var portal = await _context.Portals.FindAsync(portalId);
            var domain = portal?.Domain?.TrimEnd('/') ?? string.Empty;
            var cleanedSlug = (slug ?? string.Empty).TrimStart('/');
            if (multipleLanguage && !string.IsNullOrWhiteSpace(language))
            {
                return $"{domain}/{language}/{cleanedSlug}.html";
            }
            return $"{domain}/{cleanedSlug}.html";
        }

        public Task<string> GetFullPathAsync(int portalId, string slug, IEnumerable<Portal> portals, string language = null, bool multipleLanguage = false)
        {
            var portal = portals?.FirstOrDefault(p => p.Id == portalId);

            var domain = portal?.Domain?.TrimEnd('/') ?? string.Empty;
            if(!_env.IsProduction())
            {
                domain = portal?.DomainDev?.TrimEnd('/') ?? string.Empty;
            }    
            var cleanedSlug = (slug ?? string.Empty).TrimStart('/');
            string rtUrl = null;
            // Nếu slug là null/rỗng hoặc là "vi" hoặc "en" thì không thêm .html
            bool isLangRoot = string.IsNullOrWhiteSpace(cleanedSlug) || cleanedSlug.Equals("vi", StringComparison.OrdinalIgnoreCase) || cleanedSlug.Equals("en", StringComparison.OrdinalIgnoreCase);

            if (multipleLanguage)
            {
                if (isLangRoot)
                {
                    rtUrl = $"{domain}/{language}";
                }
                else
                {
                    rtUrl = $"{domain}/{language}/{cleanedSlug}.html";
                }
            }
            else
            {
                if (isLangRoot)
                {
                    rtUrl = $"{domain}/{cleanedSlug}";
                }
                else
                {
                    rtUrl = $"{domain}/{cleanedSlug}.html";
                }
            }
            return Task.FromResult(rtUrl);
        }

        /// <summary>
        /// Call the portal's RefeshCacheAPI/Refresh/{key} endpoint to notify FE to refresh cache.
        /// Returns true when the remote call returns a successful HTTP status code.
        /// </summary>
        public async Task<bool> TriggerRemoteCacheRefreshAsync(int portalId, ModuleType type, string code, string language)
        {
            var portal = await _context.Portals.FindAsync(portalId);
            return await TriggerRemoteCacheRefreshAsync(portal, type, code,language);
        }

        public async Task<bool> TriggerRemoteCacheRefreshAsync(Portal portal, ModuleType type, string code, string language)
        {
            try
            {
                if (portal == null) return false;

                // Build cache key same format used elsewhere
                string cacheKey = $"ModuleHtml::{type}::{code}::{language}::{portal.Id}";

                // URL-encode the cache key to safely include in URL
                var encodedCacheKey = Uri.EscapeDataString(cacheKey);
                // HTML-encode if the key is ever rendered into HTML
                var htmlEncodedCacheKey = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(cacheKey);

                // Choose domain based on environment: production uses portal.Domain, otherwise portal.DomainDev (fallback to Domain)
                var domain = _env.IsProduction()
                    ? (portal.Domain ?? string.Empty)
                    : (!string.IsNullOrWhiteSpace(portal.DomainDev) ? portal.DomainDev : portal.Domain ?? string.Empty);

                domain = domain.TrimEnd('/');

                var apiPath = (portal.RefeshCacheAPI ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(apiPath)) return false;
                if (!apiPath.StartsWith("/")) apiPath = "/" + apiPath;

                // Use configured domain + API path (no extra Refresh segment)
                // insert URL-encoded cache key to avoid invalid URL characters
                var url = $"{domain}{apiPath}?key={encodedCacheKey}";
                using var http = new HttpClient();
                http.Timeout = TimeSpan.FromSeconds(10);
                var response = await http.PostAsync(url, null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TriggerRemoteCacheRefreshByKeyAsync(int portalId, string code)
        {
            var portal = await _context.Portals.FindAsync(portalId);
            return await TriggerRemoteCacheRefreshByKeyAsync(portal,  code);
        }

        public async Task<bool> TriggerRemoteCacheRefreshByKeyAsync(Portal portal, string key)
        {
            try
            {
                if (portal == null || string.IsNullOrWhiteSpace(key)) return false;

                // URL-encode the cache key để tránh ký tự lạ trong URL
                var encodedCacheKey = Uri.EscapeDataString(key);

                // Chọn domain phù hợp với môi trường
                var domain = _env.IsProduction()
                    ? (portal.Domain ?? string.Empty)
                    : (!string.IsNullOrWhiteSpace(portal.DomainDev) ? portal.DomainDev : portal.Domain ?? string.Empty);

                domain = domain.TrimEnd('/');

                var apiPath = (portal.RefeshCacheAPI ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(apiPath)) return false;
                if (!apiPath.StartsWith("/")) apiPath = "/" + apiPath;

                var url = $"{domain}{apiPath}?key={encodedCacheKey}";
                using var http = new HttpClient();
                http.Timeout = TimeSpan.FromSeconds(10);
                var response = await http.PostAsync(url, null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
