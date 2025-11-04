//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using PT.Base;
//using PT.BE.Areas.Base.Controllers;
//using PT.Domain.Model;
//using PT.Infrastructure.Interfaces;
//using PT.Shared;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

//namespace PT.BE.Areas.Setting.Controllers
//{
//    [Area("Base")]
//    [AuthorizePermission("Index")]
//    public class SeoSettingsController : BaseController
//    {
//        private readonly ILogger _logger;
//        private readonly IFileRepository _iFileRepository;
//        private readonly IWebHostEnvironment _iHostingEnvironment;
//        private readonly IOptions<List<SeoSettings>> _seoSettings;
//        private readonly ILinkRepository _iLinkRepository;
//        private readonly IOptions<BaseSettings> _baseSettings;
//        private readonly IContentPageRepository _iContentPageRepository;
//        private readonly ICategoryRepository _iCategoryRepository;
//        private readonly IEmployeeRepository _iEmployeeRepository;
//        private readonly ISeoSettingRepository _iSeoSettingRepository;
//        private readonly IPortalRepository _iPortalRepository;
//        public SeoSettingsController(
//            ILogger<SeoSettingsController> logger, 
//            IFileRepository iFileRepository,
//            IWebHostEnvironment iHostingEnvironment, 
//            IOptions<List<SeoSettings>> seoSettings,
//            ILinkRepository iLinkRepository,
//            IOptions<BaseSettings> baseSettings,
//            IContentPageRepository iContentPageRepository,
//            ICategoryRepository iCategoryRepository,
//            IEmployeeRepository  iEmployeeRepository,
//            ISeoSettingRepository iSeoSettingRepository,
//            IPortalRepository iPortalRepository
//            )
//        {
//            controllerName = "SeoSettings";
//            tableName = "Base";

//            _logger = logger;
//            _iFileRepository = iFileRepository;
//            _iHostingEnvironment = iHostingEnvironment;
//            _seoSettings = seoSettings;
//            _iLinkRepository = iLinkRepository;
//            _baseSettings = baseSettings;
//            _iContentPageRepository = iContentPageRepository;
//            _iCategoryRepository = iCategoryRepository;
//            _iEmployeeRepository = iEmployeeRepository;
//            _iSeoSettingRepository = iSeoSettingRepository;
//            _iPortalRepository = iPortalRepository;
//        }
//        [HttpGet]
//        [AuthorizePermission("Index")]
//        public async Task<IActionResult> Index()
//        {
//            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
//            ViewData["portals"] = portals;
//            return View();
//        }

//        #region [Seo]
//        [HttpGet]
//        [AuthorizePermission("Index")]
//        public async Task<IActionResult> Seo(string language = "vi", int portalId = 1)
//        {
//            var portals = await _iPortalRepository.SearchAsync(true,0,0);
//            var data = await _iSeoSettingRepository.SingleOrDefaultAsync(true,x => x.Language == language && x.PortalId == portalId);
//            if(data == null)
//            {
//                return View(new SeoSetting { Language = language, PortalId= portalId, Portals= portals });
//            }
//            else
//            {
//                data.Portals = portals;
//                return View(data);
//            }    
//        }
//        [HttpPost, ValidateAntiForgeryToken, ActionName("Seo")]
//        [AuthorizePermission("Index")]
//        public async Task<ResponseModel> SeoPost(SeoSetting model, string language = "vi", int portalId = 1)
//        {
//            try
//            {
//                if (ModelState.IsValid)
//                {
//                    var data = await _iSeoSettingRepository.SingleOrDefaultAsync(true, x => x.Language == language && x.PortalId == portalId);
//                    if (data == null)
//                    {
//                        _iSeoSettingRepository.Add(new SeoSetting
//                        {
//                            Language = language,
//                            PortalId = portalId,
//                            Title = model.Title,
//                            Description = model.Description,
//                            Keywords = model.Keywords,
//                            MetaGoogle = model.MetaGoogle,
//                            Robots = model.Robots
//                        });
//                    }
//                    else
//                    {
//                        data.Title = model.Title;
//                        data.Description = model.Description;
//                        data.Keywords = model.Keywords;
//                        data.MetaGoogle = model.MetaGoogle;
//                        data.Robots = model.Robots;
//                        _iSeoSettingRepository.Update(data);
//                    }
//                    await _iSeoSettingRepository.CommitAsync();
//                    await AddLog(new LogModel { Name = $"Cập nhật cấu thông tin seo {model.Id}.", Type = LogType.Edit });

//                    return new ResponseModel() { Output = 1, Message = "Cập nhật cấu hình thành công.", Type = ResponseTypeMessage.Success };
//                }
//                else
//                {
//                    return new ResponseModel() { Output = 2, Message = "Bạn chưa nhập đầy đủ thông tin.", Type = ResponseTypeMessage.Warning };
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
//            }
//            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
//        }
//        #endregion

//        [HttpGet]
//        [AuthorizePermission("Index")]
//        public IActionResult Sitemap()
//        {
//            return View();
//        }

//        [HttpPost, ActionName("Sitemap")]
//        [AuthorizePermission("Index")]
//        public async Task<IActionResult> SitemapPost(
//            int? page, 
//            int? limit, 
//            string key,
//            bool? includeSitemap,
//            ESlugType? type,
//            string language = "vi", 
//            string ordertype = "asc", 
//            string orderby = "name"
           
//            )
//        {
//            page = page < 0 ? 1 : page;
//            limit = (limit > 100 || limit < 10) ? 10 : limit;
//            var data = await _iLinkRepository.SearchPagedListAsync(
//                page ?? 1,
//                limit ?? 10,
//                    m =>
//                        (m.Language == language) &&
//                        (m.Slug.Contains(key) || m.Title.Contains(key) || m.Description.Contains(key) || m.Keywords.Contains(key) || m.FocusKeywords.Contains(key) || key == null) &&
//                        m.Status &&
//                        (m.Type==type || type==null) &&
//                        (m.IncludeSitemap == includeSitemap || includeSitemap == null)
//                        ,
//                OrderByExtention(ordertype, orderby));
//            return View("SitemapAjax", data);
//        }
//        private Func<IQueryable<Link>, IOrderedQueryable<Link>> OrderByExtention(string ordertype, string orderby)
//        {
//            return orderby switch
//            {
//                "name" => ordertype == "asc" ? EntityExtention<Link>.OrderBy(m => m.OrderBy(x => x.ObjectId)) : EntityExtention<Link>.OrderBy(m => m.OrderByDescending(x => x.ObjectId)),
//                _ => ordertype == "asc" ? EntityExtention<Link>.OrderBy(m => m.OrderBy(x => x.Id)) : EntityExtention<Link>.OrderBy(m => m.OrderByDescending(x => x.Id)),
//            };
//        }

//        #region [Edit]
//        [HttpGet]
//        [AuthorizePermission("Index")]
//        public async Task<IActionResult> SitemapEdit(int id)
//        {
//            var dl = await _iLinkRepository.SingleOrDefaultAsync(true, m => m.Id == id);
//            if (dl == null || (dl != null && dl.Delete) || (dl != null && !dl.Status))
//            {
//                return View("404");
//            }
//            var model = MapModel<SeoModel>.Go(dl);
//            model.LinkId = dl.Id;
//            string name = "";
//            string content = "";

//            if (dl.Type==ESlugType.ContentPage)
//            {
//                var data = await _iContentPageRepository.SingleOrDefaultAsync(true, x => x.Id == dl.ObjectId);
//                name = data?.Name;
//                content = data?.Content;
//            }
//            else if (dl.Type == ESlugType.Category)
//            {
//                var data = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Id == dl.ObjectId);
//                name = data?.Name;
//                content = data?.Content;
//            }
//            else if (dl.Type == ESlugType.Employee)
//            {
//                var data = await _iEmployeeRepository.SingleOrDefaultAsync(true, x => x.Id == dl.ObjectId);
//                name = "";
//                content = data?.Content;
//            }
//            else if (dl.Type == ESlugType.ImageGallery)
//            {
//                var data = await _iContentPageRepository.SingleOrDefaultAsync(true, x => x.Id == dl.ObjectId);
//                name = data?.Name;
//                content = data?.Content;
//            }
//            else if (dl.Type == ESlugType.Tag)
//            {
//                var data = await _iContentPageRepository.SingleOrDefaultAsync(true, x => x.Id == dl.ObjectId);
//                name = data?.Name;
//                content = data?.Content;
//            }
//            else if (dl.Type == ESlugType.Static)
//            {
//                name = "";
//                content = "";
//            }
//            ViewData["seo-page-name"] = name;
//            ViewData["seo-page-content"] = content;
//            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{dl.Language}" : "";
//            ViewData["IsSeoEdit"] = "in";
//            return View(model);
//        }

//        [HttpPost, ActionName("SitemapEdit")]
//        [AuthorizePermission("Index")]
//        public async Task<ResponseModel> SitemapEditPost(SeoModel use)
//        {
//            try
//            {
//                var dl = await _iLinkRepository.SingleOrDefaultAsync(true, m => m.Id == use.LinkId);
//                if (dl == null || (dl != null && dl.Delete) || (dl != null && !dl.Status))
//                {
//                    return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
//                }

//                if (ModelState.IsValid)
//                {
//                    await UpdateLinkAsync(use.ChangeSlug, dl.Type,  dl.ObjectId, dl.Language, MapModel<SeoModel>.Go(use), dl.Name, dl.Area, dl.Controller, dl.Acction);
//                    await AddLog(new LogModel
//                    {
//                        ObjectId = dl.Id,
//                        ActionTime = DateTime.Now,
//                        Name = $"Cập sitemap\"#{dl.Id}\".",
//                        Type = LogType.Edit
//                    });

//                    return new ResponseModel() { Output = 1, Message = "Cập nhật sitemap thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
//                }
//                return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin.", Type = ResponseTypeMessage.Warning };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
//            }
//            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
//        }
//        #endregion
//    }
//}