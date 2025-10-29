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
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;

namespace PT.UI.Controllers
{
    public class ProductController : Controller
    {
        private readonly IOptions<List<SeoSettings>> _seoSettings;
        private readonly ILinkRepository _iLinkRepository;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ILinkReferenceRepository _iLinkReferenceRepository;
        public ProductController(IOptions<List<SeoSettings>> seoSettings, ILinkRepository iLinkRepository, IOptions<BaseSettings> baseSettings, IWebHostEnvironment iHostingEnvironment, IContentPageRepository iContentPageRepository, ILinkReferenceRepository iLinkReferenceRepository)
        {
            _seoSettings = seoSettings;
            _iLinkRepository = iLinkRepository;
            _baseSettings = baseSettings;
            _iHostingEnvironment = iHostingEnvironment;
            _iContentPageRepository = iContentPageRepository;
            _iLinkReferenceRepository = iLinkReferenceRepository;
        }

        public IActionResult Index(string linkData, int portalId)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            return View();
        }
    }
}