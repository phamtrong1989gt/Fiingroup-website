using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PT.Base.Services;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;

namespace PT.UI.Controllers
{
    public class CacheController : Controller
    {
        private readonly ILinkRepository _iLinkRepository;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ISettingService _iSettingService;
        public CacheController(ISettingService iSettingService, ILinkRepository iLinkRepository, IOptions<BaseSettings> baseSettings, IWebHostEnvironment iHostingEnvironment, IContentPageRepository iContentPageRepository)
        {
            _iLinkRepository = iLinkRepository;
            _baseSettings = baseSettings;
            _iHostingEnvironment = iHostingEnvironment;
            _iContentPageRepository = iContentPageRepository;
            _iSettingService = iSettingService;
        }

        [HttpPost]
        public IActionResult Refresh(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    return BadRequest();
                }
                // Allow only when request comes from localhost (or 127.0.0.1).
                // If behind a reverse proxy, X-Forwarded-Host is checked first.
                var forwardedHost = Request.Headers["X-Forwarded-Host"].FirstOrDefault();
                var host = !string.IsNullOrEmpty(forwardedHost)
                    ? forwardedHost.Split(',')[0].Trim().ToLowerInvariant()
                    : HttpContext.Request.Host.Host?.ToLowerInvariant();

                if (host != "localhost" && host != "127.0.0.1")
                {
                    // Deny non-localhost callers
                    return Forbid();
                }

                _iSettingService.RefreshByKey(key);
                return Ok();
            }
            catch (System.Exception)
            {
                return BadRequest();
            }
        }
    }
}