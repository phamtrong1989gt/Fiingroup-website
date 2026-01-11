using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PT.Base;
using PT.Base.Services;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;
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
using static System.Runtime.InteropServices.JavaScript.JSType;
// ⭐ THÊM USING CHO IMAGESHARP
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using Microsoft.Extensions.Logging;

namespace PT.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILinkRepository _iLinkRepository;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ILinkReferenceRepository _iLinkReferenceRepository;
        private readonly ISettingService _iSettingService;
        private readonly IOptions<BaseSettings> _baseSetting;
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILinkRepository iLinkRepository, IOptions<BaseSettings> baseSettings, IWebHostEnvironment iHostingEnvironment, IContentPageRepository iContentPageRepository, ILinkReferenceRepository iLinkReferenceRepository, ISettingService iSettingService, IOptions<BaseSettings> baseSetting, ILogger<HomeController> logger)
        {
            _iLinkRepository = iLinkRepository;
            _baseSettings = baseSettings;
            _iHostingEnvironment = iHostingEnvironment;
            _iContentPageRepository = iContentPageRepository;
            _iLinkReferenceRepository = iLinkReferenceRepository;
            _iSettingService = iSettingService;
            _baseSetting = baseSetting;
            _logger = logger;
        }


        [HttpGet]
        public async Task<object> Search(string key, string language)
        {
            key = key?.ToLower();
            if(key == null || key.Length < 3)
            {
                return View(new List<object>());
            }
            var datas = await _iLinkRepository.SearchAsync(true, 0, 20, x => 
            (x.Type == ESlugType.ContentPage || x.Type == ESlugType.Static || x.Type == ESlugType.Category) &&
            (x.Name.ToLower().Contains(key) || x.Keywords.ToLower().Contains(key) || x.Description.ToLower().Contains(key)) && x.Status && !x.Delete && x.PortalId == _baseSetting.Value.PortalId && (x.Language == language || x.Language == "All"), x=>x.OrderBy(m=>m.Name));
            var newList = datas.Select(x => new
            {
                x.Id,
                x.Name,
                x.Slug,
                x.Description,
                x.Language,
                x.Keywords,
                Href=  $"{( _baseSetting.Value.MultipleLanguage ? "/" + x.Language : "")}/{(string.IsNullOrEmpty(x.Slug) ? "" : x.Slug + ".html")}"
            }).ToList();
            return newList;
        }

        public IActionResult Error(int? statusCode = null)
        {
            if (statusCode == null)
                statusCode = HttpContext.Response.StatusCode;
            if (statusCode == 404)
            {
                return View("_Home404");
            }
            return Redirect($"/");
        }

        public async Task<IActionResult> Index(string linkData, int portalId)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            return View();
        }

        public IActionResult Page404(string linkData)
        {
            if (linkData != null)
            {
                ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            }
            return View("_Home404");
        }

        public IActionResult Page301(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return RedirectToAction("/");
            }

            // If url is relative (no scheme/host) allow (normalize leading slash)
            if (!Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
            {
                if (!url.StartsWith("/"))
                {
                    url = "/" + url;
                }
                return RedirectPermanent(url);
            }

            // At this point we have an absolute URI.
            // Allow redirects if:
            //  - same host as current site OR
            //  - host is in the configured whitelist (AllowedRedirectDomains)
            var currentHost = $"{Request.Scheme}://{Request.Host.Value}";
            if (absoluteUri.AbsoluteUri.StartsWith(currentHost, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectPermanent(absoluteUri.AbsoluteUri);
            }

            // Read whitelist from configuration key "AllowedRedirectDomains"
            // Format: "fiinratings.vn,other-domain.com"
            try
            {
                var allowedListRaw = "fiinratings.vn";
                var allowedDomains = allowedListRaw
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(d => d.Trim().ToLowerInvariant())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();

                var host = absoluteUri.Host?.ToLowerInvariant() ?? "";

                bool isAllowedExternal = allowedDomains.Length > 0 &&
                    allowedDomains.Any(d => host == d || host.EndsWith("." + d, StringComparison.OrdinalIgnoreCase));

                if (isAllowedExternal)
                {
                    return RedirectPermanent(absoluteUri.AbsoluteUri);
                }
                else
                {
                    return RedirectPermanent(absoluteUri.AbsoluteUri);
                }    
            }
            catch
            {
                // If any error reading configuration, fall through to deny external redirect.
            }

            // Not allowed to redirect outside domain
            return RedirectToAction("Page404");
        }

        // ✅ TRANG GIỚI THIỆU - Cache 1 giờ (static content)
        [OutputCache(PolicyName = "StaticPage")]
        public IActionResult About(string linkData)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            return View();
        }

        // ✅ TRANG TÌM KIẾM - Cache 30 giây (dynamic content)
        // Vary theo keyword (k) và page number
        [OutputCache(PolicyName = "SearchPage")]
        public async Task<IActionResult> Search(string language, string k, int? page, string linkData)
        {
            var objectLink = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            objectLink.Title = string.IsNullOrEmpty(objectLink.Title) ? objectLink.Name : objectLink.Title;
            objectLink.Title = $"{objectLink.Title}{((page == null || page == 1) ? "" : (language == "vi" ? " - trang" : " - page"))} {page}";
            ViewData["linkData"] = objectLink;


            var data = await _iContentPageRepository.SearchPagedListAsync(
                     page ?? 1,
                     10,
                     null,
                     null,
                     m => (m.Name.Contains(k) || m.Content.Contains(k) || m.Summary.Contains(k))
                         //&& m.Type == CategoryType.Blog
                         && (m.Language == language)
                         && m.Status
                         , x => x.OrderByDescending(mbox => mbox.DatePosted), x => new ContentPage
                         {
                             Category = x.Category,
                             Id = x.Id,
                             Author = x.Author,
                             Banner = x.Banner,
                             DatePosted = x.DatePosted,
                             Name = x.Name,
                             Language = x.Language,
                             Status = x.Status,
                             Summary = x.Summary,
                             Tags = x.Tags,
                             Type = x.Type,
                             Link = x.Link
                         });

            int totalPage = (data.TotalRows % data.Limit > 0) ? (data.TotalRows / data.Limit + 1) : (data.TotalRows / data.Limit);
            if (totalPage >= 2)
            {
                page = page ?? 1;
                if (page < totalPage)
                {
                    ViewData["linkNext"] = $"{Request.Path}?page={page + 1}&k={k}";
                }
                if (page >= totalPage)
                {
                    ViewData["linkPrev"] = $"{Request.Path}?page={page - 1}&k={k}";
                }
            }

            return View(data);
        }

        public async Task<IActionResult> ChangeLanguage(string language = "vi", int linkId = 0)
        {
            var url = await _iLinkReferenceRepository.GetLink(language, linkId);
            if (url == null)
            {
                return LocalRedirect($"/{language}");
            }
            else
            {
                return LocalRedirect(url);
            }
        }

        // ✅ POST REQUEST - Tự động BỎ QUA cache (không cần config gì)
        [HttpPost]
        [Route("admin/AdminView")]
        [Authorize]
        public void AdminView(int status)
        {
            var option = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(1)
            };
            Response.Cookies.Delete("AdminView");
            Response.Cookies.Append("AdminView", status.ToString(), option);
        }

        // ✅ ROBOTS.TXT - Cache 1 giờ (ít thay đổi)
        [Route("robots.txt")]
        [Route("{language}/robots.txt")]
        [OutputCache(PolicyName = "StaticPage")]
        public async Task<FileResult> Robots(string language = "vi")
        {
            return await GetFileRobots(language);
        }

        // ✅ SITEMAP.XML - Cache 1 giờ (ít thay đổi)
        [Route("sitemap.xml")]
        [OutputCache(PolicyName = "StaticPage")]
        public Task<FileStreamResult> SitemapAll(string language = "vi")
        {
            return GetFileSitemap(language);
        }

        private async Task<FileStreamResult> GetFileRobots(string language)
        {
            string str = "";
            var dl = await _iSettingService.SeoSettingGet(language, _baseSettings.Value.PortalId);
            if (dl != null && !string.IsNullOrEmpty(dl.Robots))
            {
                str = dl.Robots;
            }
            else
            {
                str = "# Robots.txt configuration not found\nUser-agent: *\nDisallow:";
            }
            var ms = new MemoryStream(Encoding.ASCII.GetBytes(str));
            return new FileStreamResult(ms, "text/plain");
        }

        private async Task<FileStreamResult> GetFileSitemap(string language, string domain = null)
        {
            try
            {
                bool IsMuti = _baseSettings.Value.MultipleLanguage;
                string Domain = $"{AppHttpContext.Current.Request.Scheme}://{Request.Host}";
                if (!string.IsNullOrEmpty(domain))
                {
                    Domain = $"{AppHttpContext.Current.Request.Scheme}://{domain}";
                }
                var stringBuilder = new StringBuilder();

                var listItem = await _iLinkRepository.SearchAsync(true, 0, 0, x => (x.Language == language || language == "") && x.Status && x.IncludeSitemap && x.PortalId == _baseSettings.Value.PortalId);
                stringBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                stringBuilder.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd\">");

                foreach (var item in listItem)
                {
                    stringBuilder.AppendLine("<url>");
                    if (string.IsNullOrEmpty(item.Slug))
                    {
                        stringBuilder.AppendLine("<loc>" + (IsMuti ? string.Format("{0}/{1}", Domain, item.Language, item.Slug) : string.Format("{0}/{1}", Domain, item.Slug)) + "</loc>");
                    }
                    else
                    {
                        stringBuilder.AppendLine("<loc>" + (IsMuti ? string.Format("{0}/{1}/{2}.html", Domain, item.Language, item.Slug) : string.Format("{0}/{1}.html", Domain, item.Slug)) + "</loc>");
                    }

                    stringBuilder.AppendLine($"<lastmod>{item.Lastmod:yyyy-MM-ddThh:mm:ss+00:00}</lastmod>");
                    stringBuilder.AppendLine("<changefreq>" + item.Changefreq + "</changefreq>");
                    stringBuilder.AppendLine("<priority>" + item.Priority.ConvertToString() + "</priority>");
                    stringBuilder.AppendLine("</url>");
                }

                stringBuilder.AppendLine("</urlset>");

                var ms = new MemoryStream(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
                return new FileStreamResult(ms, "text/xml");
            }
            catch
            {
                var ms1 = new MemoryStream(Encoding.ASCII.GetBytes(""));
                return new FileStreamResult(ms1, "text/xml");
            }
        }

        [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
        [Route("data/image")]
        public IActionResult Image(string path, int size, bool? s)
        {
            try
            {
                path = $"{_baseSetting.Value.DataPath}\\{path}";
                var image = System.Drawing.Image.FromFile(path);
                var a = ResizeImage(Path.GetFileName(path), image, size, s ?? false);
                return File(CopyImageToByteArray(a, GetImageFormat(a)), "image/jpeg");
            }
            catch
            {
                return Content(" ");
            }
        }

        private Bitmap ResizeImage(string fileName, System.Drawing.Image image, int maxSideSize, bool makeItSquare)
        {
            int newWidth;
            int newHeight;

            int oldWidth = image.Width;
            int oldHeight = image.Height;
            Bitmap newImage;
            if (makeItSquare)
            {
                int smallerSide = oldWidth >= oldHeight ? oldHeight : oldWidth;
                double coeficient = maxSideSize / (double)smallerSide;
                newWidth = Convert.ToInt32(coeficient * oldWidth);
                newHeight = Convert.ToInt32(coeficient * oldHeight);
                Bitmap tempImage = new Bitmap(image, newWidth, newHeight);
                int cropX = (newWidth - maxSideSize) / 2;
                int cropY = (newHeight - maxSideSize) / 2;
                newImage = new Bitmap(maxSideSize, maxSideSize);
                Graphics tempGraphic = Graphics.FromImage(newImage);
                tempGraphic.SmoothingMode = SmoothingMode.AntiAlias;
                tempGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                tempGraphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                tempGraphic.DrawImage(tempImage, new System.Drawing.Rectangle(0, 0, maxSideSize, maxSideSize), cropX, cropY, maxSideSize, maxSideSize, GraphicsUnit.Pixel);
            }
            else
            {
                int maxSide = oldWidth >= oldHeight ? oldWidth : oldHeight;

                if (maxSide > maxSideSize)
                {
                    double coeficient = maxSideSize / (double)maxSide;
                    newWidth = Convert.ToInt32(coeficient * oldWidth);
                    newHeight = Convert.ToInt32(coeficient * oldHeight);
                }
                else
                {
                    newWidth = oldWidth;
                    newHeight = oldHeight;
                }
                newImage = new Bitmap(image, newWidth, newHeight);
            }
            return newImage;
        }

        public ImageFormat GetImageFormat(System.Drawing.Image img)
        {
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
                return System.Drawing.Imaging.ImageFormat.Png;
            else if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif))
                return System.Drawing.Imaging.ImageFormat.Gif;
            else if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Icon))
                return ImageFormat.Icon;
            else
                return System.Drawing.Imaging.ImageFormat.Jpeg;
        }

        private byte[] CopyImageToByteArray(System.Drawing.Image theImage, ImageFormat type)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                theImage.Save(memoryStream, type);
                return memoryStream.ToArray();
            }
        }

        // ✅ POST REQUEST - Tự động BỎ QUA cache
        [HttpPost]
        public void ChangePriceType(string type = "VND")
        {
            CookieExtensions.Set("CurrentPrice", type);
        }

        // ✅ BANNER HOMEPAGE - Có thể cache nếu muốn (tuỳ bạn)
        // Nếu banner thay đổi thường xuyên thì comment [OutputCache] đi
        [HttpGet]
        [Route("data/BannerHomePage")]
        [OutputCache(PolicyName = "HomePage")]
        public IActionResult BannerHomePage(string language)
        {
            return View("BannerHomePage", language);
        }
        
        // ⭐ HÀM MỚI: Convert ảnh sang WebP với resize
        // ⭐ FIX: VaryByQueryKeys để cache riêng biệt cho mỗi combination của params
        [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "path", "size", "s", "quality" })]
        [Route("data/image-webp")]
        [Route("data2/image-webp")]
        public IActionResult ImageWebP(string path, int? size, bool? s, int? quality)
        {
            try
            {
                // ⭐ FIX: Loại bỏ "/data" ở đầu path nếu có (tránh duplicate)
                if (path.StartsWith("/data/", StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring(6); // Remove "/data/"
                }
                else if (path.StartsWith("data/", StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring(5); // Remove "data/"
                }

                // Xây dựng đường dẫn đầy đủ
                string fullPath = Path.Combine(_baseSetting.Value.DataPath, path.Replace('/', Path.DirectorySeparatorChar));
                
                if (!System.IO.File.Exists(fullPath))
                {
                    _logger.LogWarning("ImageWebP: Image not found at {FullPath}", fullPath);
                    return NotFound($"Image not found: {path}");
                }

                // Load ảnh bằng ImageSharp (cross-platform)
                using var image = SixLabors.ImageSharp.Image.Load(fullPath);

                // Resize nếu có size parameter
                if (size.HasValue && size.Value > 0)
                {
                    image.Mutate(x =>
                    {
                        if (s == true) // Square mode - crop và resize
                        {
                            int smallerSide = Math.Min(image.Width, image.Height);
                            int cropX = (image.Width - smallerSide) / 2;
                            int cropY = (image.Height - smallerSide) / 2;

                            // Crop thành hình vuông
                            x.Crop(new SixLabors.ImageSharp.Rectangle(cropX, cropY, smallerSide, smallerSide));
                            
                            // Resize về kích thước mong muốn
                            x.Resize(size.Value, size.Value);
                        }
                        else // Resize giữ tỷ lệ
                        {
                            var options = new ResizeOptions
                            {
                                Size = new SixLabors.ImageSharp.Size(size.Value, size.Value),
                                Mode = ResizeMode.Max, // Giữ tỷ lệ, fit trong size
                                Sampler = KnownResamplers.Lanczos3 // High quality resampling
                            };
                            x.Resize(options);
                        }
                    });
                }

                // Cấu hình WebP encoder
                var encoder = new WebpEncoder
                {
                    Quality = quality ?? 80, // Default quality 80 (0-100)
                    FileFormat = WebpFileFormatType.Lossy, // Lossy compression (nhỏ hơn)
                    Method = WebpEncodingMethod.BestQuality // Chất lượng tốt nhất
                };

                // Convert sang WebP và trả về
                var ms = new MemoryStream();
                image.SaveAsWebp(ms, encoder);
                ms.Position = 0;

                return File(ms, "image/webp");
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, "ImageWebP: Error processing image {Path} with size={Size}, s={S}, quality={Quality}", 
                    path, size, s, quality);
                return StatusCode(500, $"Error processing image: {ex.Message}");
            }
        }

        // ⭐ HÀM AUTO-DETECT: Trả về WebP nếu browser hỗ trợ, ngược lại JPEG
        [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
        [Route("data/image-smart")]
        public IActionResult ImageSmart(string path, int? size, bool? s, int? quality)
        {
            // Check Accept header của browser
            var acceptHeader = Request.Headers["Accept"].ToString();
            bool supportsWebP = acceptHeader.Contains("image/webp", StringComparison.OrdinalIgnoreCase);

            if (supportsWebP)
            {
                // Browser hỗ trợ WebP → trả về WebP (nhỏ hơn ~30%)
                return ImageWebP(path, size, s, quality);
            }
            else
            {
                // Browser cũ không hỗ trợ WebP → fallback về JPEG
                return Image(path, size ?? 0, s);
            }
        }
    }
}