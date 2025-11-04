using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PT.Base;
using PT.Base.Services;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;

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

        [HttpPost("Refresh/{key}")]
        public IActionResult Refresh(string key)
        {
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
    }
}