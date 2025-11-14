using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Infrastructure.Repositories;
using PT.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PT.BE.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class PageFlowManagerController : Base.Controllers.BaseController
    {
        private readonly ILogger _logger;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ILinkRepository _iLinkRepository;
        private readonly ITagRepository _iTagRepository;
        private readonly IContentPageTagRepository _iContentPageTagRepository;
        private readonly IWebHostEnvironment _iWebHostEnvironment;
        private readonly IFileRepository _iFileRepository;
        private readonly IPortalRepository _iPortalRepository;
        private readonly IContentPageRelatedRepository _iContentPageRelatedRepository;
        public PageFlowManagerController(
            ILogger<PageFlowManagerController> logger,
            IOptions<BaseSettings> baseSettings,
            IContentPageRepository iContentPageRepository,
            ILinkRepository iLinkRepository,
            ITagRepository iTagRepository,
            IContentPageTagRepository iContentPageTagRepository,
            IWebHostEnvironment iWebHostEnvironment,
            IFileRepository iFileRepository,
            IPortalRepository iPortalRepository,
            IContentPageRelatedRepository iContentPageRelatedRepository
        )
        {
            controllerName = "PageFlowManager";
            tableName = "ContentPage";
            _logger = logger;
            _baseSettings = baseSettings;
            _iContentPageRepository = iContentPageRepository;
            _iLinkRepository = iLinkRepository;
            _iTagRepository = iTagRepository;
            _iContentPageTagRepository = iContentPageTagRepository;
            _iWebHostEnvironment = iWebHostEnvironment;
            _iFileRepository = iFileRepository;
            _iPortalRepository = iPortalRepository;
            _iContentPageRelatedRepository = iContentPageRelatedRepository;
        }

        #region [Index]
        [AuthorizePermission]
        public async Task<IActionResult> Index()
        {
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            ViewData["PortalSelectList"] = new SelectList(portals, "Id", "Name");
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, int? categoryId, int? tagId, bool? status, int? portalId, string language ="vi", string ordertype = "asc", string orderby = "name", ECategoryType? categoryType = null)
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iContentPageRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                categoryId,
                tagId,
                    m =>(m.Name.Contains(key) || key == null || m.Content.Contains(key) || m.Summary.Contains(key)) && 
                        (m.Language== language) && 
                        (m.Status==status || status ==null) && 
                        (m.PortalId== portalId || portalId == null) &&
                        (m.CategoryType == categoryType || categoryType == null) &&
                        (m.CategoryType == ECategoryType.ContentPage_Flow || m.CategoryType == ECategoryType.ContentPage_FlowItems || m.CategoryType == ECategoryType.ContentPage_Solution),
                OrderByExtention(ordertype, orderby), 
                x=> new ContentPage {
                    Category = x.Category,
                    Id = x.Id,
                    Author = x.Author,
                    Banner = x.Banner,
                    DatePosted = x.DatePosted,
                    Name = x.Name,
                    Language = x.Language,
                    Price = x.Price,
                    Serice = x.Serice,
                    ServiceId = x.ServiceId,
                    Status = x.Status,
                    Summary = x.Summary,
                    Tags = x.Tags,
                    Type = x.Type,
                    Link = x.Link,
                    IsHome = x.IsHome,
                    PortalId = x.PortalId,
                    CategoryId = x.CategoryId,
                    CategoryType = x.CategoryType,
                    Extentions = x.Extentions,
                    StartDate = x.StartDate,
                    TimeFromTo = x.TimeFromTo,
                    Topic = x.Topic,
                    DeliveryTime = x.DeliveryTime,
                    Address = x.Address,
                    FilePath = x.FilePath,
                    Pages = x.Pages,
                    SlugType = x.SlugType
                });
            var portals = await _iPortalRepository.SearchAsync(true);
            foreach (var item in data.Data)
            {
                item.Portal = portals.FirstOrDefault(x => x.Id == item.PortalId);
            }
            return View("IndexAjax", data);
        }
        private Func<IQueryable<ContentPage>, IOrderedQueryable<ContentPage>> OrderByExtention(string ordertype, string orderby)
        {
            Func<IQueryable<ContentPage>, IOrderedQueryable<ContentPage>> functionOrder = null;
            switch (orderby)
            {
                case "name":
                    functionOrder = ordertype == "asc" ? EntityExtention<ContentPage>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<ContentPage>.OrderBy(m => m.OrderByDescending(x => x.Name));
                    break;
                default:
                    functionOrder = ordertype == "asc" ? EntityExtention<ContentPage>.OrderBy(m => m.OrderBy(x => x.DatePosted)) : EntityExtention<ContentPage>.OrderBy(m => m.OrderByDescending(x => x.DatePosted));
                    break;
            }
            return functionOrder;
        }
        #endregion

        
        #region [Create Flow item]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> CreateSolution(int? portalId, string language = "vi")
        {
            return await Create(portalId, language, ECategoryType.ContentPage_Solution);
        }
        #endregion

        #region [Create Flow item]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> CreateItem(int? portalId, string language = "vi")
        {
            return await Create(portalId, language, ECategoryType.ContentPage_FlowItems);
        }
        #endregion

        #region [Create]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Create(int? portalId,string language = "vi", ECategoryType categoryType = ECategoryType.ContentPage_Flow)
        {
            var dl = new PageModel
            {
                Language = language
            };
            dl.TagSelectList = new MultiSelectList(await _iTagRepository.SearchAsync(true, 0, 0, x => x.Status && x.Language == language, x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Language = x.Language, Status = x.Status }), "Id", "Name");
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
            // Lấy danh sách portal để hiển thị trong dropdown trên trang quản lý
            // Tham số: true = only active, 0,0 = không phân trang
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            // Đưa danh sách portal vào ViewData để view có thể bind vào SelectList
            dl.PortalSelectList = new SelectList(portals, "Id", "Name");
            // Trả về view chính. Dữ liệu bảng sẽ được nạp bằng Ajax gọi IndexPost
            dl.PortalId = portalId ?? 1;
            dl.PortalName = (await _iPortalRepository.SingleOrDefaultAsync(true, x => x.Id == portalId))?.Name;
            dl.CategoryType = categoryType;
            return View("Create", dl);
        }

        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(PageModel use, string altId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _iContentPageRepository.BeginTransaction();
                    var data = new ContentPage
                    {
                        Name = use.Name,
                        Banner = use.Banner,
                        Content = use.Content,
                        Status = use.Status,
                        Language = use.Language,
                        Type = CategoryType.ContentPagePage,
                        SlugType = ESlugType.ContentPage,  
                        Summary = use.Summary,
                        DatePosted = DateTime.Now,
                        PortalId = use.PortalId ?? 1,
                        CategoryType = use.CategoryType,
                        Input1 = use.Input1,
                        Input2 = use.Input2,
                        Input3 = use.Input3,
                        Input4 = use.Input4,
                        Input5 = use.Input5,
                        Input6 = use.Input6,
                        Input7 = use.Input7,
                        Input8 = use.Input8,
                    };
                    await _iContentPageRepository.AddAsync(data);
                    await _iContentPageRepository.CommitAsync();

                    await CreateLinkAsync(ESlugType.ContentPage, data.Language, data.Id, MapModel<SeoModel>.Go(use), data.Name, "", "ContentPage", "Details", data.PortalId);
                    await UpdateRelated(data.Id, use.ContentPageRelatedIds);
                    await UpdateTag(data.Id, use.TagIds);
                    await UpdateFileData(data.Id, ESlugType.ContentPage, altId);
                    await _iContentPageRepository.CommitTransaction();
                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới trang nội dung \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới Trang nội dung thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [Edit]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Edit(int id)
        {
            var dl = await _iContentPageRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null)
            {
                return View("404");
            }
            var model = MapModel<PageModel>.Go(dl);
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{dl.Language}" : "";
            var ktLink = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.ObjectId == id && x.Type == ESlugType.ContentPage);
            if (ktLink != null)
            {
             model.Changefreq = ktLink.Changefreq;
                model.Lastmod = ktLink.Lastmod;
                model.Priority = ktLink.Priority.ConvertToString();
                model.Description = ktLink.Description;
                model.FacebookBanner = ktLink.FacebookBanner;
                model.FacebookDescription = ktLink.FacebookDescription;
                model.FocusKeywords = ktLink.FocusKeywords;
                model.GooglePlusDescription = ktLink.GooglePlusDescription;
                model.IncludeSitemap = ktLink.IncludeSitemap;
                model.Keywords = ktLink.Keywords;
                model.MetaRobotsAdvance = ktLink.MetaRobotsAdvance;
                model.MetaRobotsFollow = ktLink.MetaRobotsFollow;
                model.MetaRobotsIndex = ktLink.MetaRobotsIndex;
                model.Redirect301 = ktLink.Redirect301;
                model.Title = ktLink.Title;
                model.LinkId = ktLink.Id;
                model.Slug = ktLink.Slug;
            }
            var blogTagIds = (await _iContentPageTagRepository.SearchAsync(true, 0, 0, x => x.ContentPageId == id)).Select(x=>x.TagId).ToList();
            model.TagSelectList = new MultiSelectList(await _iTagRepository.SearchAsync(true, 0, 0, x => x.Status && x.Language == model.Language, x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Language = x.Language, Status = x.Status }), "Id", "Name");
            model.TagIds = blogTagIds;
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            // Đưa danh sách portal vào ViewData để view có thể bind vào SelectList
            model.PortalSelectList = new SelectList(portals, "Id", "Name");
            model.PortalId = dl.PortalId;
            model.PortalName = (await _iPortalRepository.SingleOrDefaultAsync(true, x => x.Id == dl.PortalId))?.Name;
            var listRelated = (await _iContentPageRelatedRepository.GetContentPageAsync(id, 0, 0, null, x => x.OrderBy(m => m.DatePosted), x => new ContentPage { Id = x.Id, DatePosted = x.DatePosted, Status = x.Status, Name = x.Name })).Select(x => new { id = x.Id, text = x.Name });
            model.ContentPageRelatedIds = string.Join(',', listRelated.Select(x => x.id));
            model.RelatedString = Newtonsoft.Json.JsonConvert.SerializeObject(listRelated);
            model.FullPath = await _iPortalRepository.GetFullPathAsync(model.PortalId ?? 1, model.Slug ?? string.Empty, portals, model.Language, _baseSettings.Value.MultipleLanguage);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(PageModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _iContentPageRepository.BeginTransaction();
                    var dl = await _iContentPageRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    dl.Name = use.Name;
                    dl.Banner = use.Banner;
                    dl.Content = use.Content;
                    dl.Status = use.Status;
                    dl.Summary = use.Summary;
                    dl.Input1 = use.Input1;
                    dl.Input2 = use.Input2;
                    dl.Input3 = use.Input3;
                    dl.Input4 = use.Input4;
                    dl.Input5 = use.Input5;
                    dl.Input6 = use.Input6;
                    dl.Input7 = use.Input7;
                    dl.Input8 = use.Input8;

                    _iContentPageRepository.Update(dl);
                    await _iContentPageRepository.CommitAsync();

                    await UpdateLinkAsync(use.ChangeSlug, ESlugType.ContentPage, dl.Id, dl.Language, MapModel<SeoModel>.Go(use),dl.Name, "", "ContentPage", "Details");
                    await UpdateRelated(dl.Id, use.ContentPageRelatedIds);
                    await UpdateTag(id, use.TagIds);
                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật Trang nội dung \"{dl.Name}\".",
                        Type = LogType.Edit
                    });
                    await _iContentPageRepository.CommitTransaction();
                    return new ResponseModel() { Output = 1, Message = "Cập nhật Trang nội dung thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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

        #region [UpdateTag]
        private async Task UpdateTag(int blogId, List<int> list)
        {
            if (list == null)
            {
                list = new List<int>();
            }
            var _current = await _iContentPageTagRepository.SearchAsync(true, 0, 0, x => x.ContentPageId == blogId);
            var idsAdd = list.Where(x => !_current.Any(y => y.TagId == x));
            _iContentPageTagRepository.DeleteWhere(x => x.ContentPageId == blogId && !list.Contains(x.TagId));
            foreach (var item in idsAdd)
            {
                await _iContentPageTagRepository.AddAsync(new ContentPageTag { ContentPageId = blogId, TagId = item });
            }
            await _iContentPageTagRepository.CommitAsync();
        }
        #endregion

        #region [Delete]
        [HttpPost, ActionName("Delete")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeletePost(int id)
        {
            try
            {
                await _iContentPageRepository.BeginTransaction();
                var kt = await _iContentPageRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null)
                {
                    return new ResponseModel() { Output = 0, Message = "Trang nội dung không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                _iContentPageRepository.Delete(kt);
                await _iContentPageRepository.CommitAsync();
                await DeleteSeoLink(kt.SlugType ?? ESlugType.ContentPage, kt.Id);
                await RemoveFileData(id, ESlugType.ContentPage);

                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa Trang nội dung \"{kt.Name}\".",
                    Type = LogType.Delete
                });
                await _iContentPageRepository.CommitTransaction();
                return new ResponseModel() { Output = 1, Message = "Xóa Trang nội dung thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [Tags]
        [HttpPost, AuthorizePermission("Index"), ActionName("AddTag")]
        public async Task<object> AddTag(string name, string language)
        {
            try
            {
                return await AddTagLink(name, language);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
                return null;
            }
        }
        #endregion

        #region [Select tag]
        [HttpGet, Authorize]
        public async Task<object> SearchTag(string language = "vi")
        {
            return (await _iTagRepository.SearchAsync(true, 0, 0, x => x.Status && x.Language == language, x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Language = x.Language, Status = x.Status })).Select(x=> new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
        }
        #endregion

        #region [Upload file]
        /// <summary>
        /// Upload image cho category: validate extension, kích thước, resize nếu cần và ghi file metadata.
        /// - Trả về ResponseModel hoặc CKEditor response tuỳ type.
        /// </summary>
        [HttpPost, ActionName("UploadImage")]
        [AuthorizePermission("Index")]
        public async Task<object> UploadImagePost(string altId, int id, int type = 0)
        {
            try
            {
                var allowed = (_baseSettings.Value.ImagesType ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

                var folderByDate = Functions.GenFolderByDate();
                string configuredDataPath = _baseSettings.Value.DataPath;

                // Lấy đường dẫn vật lý và public base url, helper đã tạo và đảm bảo thư mục tồn tại
                var (physicalPath, publicUrlBase) = Functions.SetupSharedDataFolder(configuredDataPath, folderByDate);

                var file = Request.Form.Files.FirstOrDefault();
                if (file == null)
                    return new ResponseModel<FileDataModel> { Output = 0, Message = "Không có tệp được gửi.", Type = ResponseTypeMessage.Warning };

                var ext = Path.GetExtension(file.FileName);
                if (!allowed.Contains(ext))
                    return new ResponseModel<FileDataModel> { Output = 2, Message = "Tệp tải lên không đúng định dạng.", Type = ResponseTypeMessage.Warning };

                if (_baseSettings.Value.ImagesMaxSize < file.Length)
                    return new ResponseModel<FileDataModel> { Output = 3, Message = "Tệp tải lên vượt quá kích thước cho phép.", Type = ResponseTypeMessage.Warning };

                var safeName = Path.GetFileNameWithoutExtension(file.FileName);
                var fileName = safeName + ext;
                var fullPath = Path.Combine(physicalPath, fileName);
                if (System.IO.File.Exists(fullPath))
                {
                    fileName = $"{safeName}_{id}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                    fullPath = Path.Combine(physicalPath, fileName);
                }

                // Lưu file: resize nếu cần, else copy
                try
                {
                    using var img = System.Drawing.Image.FromStream(file.OpenReadStream());
                    if ((img.Width > _baseSettings.Value.ImageMaxWith) || (img.Height > _baseSettings.Value.ImageMaxWith))
                    {
                        _iFileRepository.ResizeImage(file, fullPath, _baseSettings.Value.ImageMaxWith, false);
                    }
                    else
                    {
                        using var fs = new FileStream(fullPath, FileMode.Create);
                        // reset stream position
                        file.OpenReadStream().CopyTo(fs);
                    }
                }
                catch
                {
                    // Nếu không thể load image thì ghi trực tiếp
                    using var fs = new FileStream(fullPath, FileMode.Create);
                    await file.CopyToAsync(fs);
                }

                var publicUrl = (publicUrlBase ?? "/Data/").Replace("\\", "/");
                if (!publicUrl.EndsWith("/")) publicUrl += "/";
                publicUrl = publicUrl + Uri.EscapeDataString(fileName);

                await AddFileData(id, publicUrl, ESlugType.ContentPage, altId);

                if (type == 1)
                {
                    return new FileDataCKEditerModel
                    {
                        FileName = fileName,
                        Number = 200,
                        Uploaded = 1,
                        Url = publicUrl
                    };
                }

                return new ResponseModel<FileDataModel>
                {
                    Output = 1,
                    Message = "Tải tệp lên thành công.",
                    Type = ResponseTypeMessage.Success,

                    Data = new FileDataModel
                    {
                        CreatedDate = DateTime.Now,
                        CreatedUser = DataUserInfo.UserId,
                        Path = publicUrl,
                        FileName = fileName
                    },
                    IsClosePopup = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }

            return new ResponseModel<FileDataModel>() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        [HttpPost, Authorize]
        public async Task<List<SelectListItem>> SearchContentPage(string q, int top = 10, string language = "vi", int portalId = 0)
        {
            top = top > 100 ? 100 : top;
            return (await _iContentPageRepository.SearchAsync(true, 0, top, x => x.Name.ToLower().Contains(q.ToLower()) && x.Status && x.CategoryType == PT.Domain.Model.ECategoryType.ContentPage_FlowItems && x.Language == language && x.PortalId == portalId, x => x.OrderBy(y => y.Name),
                x => new ContentPage { Id = x.Id, Name = x.Name, Status = x.Status, Language = x.Language, Type = x.Type })).Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
        }


        private async Task UpdateRelated(int blogId, string strData)
        {
            var list = new List<int>();
            if (strData != null && strData != "")
            {
                list = strData.Split(',').Select(x => int.Parse(x)).ToList();
            }
            var _current = await _iContentPageRelatedRepository.SearchAsync(true, 0, 0, x => x.ParentId == blogId);
            var idsAdd = list.Where(x => !_current.Any(y => y.ContentPageId == x));
            _iContentPageRelatedRepository.DeleteWhere(x => x.ParentId == blogId && !list.Contains(x.ContentPageId));
            foreach (var item in idsAdd)
            {
                await _iContentPageRelatedRepository.AddAsync(new ContentPageRelated { ParentId = blogId, ContentPageId = item });
            }
            await _iContentPageRelatedRepository.CommitAsync();
        }
    }
}