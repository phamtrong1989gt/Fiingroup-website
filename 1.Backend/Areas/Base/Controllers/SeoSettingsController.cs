using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.BE.Areas.Base.Controllers;
using PT.Shared;
using PT.Base;

namespace PT.BE.Areas.Setting.Controllers
{
    [Area("Base")]
    [AuthorizePermission("Index")]
    public class SeoSettingsController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IFileRepository _iFileRepository;
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly IOptions<List<SeoSettings>> _seoSettings;
        private readonly ILinkRepository _iLinkRepository;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly IEmployeeRepository _iEmployeeRepository;
        public SeoSettingsController(
            ILogger<SeoSettingsController> logger, 
            IFileRepository iFileRepository,
            IWebHostEnvironment iHostingEnvironment, 
            IOptions<List<SeoSettings>> seoSettings,
            ILinkRepository iLinkRepository,
            IOptions<BaseSettings> baseSettings,
            IContentPageRepository iContentPageRepository,
            ICategoryRepository iCategoryRepository,
            IEmployeeRepository  iEmployeeRepository
            )
        {
            controllerName = "SeoSettings";
            tableName = "Base";

            _logger = logger;
            _iFileRepository = iFileRepository;
            _iHostingEnvironment = iHostingEnvironment;
            _seoSettings = seoSettings;
            _iLinkRepository = iLinkRepository;
            _baseSettings = baseSettings;
            _iContentPageRepository = iContentPageRepository;
            _iCategoryRepository = iCategoryRepository;
            _iEmployeeRepository = iEmployeeRepository;
        }
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Index()
        {
            return View();
        }
        #region [Seo]
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Seo(string language = "vi")
        {
            var dl = _seoSettings.Value.FirstOrDefault(x => x.Id == language);
            return View(dl);
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Seo")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> SeoPost(SeoSettings model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var data = _seoSettings.Value.FirstOrDefault(x => x.Id == model.Id);
                    if (data == null)
                    {
                        data = new SeoSettings
                        {
                            Id = model.Id
                        };
                        _seoSettings.Value.Add(data);
                    }
                    data.Keywords = model.Keywords;
                    data.Lastmod = model.Lastmod;
                    data.MetaGoogle = model.MetaGoogle;
                    data.Robots = model.Robots;
                    data.Title = model.Title;
                    data.Analytics = model.Analytics;
                    data.AppSubport = model.AppSubport;
                    data.Changefreq = model.Changefreq;
                  
                    data.Description = model.Description;
                    data.FacebookPixelID = model.FacebookPixelID;
                    _iFileRepository.SettingsUpdate(_iHostingEnvironment.ContentRootPath + "/appsettings.Seo.json", new { SeoSettings = _seoSettings.Value });

                    await AddLog(new LogModel { Name = $"Cập nhật cấu thông tin seo {model.Id}.", Type = LogType.Edit });

                    return new ResponseModel() { Output = 1, Message = "Cập nhật cấu hình thành công.", Type = ResponseTypeMessage.Success };
                }
                else
                {
                    return new ResponseModel() { Output = 2, Message = "Bạn chưa nhập đầy đủ thông tin.", Type = ResponseTypeMessage.Warning };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Sitemap()
        {
            return View();
        }

        [HttpPost, ActionName("Sitemap")]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> SitemapPost(
            int? page, 
            int? limit, 
            string key,
            bool? includeSitemap,
            CategoryType? type,
            string language = "vi", 
            string ordertype = "asc", 
            string orderby = "name"
           
            )
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iLinkRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                    m =>
                        (m.Language == language) &&
                        (m.Slug.Contains(key) || m.Title.Contains(key) || m.Description.Contains(key) || m.Keywords.Contains(key) || m.FocusKeywords.Contains(key) || key == null) &&
                        m.Status &&
                        (m.Type==type || type==null) &&
                        (m.IncludeSitemap == includeSitemap || includeSitemap == null) &&
                        (m.Type!=CategoryType.Employee) &&
                        (m.Type != CategoryType.Tag) &&
                        !m.Delete,
                OrderByExtention(ordertype, orderby));
            return View("SitemapAjax", data);
        }
        private Func<IQueryable<Link>, IOrderedQueryable<Link>> OrderByExtention(string ordertype, string orderby)
        {
            return orderby switch
            {
                "name" => ordertype == "asc" ? EntityExtention<Link>.OrderBy(m => m.OrderBy(x => x.ObjectId)) : EntityExtention<Link>.OrderBy(m => m.OrderByDescending(x => x.ObjectId)),
                _ => ordertype == "asc" ? EntityExtention<Link>.OrderBy(m => m.OrderBy(x => x.Id)) : EntityExtention<Link>.OrderBy(m => m.OrderByDescending(x => x.Id)),
            };
        }

        #region [Edit]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> SitemapEdit(int id)
        {
            var dl = await _iLinkRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null || (dl != null && dl.Delete) || (dl != null && !dl.Status))
            {
                return View("404");
            }
            var model = MapModel<SeoModel>.Go(dl);
            model.LinkId = dl.Id;
            string name = "";
            string content = "";

            if (dl.Type==CategoryType.Blog || dl.Type == CategoryType.Service || dl.Type==CategoryType.Page || dl.Type == CategoryType.FAQ)
            {
                var data = await _iContentPageRepository.SingleOrDefaultAsync(true, x => x.Id == dl.ObjectId);
                name = data?.Name;
                content = data?.Content;
            }
            else if (dl.Type == CategoryType.CategoryBlog || dl.Type == CategoryType.CategoryService)
            {
                var data = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Id == dl.ObjectId);
                name = data?.Name;
                content = data?.Content;
            }
            else if (dl.Type == CategoryType.Employee)
            {
                var data = await _iEmployeeRepository.SingleOrDefaultAsync(true, x => x.Id == dl.ObjectId);
                name = "";
                content = data?.Content;
            }
            else if (dl.Type == CategoryType.ImageGallery)
            {
                var data = await _iContentPageRepository.SingleOrDefaultAsync(true, x => x.Id == dl.ObjectId);
                name = data?.Name;
                content = data?.Content;
            }
            else if (dl.Type == CategoryType.Tag)
            {
                var data = await _iContentPageRepository.SingleOrDefaultAsync(true, x => x.Id == dl.ObjectId);
                name = data?.Name;
                content = data?.Content;
            }
            else if (dl.Type == CategoryType.Static)
            {
                name = "";
                content = "";
            }
            ViewData["seo-page-name"] = name;
            ViewData["seo-page-content"] = content;
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{dl.Language}" : "";
            ViewData["IsSeoEdit"] = "in";
            return View(model);
        }

        [HttpPost, ActionName("SitemapEdit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> SitemapEditPost(SeoModel use)
        {
            try
            {
                var dl = await _iLinkRepository.SingleOrDefaultAsync(true, m => m.Id == use.LinkId);
                if (dl == null || (dl != null && dl.Delete) || (dl != null && !dl.Status))
                {
                    return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }

                if (ModelState.IsValid)
                {
                    await UpdateSeoLink(use.ChangeSlug, dl.Type, dl.Type, dl.ObjectId, dl.Language, MapModel<SeoModel>.Go(use), dl.Name, dl.Area, dl.Controller, dl.Acction);
                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập sitemap\"#{dl.Id}\".",
                        Type = LogType.Edit
                    });

                    return new ResponseModel() { Output = 1, Message = "Cập nhật sitemap thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin.", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion
    }
}