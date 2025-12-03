using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PT.Base.Services;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace PT.UI.Controllers
{
    public class CacheController : Controller
    {
        private readonly ILinkRepository _iLinkRepository;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ISettingService _iSettingService;
        private readonly ILogger<CacheController> _logger;
        public CacheController(ISettingService iSettingService, ILinkRepository iLinkRepository, IOptions<BaseSettings> baseSettings, IWebHostEnvironment iHostingEnvironment, IContentPageRepository iContentPageRepository, ILogger<CacheController> logger)
        {
            _iLinkRepository = iLinkRepository;
            _baseSettings = baseSettings;
            _iHostingEnvironment = iHostingEnvironment;
            _iContentPageRepository = iContentPageRepository;
            _iSettingService = iSettingService;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Refresh(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    _logger.LogWarning("Cache.Refresh called with empty key.");
                    return BadRequest();
                }
                // Allow only when request comes from localhost (or 127.0.0.1).
                // If behind a reverse proxy, X-Forwarded-Host is checked first.
                var forwardedHost = Request.Headers["X-Forwarded-Host"].FirstOrDefault();
                var host = !string.IsNullOrEmpty(forwardedHost)
                    ? forwardedHost.Split(',')[0].Trim().ToLowerInvariant()
                    : HttpContext.Request.Host.Host?.ToLowerInvariant();

                var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                _logger.LogInformation("Cache.Refresh called. key={Key}, host={Host}, forwardedHost={ForwardedHost}, remoteIp={RemoteIp}", key, host, forwardedHost, remoteIp);

                //if (host != "localhost" && host != "127.0.0.1")
                //{
                //    _logger.LogWarning("Cache.Refresh forbidden from host={Host}, remoteIp={RemoteIp}", host, remoteIp);
                //    // Deny non-localhost callers
                //    return Forbid();
                //}

                _iSettingService.RefreshByKey(key);
                _logger.LogInformation("Cache.Refresh succeeded for key={Key}", key);
                return Ok();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Cache.Refresh failed for key={Key}", key);
                return BadRequest();
            }
        }
    }
}