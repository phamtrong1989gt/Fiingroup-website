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
    public class HomeController : Controller
    {
        private readonly IOptions<List<SeoSettings>> _seoSettings;
        private readonly ILinkRepository _iLinkRepository;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ILinkReferenceRepository _iLinkReferenceRepository;
        public HomeController(IOptions<List<SeoSettings>> seoSettings, ILinkRepository iLinkRepository, IOptions<BaseSettings> baseSettings, IWebHostEnvironment iHostingEnvironment, IContentPageRepository iContentPageRepository, ILinkReferenceRepository iLinkReferenceRepository)
        {
            _seoSettings = seoSettings;
            _iLinkRepository = iLinkRepository;
            _baseSettings = baseSettings;
            _iHostingEnvironment = iHostingEnvironment;
            _iContentPageRepository = iContentPageRepository;
            _iLinkReferenceRepository = iLinkReferenceRepository;
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

        public IActionResult Index(string linkData)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            return View();
        }
        public IActionResult Page404(string linkData)
        {
            if(linkData!= null)
            {
                ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            }    
            return View("_Home404");
        }
        public IActionResult About(string linkData)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            return View();
        }

        public IActionResult FAQ(string linkData)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            return View();
        }

        public IActionResult Clinic1(string linkData)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            return View();
        }

        public IActionResult Clinic2( string linkData)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            return View();
        }

        public async Task<IActionResult> Search(string language, string k, int? page, string linkData)
        {
            var objectLink = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            objectLink.Title = string.IsNullOrEmpty(objectLink.Title) ? objectLink.Name : objectLink.Title;
            objectLink.Title = $"{ objectLink.Title }{ ((page == null || page == 1) ? "" : (language == "vi" ? " - trang" : " - page"))} {page}";
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
                         && !m.Delete, x => x.OrderByDescending(mbox => mbox.DatePosted), x => new ContentPage
                         {
                             Category = x.Category,
                             Id = x.Id,
                             Author = x.Author,
                             Banner = x.Banner,
                             DatePosted = x.DatePosted,
                             Delete = x.Delete,
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

        [Route("robots.txt")]
        [Route("{language}/robots.txt")]
        public FileResult Robots(string language = "vi")
        {
            return GetFileRobots(language);
        }

        [Route("sitemap.xml")]
        public Task<FileStreamResult> SitemapAll(string language = "vi")
        {
            return GetFileSitemap("");
        }

        private FileStreamResult GetFileRobots(string language)
        {
            string str = "";
            var dl = _seoSettings.Value.FirstOrDefault(m => m.Id == language);
            if (dl != null)
            {
                str = dl.Robots;
            }
            var ms = new MemoryStream(Encoding.ASCII.GetBytes(str));
            return new FileStreamResult(ms, "text/plain");
        }

        private async Task<FileStreamResult> GetFileSitemap(string language, string domain = null)
        {
            try
            {
                bool IsMuti = _baseSettings.Value.MultipleLanguage;
                string Domain = $"{ AppHttpContext.Current.Request.Scheme }://{Request.Host}";
                if(!string.IsNullOrEmpty(domain))
                {
                    Domain = $"{ AppHttpContext.Current.Request.Scheme }://{domain}";
                }
                var stringBuilder = new StringBuilder();

                var listItem = await _iLinkRepository.SearchAsync(true, 0, 0, x => (x.Language == language || language =="") && !x.Delete && x.Status && x.IncludeSitemap && x.Type != CategoryType.Employee);
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


        [Route("data/image")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 30000000)]
        public IActionResult Image(string path, int size, bool? s)
        {
            try
            {
                path = $"{_iHostingEnvironment.WebRootPath}\\{path}";
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
                tempGraphic.DrawImage(tempImage, new Rectangle(0, 0, maxSideSize, maxSideSize), cropX, cropY, maxSideSize, maxSideSize, GraphicsUnit.Pixel);
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
                return System.Drawing.Imaging.ImageFormat.Icon;
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

        [HttpPost]
        public void ChangePriceType(string type = "VND")
        {
            CookieExtensions.Set("CurrentPrice", type);
        }

        [HttpGet]
        [Route("data/BannerHomePage")]
        public IActionResult BannerHomePage(string language)
        {
            return View("BannerHomePage", language);
        }
    }
}