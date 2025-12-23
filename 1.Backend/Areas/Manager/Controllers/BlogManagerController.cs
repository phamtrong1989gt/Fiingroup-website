using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Base;
using PT.Base.Services;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Infrastructure.Repositories;
using PT.Shared;
using PT.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PT.BE.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class BlogManagerController : Base.Controllers.BaseController
    {
        private readonly ILogger _logger;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ILinkRepository _iLinkRepository;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly ITagRepository _iTagRepository;
        private readonly IContentPageCategoryRepository _iContentPageCategoryRepository;
        private readonly IContentPageTagRepository _iContentPageTagRepository;
        private readonly IContentPageRelatedRepository _iContentPageRelatedRepository;
        private readonly IContentPageReferenceRepository _iContentPageReferenceRepository;
        private readonly IWebHostEnvironment _iWebHostEnvironment;
        private readonly IFileRepository _iFileRepository;
        private readonly IPortalRepository _iPortalRepository;
        private readonly IAsyncNewsService _iAsyncNewsService;

        public BlogManagerController(
            ILogger<BlogManagerController> logger,
            IOptions<BaseSettings> baseSettings,
            IContentPageRepository iContentPageRepository,
            ILinkRepository iLinkRepository,
            ICategoryRepository iCategoryRepository,
            ITagRepository iTagRepository,
            IContentPageCategoryRepository iContentPageCategoryRepository,
            IContentPageTagRepository iContentPageTagRepository,
            IContentPageRelatedRepository iContentPageRelatedRepository,
            IContentPageReferenceRepository iContentPageReferenceRepository,
            IWebHostEnvironment iWebHostEnvironment,
            IFileRepository iFileRepository,
            IPortalRepository iPortalRepository,
            IAsyncNewsService iAsyncNewsService
        )
        {
            controllerName = "BlogManager";
            tableName = "Blog";
            _logger = logger;
            _baseSettings = baseSettings;
            _iContentPageRepository = iContentPageRepository;
            _iLinkRepository = iLinkRepository;
            _iCategoryRepository = iCategoryRepository;
            _iTagRepository = iTagRepository;
            _iContentPageCategoryRepository = iContentPageCategoryRepository;
            _iContentPageTagRepository = iContentPageTagRepository;
            _iContentPageRelatedRepository = iContentPageRelatedRepository;
            _iContentPageReferenceRepository = iContentPageReferenceRepository;
            _iWebHostEnvironment = iWebHostEnvironment;
            _iFileRepository = iFileRepository;
            _iPortalRepository = iPortalRepository;
            _iAsyncNewsService = iAsyncNewsService;
        }

        #region [Index]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Index()
        {
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            ViewData["PortalSelectList"] = new SelectList(portals, "Id", "Name");
            return View();
        }

        [HttpPost, ActionName("Index")]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, int? categoryId, int? tagId, bool? status, int? portalId, string language = "vi", string ordertype = "asc", string orderby = "name", ECategoryType? categoryType = null)
        {
            var allows = _iCategoryRepository.GetCategoryPrefixedTypes().Where(x=> x != ECategoryType.ContentPage_Page && x != ECategoryType.ContentPage_Solution && x != ECategoryType.ContentPage_Flow && x != ECategoryType.ContentPage_FlowItems);
            var categorys = await _iCategoryRepository.SearchAsync(true, 0, 0);
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iContentPageRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                categoryId,
                tagId,
                m => (m.Name.Contains(key) || key == null || m.Content.Contains(key) || m.Summary.Contains(key)) 
                     && (m.CategoryType != null && allows.Contains(m.CategoryType ?? ECategoryType.ContentPage_Blog)) &&
                    (m.Language == language) &&
                    (m.Status == status || status == null) &&
                    (m.CategoryType == categoryType || categoryType == null) &&
                    (m.PortalId == portalId || portalId  == null) 
                    ,
                OrderByExtention(ordertype, orderby), x => new ContentPage
                {
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
                    SlugType = x.SlugType,
                    NewsId = x.NewsId,
                    Input14 = x.Input14,
                    Input15 = x.Input15
                });

            var portals = await _iPortalRepository.SearchAsync(true);
            foreach (var item in data.Data)
            {
                item.Portal = portals.FirstOrDefault(x => x.Id == item.PortalId);
                item.Category = categorys.FirstOrDefault(x => x.Id == item.CategoryId);
                item.FullPath = await _iPortalRepository.GetFullPathAsync(item.PortalId, item.Link?.Slug, portals, item.Language, _baseSettings.Value.MultipleLanguage);
            }
            return View("IndexAjax", data);
        }
        private Func<IQueryable<ContentPage>, IOrderedQueryable<ContentPage>> OrderByExtention(string ordertype, string orderby) =>
            orderby switch
            {
                "name" => ordertype == "asc" ? EntityExtention<ContentPage>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<ContentPage>.OrderBy(m => m.OrderByDescending(x => x.Name)),
                _ => ordertype == "asc" ? EntityExtention<ContentPage>.OrderBy(m => m.OrderBy(x => x.DatePosted)) : EntityExtention<ContentPage>.OrderBy(m => m.OrderByDescending(x => x.DatePosted)),
            };
        #endregion

        #region [Create]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Create(string language = "vi", int portalId = 1)
        {
            var dl = new BlogModel
            {
                Language = language
            };
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
            dl.TagSelectList = new MultiSelectList(await _iTagRepository.SearchAsync(true, 0, 20, x => x.Status && x.Language == language && x.PortalId == portalId, x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Language = x.Language, Status = x.Status }), "Id", "Name");
            dl.PortalName = (await _iPortalRepository.SingleOrDefaultAsync(true, x => x.Id == portalId))?.Name;
            dl.PortalId = portalId;
            var categorys = (await CategorysAsync(language, portalId)).Where(x=> x.CategoryType == ECategoryType.ContentPage_Blog || x.CategoryType == ECategoryType.ContentPage_Publications).ToList();
            ViewData["CategoryJson"] = Newtonsoft.Json.JsonConvert.SerializeObject(categorys.Select(x=> new { x.Id, x.CategoryType, x.SlugType }));
            dl.CategorySelectList = await GetPortalSelectList(categorys, language, portalId);
            return View(dl);
        }
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(BlogModel use, string categoryIds, string altId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Lấy ra danh mục chính
                    await _iContentPageRepository.BeginTransaction();
                    var categoryMain = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Id == use.CategoryId);
                    if(categoryMain == null || categoryMain.CategoryType == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Danh mục chính không tồn tại hoặc dữ liệu chưa dc chuẩn hóa, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    var data = new ContentPage
                    {
                        Name = use.Name,
                        Banner = use.Banner,
                        Content = use.Content,
                        Status = use.Status,
                        Language = use.Language,
                        Summary = use.Summary,
                        DatePosted = use.DatePosted,
                        Author = use.Author,
                        PortalId = use.PortalId ?? 1,
                        CategoryId = use.CategoryId,
                        SlugType = ESlugType.ContentPage,
                        CategoryType = categoryMain.CategoryType,
                        DeliveryTime = use.DeliveryTime,
                        Address = use.Address,
                        Price = use.Price,
                        FilePath = use.FilePath,
                        Extentions = use.Extentions,
                         Pages = use.Pages,
                        StartDate = use.StartDate,
                        Topic = use.Topic,
                        TimeFromTo = use.TimeFromTo,
                        IsHome = use.IsHome
                    };
                    await _iContentPageRepository.AddAsync(data);
                    await _iContentPageRepository.CommitAsync();

                    var linkId =  await CreateLinkAsync(ESlugType.ContentPage, data.Language, data.Id, MapModel<SeoModel>.Go(use), data.Name, "", "ContentPage", "Details", data.PortalId);
                    await UpdateCategory(data.Id, categoryIds, data.CategoryId);
                    await UpdateTag(data.Id, use.TagIds);
                    await UpdateRelated(data.Id, use.ContentPageRelatedIds);
                    await UpdateReference(data.Id, use.ContentPageReferenceIds);
                    await UpdateFileData(data.Id, ESlugType.ContentPage, altId);
                    await _iContentPageRepository.CommitTransaction();
                    try
                    {
                        var listCategorys = new List<int>();
                        if (!string.IsNullOrWhiteSpace(use.CategoryIds))
                        {
                            listCategorys = use.CategoryIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
                        }
                        data.Link = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.Id == linkId);
                        var outData = await _iAsyncNewsService.CreateAsync(data, use.TagIds, listCategorys);
                        if (outData != null && outData.Success)
                        {
                            data.NewsId = outData.Data.NewsId;
                            string note = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Tạo tin mới {data.NewsId} thành công";
                            data.Input14 = $"<i title=\"{note}\" class=\"material-icons text-success icon-label-status-syns\">check</i>";
                            _iContentPageRepository.Update(data);
                            await _iContentPageRepository.CommitAsync();
                        }
                        else
                        {
                            string note = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Tạo tin mới của tin #{data.Id} thất bại: {outData.ErrorMessage}";
                            data.Input14 = $"<i title=\"{note}\" class=\"material-icons text-danger icon-label-status-syns\">close</i>";
                            _iContentPageRepository.Update(data);
                            await _iContentPageRepository.CommitAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
                    }
                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới tin tức \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới tin tức thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin hoặc liên kết thân thiện/Permalink đã tồn tại, vui lòng thay thêm ký tự bất kỳ đằng sau", Type = ResponseTypeMessage.Warning };
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
            var model = MapModel<BlogModel>.Go(dl);
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
            var blogTagIds = (await _iContentPageTagRepository.SearchAsync(true, 0, 0, x => x.ContentPageId == id)).Select(x => x.TagId).ToList();
            model.TagSelectList = new MultiSelectList(await _iTagRepository.SearchAsync(true, 0, 20, x => x.Status && x.Language == model.Language && x.PortalId == dl.PortalId, x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Language = x.Language, Status = x.Status }), "Id", "Name");
            model.TagIds = blogTagIds;

            var listRelated = (await _iContentPageRelatedRepository.GetContentPageAsync(id, 0, 0, null, x => x.OrderBy(m => m.DatePosted), x => new ContentPage { Id = x.Id, DatePosted = x.DatePosted, Status = x.Status, Name = x.Name })).Select(x => new { id = x.Id, text = x.Name });
            model.ContentPageRelatedIds = string.Join(',', listRelated.Select(x => x.id));
            model.RelatedString = Newtonsoft.Json.JsonConvert.SerializeObject(listRelated);

            var listReference = (await _iContentPageReferenceRepository.SearchAsync(true, 0, 0, x => x.ContentPageId == id)).Select(x => new ContentPageReferenceModel { ContentPageId = x.ContentPageId, Href = x.Href, Id = x.Id, Name = x.Name, Rel = x.Rel, Stt = 0, Type = 2, Target = x.Target });
            model.ReferenceString = Newtonsoft.Json.JsonConvert.SerializeObject(listReference);

            model.CategoryIds = string.Join(",", (await _iContentPageCategoryRepository.SearchAsync(true, 0, 0, x => x.ContentPageId == id)).Select(x => x.CategoryId));

            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            // Đưa danh sách portal vào ViewData để view có thể bind vào SelectList
            model.PortalSelectList = new SelectList(portals, "Id", "Name");
            model.PortalId = dl.PortalId;
            model.PortalName = portals.SingleOrDefault(x => x.Id == dl.PortalId)?.Name;
            var currentShared = await _iContentPageRepository.ContentPageSharedGets(model.Id);
            model.PortalShareds = portals.Where(x=>x.Id != model.PortalId).Select(x=> new PortalSharedModel { Id = x.Id, Name = x.Name, Selected = false}).ToList();
            foreach(var item in model.PortalShareds)
            {
                item.Selected = currentShared.Any(x => (x.ParentPortalId == item.Id) || ( x.SharedPortalId == item.Id));
            }
            var categorys = (await CategorysAsync(model.Language, model.PortalId ?? 1)).Where(x=> x.CategoryType == ECategoryType.ContentPage_Blog || x.CategoryType == ECategoryType.ContentPage_Publications).ToList();
            ViewData["CategoryJson"] = Newtonsoft.Json.JsonConvert.SerializeObject(categorys.Select(x => new { x.Id, x.CategoryType, x.SlugType }));
            model.CategorySelectList = await GetPortalSelectList(categorys, model.Language, model.PortalId ?? 1, model.CategoryId);
            model.FullPath = await _iPortalRepository.GetFullPathAsync(model.PortalId ?? 1, model.Slug ?? string.Empty, portals, model.Language, _baseSettings.Value.MultipleLanguage);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(BlogModel use, int id)
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
                    var categoryMain = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Id == use.CategoryId);
                    if (categoryMain == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Danh mục chính không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    dl.Name = use.Name;
                    dl.CategoryId = use.CategoryId;
                    dl.Banner = use.Banner;
                    dl.Content = use.Content;
                    dl.Status = use.Status;
                    dl.Summary = use.Summary;
                    dl.DatePosted = use.DatePosted;
                    dl.Author = use.Author;
                    dl.PortalId = use.PortalId ?? 1;
                    dl.DeliveryTime = use.DeliveryTime;
                    dl.Address = use.Address;
                    dl.Price = use.Price;
                    dl.FilePath = use.FilePath;
                    dl.Extentions = use.Extentions;
                    dl.Pages = use.Pages;
                    dl.StartDate = use.StartDate;
                    dl.Topic = use.Topic;
                    dl.TimeFromTo = use.TimeFromTo;
                    dl.IsHome = use.IsHome;

                    await UpdateLinkAsync(use.ChangeSlug, ESlugType.ContentPage,  dl.Id, dl.Language, MapModel<SeoModel>.Go(use), dl.Name, "", "ContentPage", "Details");

                    dl.CategoryType = categoryMain.CategoryType;

                    _iContentPageRepository.Update(dl);
                    await _iContentPageRepository.CommitAsync();


                    await UpdateCategory(id, use.CategoryIds, use.CategoryId);
                    await UpdateTag(id, use.TagIds);
                    await UpdateRelated(id, use.ContentPageRelatedIds);
                    await UpdateReference(id, use.ContentPageReferenceIds);

                    await _iContentPageRepository.ContentPageSharedAdds(dl.Id, dl.PortalId, use.SharedPortalIds);
                    await _iContentPageRepository.ContentPageSharedRefeshContent(dl.Id);

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật tin tức \"{dl.Name}\".",
                        Type = LogType.Edit
                    });
                    await _iContentPageRepository.CommitTransaction();
                    try
                    {
                        var listCategorys = new List<int>();
                        if (!string.IsNullOrWhiteSpace(use.CategoryIds))
                        {
                            listCategorys = use.CategoryIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
                        }

                        dl.Link = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.ObjectId == dl.Id && x.Type == ESlugType.ContentPage && x.Language == dl.Language && x.PortalId == dl.PortalId);

                        if (dl.NewsId == null || dl.NewsId <= 0)
                        {
                            var outData = await _iAsyncNewsService.CreateAsync(dl, use.TagIds, listCategorys);
                            if (outData != null && outData.Success)
                            {
                                string note = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Tạo tin mới {dl.NewsId} thành công";
                                dl.NewsId = outData.Data.NewsId;
                                dl.Input14 = $"<i title=\"{note}\" class=\"material-icons text-success icon-label-status-syns\">check</i>" ;
                                _iContentPageRepository.Update(dl);
                                await _iContentPageRepository.CommitAsync();
                            }
                            else
                            {
                                string note = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Tạo tin mới của tin #{dl.Id} thất bại: {outData.ErrorMessage}";
                                dl.Input14 = $"<i title=\"{note}\" class=\"material-icons text-danger icon-label-status-syns\">close</i>";
                                _iContentPageRepository.Update(dl);
                                await _iContentPageRepository.CommitAsync();
                            }
                        }
                        else
                        {
                            // Xử lý bên FE oke mới tiến hành đồng bộ tin lên CM
                            var outData = await _iAsyncNewsService.UpdateAsync(dl, use.TagIds, listCategorys);
                            if (outData != null && outData.Success)
                            {
                                string note = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Cập nhật {dl.NewsId} thành công";
                                dl.Input15 = $"<i title=\"{note}\" class=\"material-icons text-success icon-label-status-syns\">check</i>";
                                _iContentPageRepository.Update(dl);
                                await _iContentPageRepository.CommitAsync();
                            }
                            else
                            {
                                string note = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Cập nhật {dl.NewsId} thất bại: {outData.ErrorMessage}";
                                dl.Input15 = $"<i title=\"{note}\" class=\"material-icons text-danger icon-label-status-syns\">close</i>";
                                _iContentPageRepository.Update(dl);
                                await _iContentPageRepository.CommitAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
                    }
                    return new ResponseModel() { Output = 1, Message = "Cập nhật tin tức thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin hoặc liên kết thân thiện/Permalink đã tồn tại, vui lòng thay thêm ký tự bất kỳ đằng sau.", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [EventEdit]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> EventEdit(int id, string language = "vi", int portalId = 1)
        {
            if(id > 0)
            {
                var dl = await _iContentPageRepository.SingleOrDefaultAsync(true, m => m.Id == id);
                if (dl == null)
                {
                    return View("404");
                }

                var model = MapModel<BlogModel>.Go(dl);
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
                var blogTagIds = (await _iContentPageTagRepository.SearchAsync(true, 0, 0, x => x.ContentPageId == id)).Select(x => x.TagId).ToList();
                model.TagSelectList = new MultiSelectList(await _iTagRepository.SearchAsync(true, 0, 20, x => x.Status && x.Language == model.Language && x.PortalId == dl.PortalId, x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Language = x.Language, Status = x.Status }), "Id", "Name");
                model.TagIds = blogTagIds;

                var listRelated = (await _iContentPageRelatedRepository.GetContentPageAsync(id, 0, 0, null, x => x.OrderBy(m => m.DatePosted), x => new ContentPage { Id = x.Id, DatePosted = x.DatePosted, Status = x.Status, Name = x.Name })).Select(x => new { id = x.Id, text = x.Name });
                model.ContentPageRelatedIds = string.Join(',', listRelated.Select(x => x.id));
                model.RelatedString = Newtonsoft.Json.JsonConvert.SerializeObject(listRelated);

                var listReference = (await _iContentPageReferenceRepository.SearchAsync(true, 0, 0, x => x.ContentPageId == id)).Select(x => new ContentPageReferenceModel { ContentPageId = x.ContentPageId, Href = x.Href, Id = x.Id, Name = x.Name, Rel = x.Rel, Stt = 0, Type = 2, Target = x.Target });
                model.ReferenceString = Newtonsoft.Json.JsonConvert.SerializeObject(listReference);

                model.CategoryIds = string.Join(",", (await _iContentPageCategoryRepository.SearchAsync(true, 0, 0, x => x.ContentPageId == id)).Select(x => x.CategoryId));

                var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
                // Đưa danh sách portal vào ViewData để view có thể bind vào SelectList
                model.PortalSelectList = new SelectList(portals, "Id", "Name");
                model.PortalId = dl.PortalId;
                model.PortalName = portals.SingleOrDefault(x => x.Id == dl.PortalId)?.Name;
                var currentShared = await _iContentPageRepository.ContentPageSharedGets(model.Id);
                model.PortalShareds = portals.Where(x => x.Id != model.PortalId).Select(x => new PortalSharedModel { Id = x.Id, Name = x.Name, Selected = false }).ToList();
                foreach (var item in model.PortalShareds)
                {
                    item.Selected = currentShared.Any(x => (x.ParentPortalId == item.Id) || (x.SharedPortalId == item.Id));
                }
                var categorys = await CategorysAsync(model.Language, model.PortalId ?? 1);
                ViewData["CategoryJson"] = Newtonsoft.Json.JsonConvert.SerializeObject(categorys.Select(x => new { x.Id, x.CategoryType, x.SlugType }));
                model.CategorySelectList = await GetPortalSelectList(categorys, model.Language, model.PortalId ?? 1, model.CategoryId);
                model.FullPath = await _iPortalRepository.GetFullPathAsync(model.PortalId ?? 1, model.Slug ?? string.Empty, portals, model.Language, _baseSettings.Value.MultipleLanguage);
                return View("EventEdit", model);
            }
            else
            {
                var dl = new BlogModel
                {
                    Language = language
                };
                ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
                dl.TagSelectList = new MultiSelectList(await _iTagRepository.SearchAsync(true, 0, 20, x => x.Status && x.Language == language && x.PortalId == portalId, x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Language = x.Language, Status = x.Status }), "Id", "Name");
                dl.PortalName = (await _iPortalRepository.SingleOrDefaultAsync(true, x => x.Id == portalId))?.Name;
                dl.PortalId = portalId;
                var categorys = (await CategorysAsync(language, portalId)).Where(x=>x.CategoryType == ECategoryType.ContentPage_Event).ToList();
                ViewData["CategoryJson"] = Newtonsoft.Json.JsonConvert.SerializeObject(categorys.Select(x => new { x.Id, x.CategoryType, x.SlugType }));
                dl.CategorySelectList = await GetPortalSelectList(categorys, language, portalId);
                dl.DatePosted = DateTime.Now;
                dl.Author = "FiinGroup";
                return View("EventEdit", dl);
            }
          
        }

        [HttpPost, ActionName("EventCreate")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EventCreatePost(BlogModel use, string categoryIds, string altId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Lấy ra danh mục chính
                    await _iContentPageRepository.BeginTransaction();
                    var categoryMain = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Id == use.CategoryId);
                    if (categoryMain == null || categoryMain.CategoryType == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Danh mục chính không tồn tại hoặc dữ liệu chưa dc chuẩn hóa, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    var data = new ContentPage
                    {
                        Name = use.Name,
                        Banner = use.Banner,
                        Content = use.Content,
                        Status = use.Status,
                        Language = use.Language,
                        Summary = use.Summary,
                        DatePosted = use.DatePosted,
                        Author = use.Author,
                        PortalId = use.PortalId ?? 1,
                        CategoryId = use.CategoryId,
                        SlugType = ESlugType.ContentPage,
                        CategoryType = categoryMain.CategoryType,
                        DeliveryTime = use.DeliveryTime,
                        Address = use.Address,
                        Price = use.Price,
                        FilePath = use.FilePath,
                        Extentions = use.Extentions,
                        Pages = use.Pages,
                        StartDate = use.StartDate,
                        Topic = use.Topic,
                        TimeFromTo = use.TimeFromTo,
                        IsHome = use.IsHome,
                        Input1 = use.Input1,
                        Input2 = use.Input2,
                        Input3 = use.Input3,
                        Input4 = use.Input4,
                        Input5 = use.Input5,
                        Input6 = use.Input6,
                        Input7 = use.Input7,
                        Input8 = use.Input8,
                        Input9 = use.Input9,
                        Input10 = use.Input10,
                        Input11 = use.Input11,
                        Input12 = use.Input12,
                        Input13 = use.Input13,
                        Input14 = use.Input14,
                        Input15 = use.Input15

                    };
                    data.Content = CompieleContent(data);
                    await _iContentPageRepository.AddAsync(data);
                    await _iContentPageRepository.CommitAsync();

                    var linkId = await CreateLinkAsync(ESlugType.ContentPage, data.Language, data.Id, MapModel<SeoModel>.Go(use), data.Name, "", "ContentPage", "Details", data.PortalId);
                    await UpdateCategory(data.Id, categoryIds, data.CategoryId);
                    await UpdateTag(data.Id, use.TagIds);
                    await UpdateRelated(data.Id, use.ContentPageRelatedIds);
                    await UpdateReference(data.Id, use.ContentPageReferenceIds);
                    await UpdateFileData(data.Id, ESlugType.ContentPage, altId);
                    await _iContentPageRepository.CommitTransaction();
                    try
                    {
                        var listCategorys = new List<int>();
                        if (!string.IsNullOrWhiteSpace(use.CategoryIds))
                        {
                            listCategorys = use.CategoryIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
                        }
                        data.Link = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.Id == linkId);
                        var outData = await _iAsyncNewsService.CreateAsync(data, use.TagIds, listCategorys);
                        if (outData != null && outData.Success)
                        {
                            data.NewsId = outData.Data.NewsId;
                            string note = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Tạo tin mới {data.NewsId} thành công";
                            data.Input14 = $"<i title=\"{note}\" class=\"material-icons text-success icon-label-status-syns\">check</i>";
                            _iContentPageRepository.Update(data);
                            await _iContentPageRepository.CommitAsync();
                        }
                        else
                        {
                            string note = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Tạo tin mới của tin #{data.Id} thất bại: {outData.ErrorMessage}";
                            data.Input14 = $"<i title=\"{note}\" class=\"material-icons text-danger icon-label-status-syns\">close</i>";
                            _iContentPageRepository.Update(data);
                            await _iContentPageRepository.CommitAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
                    }
                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới tin tức \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới tin tức thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin hoặc liên kết thân thiện/Permalink đã tồn tại, vui lòng thay thêm ký tự bất kỳ đằng sau", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }

        [HttpPost, ActionName("EventEdit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EventEditPost(BlogModel use, int id)
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
                    var categoryMain = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Id == use.CategoryId);
                    if (categoryMain == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Danh mục chính không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    dl.Name = use.Name;
                    dl.CategoryId = use.CategoryId;
                    dl.Banner = use.Banner;
                    dl.Status = use.Status;
                    dl.Summary = use.Summary;
                    dl.DatePosted = use.DatePosted;
                    dl.Author = use.Author;
                    dl.PortalId = use.PortalId ?? 1;
                    dl.DeliveryTime = use.DeliveryTime;
                    dl.Address = use.Address;
                    dl.Price = use.Price;
                    dl.FilePath = use.FilePath;
                    dl.Extentions = use.Extentions;
                    dl.Pages = use.Pages;
                    dl.StartDate = use.StartDate;
                    dl.Topic = use.Topic;
                    dl.TimeFromTo = use.TimeFromTo;
                    dl.IsHome = use.IsHome;
                    dl.Input1 = use.Input1;
                    dl.Input2 = use.Input2;
                    dl.Input3 = use.Input3;
                    dl.Input4 = use.Input4;
                    dl.Input5 = use.Input5;
                    dl.Input6 = use.Input6;
                    dl.Input7 = use.Input7;
                    dl.Input8 = use.Input8;
                    dl.Input9 = use.Input9;
                    dl.Input10 = use.Input10;
                    dl.Input11 = use.Input11;
                    dl.Input12 = use.Input12;
                    dl.Input13 = use.Input13;
                    dl.Input14 = use.Input14;
                    dl.Input15 = use.Input15;
                    dl.Content = CompieleContent(dl);
                    await UpdateLinkAsync(use.ChangeSlug, ESlugType.ContentPage, dl.Id, dl.Language, MapModel<SeoModel>.Go(use), dl.Name, "", "ContentPage", "Details");

                    dl.CategoryType = categoryMain.CategoryType;

                    _iContentPageRepository.Update(dl);
                    await _iContentPageRepository.CommitAsync();


                    await UpdateCategory(id, use.CategoryIds, use.CategoryId);
                    await UpdateTag(id, use.TagIds);
                    await UpdateRelated(id, use.ContentPageRelatedIds);
                    await UpdateReference(id, use.ContentPageReferenceIds);

                    await _iContentPageRepository.ContentPageSharedAdds(dl.Id, dl.PortalId, use.SharedPortalIds);
                    await _iContentPageRepository.ContentPageSharedRefeshContent(dl.Id);
                    await _iContentPageRepository.CommitTransaction();
                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật sự kiện \"{dl.Name}\".",
                        Type = LogType.Edit
                    });
           
                    try
                    {
                        var listCategorys = new List<int>();
                        if (!string.IsNullOrWhiteSpace(use.CategoryIds))
                        {
                            listCategorys = use.CategoryIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
                        }

                        dl.Link = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.ObjectId == dl.Id && x.Type == ESlugType.ContentPage && x.Language == dl.Language && x.PortalId == dl.PortalId);

                        if (dl.NewsId == null || dl.NewsId <= 0)
                        {
                            var outData = await _iAsyncNewsService.CreateAsync(dl, use.TagIds, listCategorys);
                            if (outData != null && outData.Success)
                            {
                                string note = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Tạo tin mới {dl.NewsId} thành công";
                                dl.NewsId = outData.Data.NewsId;
                                dl.Input14 = $"<i title=\"{note}\" class=\"material-icons text-success icon-label-status-syns\">check</i>";
                                _iContentPageRepository.Update(dl);
                                await _iContentPageRepository.CommitAsync();
                            
                            }
                            else
                            {
                                string note = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Tạo tin mới của tin #{dl.Id} thất bại: {outData.ErrorMessage}";
                                dl.Input14 = $"<i title=\"{note}\" class=\"material-icons text-danger icon-label-status-syns\">close</i>";
                                _iContentPageRepository.Update(dl);
                                await _iContentPageRepository.CommitAsync();
                            }
                        }
                        else
                        {
                            // Xử lý bên FE oke mới tiến hành đồng bộ tin lên CM
                            var outData = await _iAsyncNewsService.UpdateAsync(dl, use.TagIds, listCategorys);
                            if (outData != null && outData.Success)
                            {
                                string note = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Cập nhật {dl.NewsId} thành công";
                                dl.Input15 = $"<i title=\"{note}\" class=\"material-icons text-success icon-label-status-syns\">check</i>";
                                _iContentPageRepository.Update(dl);
                                await _iContentPageRepository.CommitAsync();
                            }
                            else
                            {
                                string note = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Cập nhật {dl.NewsId} thất bại: {outData.ErrorMessage}";
                                dl.Input15 = $"<i title=\"{note}\" class=\"material-icons text-danger icon-label-status-syns\">close</i>";
                                _iContentPageRepository.Update(dl);
                                await _iContentPageRepository.CommitAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
                    }
                    return new ResponseModel() { Output = 1, Message = "Cập nhật tin tức thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin hoặc liên kết thân thiện/Permalink đã tồn tại, vui lòng thay thêm ký tự bất kỳ đằng sau.", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }

        public string CompieleContent(ContentPage data)
        {
            string section1 = SectionTemplateHelper.RenderSection(data.Input1, data.Input2);
            string section2 = SectionTemplateHelper.RenderSection(data.Input3, data.Input4);
            string section3 = SectionTemplateHelper.RenderSection(data.Input5, data.Input6);
            string section4 = SectionTemplateHelper.RenderSection(data.Input7, data.Input8);
            string section5 = SectionTemplateHelper.RenderSection(data.Input9, data.Input10);
            string section6 = SectionTemplateHelper.RenderSection(data.Input11, data.Input12);
            string outData = $"<main role=\"main\" class=\"new-layout\"><div class=\"event\"><div class=\"new-event\">{section1}{section2}{section3}{section4}{section5}{section6}</div></div></main>";
            return outData;
        }
        #endregion

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
            await _iContentPageCategoryRepository.CommitAsync();
        }

        private async Task UpdateReference(int blogId, string strData)
        {
            try
            {
                var list = new List<ContentPageReferenceModel>();

                if (strData != null && strData != "")
                {
                    list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ContentPageReferenceModel>>(strData);
                }

                var listDelete = list.Where(x => x.Type == 3 && x.Id != 0);
                var listAdd = list.Where(x => x.Type == 1 && x.Id == 0);
                if (listDelete.Any())
                {
                    _iContentPageReferenceRepository.DeleteWhere(x => x.ContentPageId == blogId && listDelete.Select(m => m.Id).Contains(x.Id));
                    await _iContentPageReferenceRepository.CommitAsync();
                }
                if (listAdd.Any())
                {
                    foreach (var item in listAdd)
                    {
                        await _iContentPageReferenceRepository.AddAsync(new ContentPageReference { Name = item.Name, Href = item.Href, ContentPageId = blogId });
                    }
                    await _iContentPageReferenceRepository.CommitAsync();
                }
            }
            catch
            {
            }
        }

        private async Task UpdateCategory(int blogId, string categoryIds, int categoryId)
        {
            var list = new List<int>();
            if (!string.IsNullOrWhiteSpace(categoryIds))
            {
                list = categoryIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }

            // Nếu có category chính (categoryId) thì đảm bảo thêm nó và tất cả các ancestor vào danh sách
            if (categoryId >0)
            {
                try
                {
                    var current = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Id == categoryId);
                    if (current != null)
                    {
                        if (!list.Contains(current.Id)) list.Add(current.Id);
                        // đi lên các cha ông
                        var parentId = current.ParentId;
                        while (parentId >0)
                        {
                            var parent = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Id == parentId);
                            if (parent == null) break;
                            if (!list.Contains(parent.Id)) list.Add(parent.Id);
                            parentId = parent.ParentId;
                        }
                    }
                }
                catch
                {
                    // bỏ qua lỗi khi không lấy được category, tiếp tục với list hiện tại
                }
            }

            var _currentCategory = await _iContentPageCategoryRepository.SearchAsync(true,0,0, x => x.ContentPageId == blogId);
            var idsAdd = list.Where(x => !_currentCategory.Any(y => y.CategoryId == x));
            _iContentPageCategoryRepository.DeleteWhere(x => x.ContentPageId == blogId && !list.Contains(x.CategoryId));
            foreach (var item in idsAdd)
            {
                await _iContentPageCategoryRepository.AddAsync(new ContentPageCategory { ContentPageId = blogId, CategoryId = item });
            }
            await _iContentPageCategoryRepository.CommitAsync();
        }

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

        #region [Delete]
        [HttpPost, ActionName("Delete")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeletePost(int id)
        {
            try
            {
                await _iContentPageRepository.Database().BeginTransactionAsync();
                var kt = await _iContentPageRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null)
                {
                    return new ResponseModel() { Output = 0, Message = "Tin tức không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                _iContentPageRepository.Delete(kt);
                await _iContentPageRepository.CommitAsync();
                await DeleteSeoLink(kt.SlugType ?? ESlugType.ContentPage, kt.Id);
                await RemoveFileData(id, ESlugType.ContentPage);
                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa tin tức \"{kt.Name}\".",
                    Type = LogType.Delete
                });
                // Xóa các liên kết bảng phụ
                _iContentPageCategoryRepository.DeleteWhere(x => x.ContentPageId == id);
                _iContentPageTagRepository.DeleteWhere(x => x.ContentPageId == id);
                _iContentPageRelatedRepository.DeleteWhere(x => x.ParentId == id || x.ContentPageId == id);
                _iContentPageReferenceRepository.DeleteWhere(x => x.ContentPageId == id);
                await _iContentPageRepository.ContentPageSharedDelete(id);
                await _iContentPageRepository.CommitAsync();
                // Xóa đồng bộ 
                var check = await _iAsyncNewsService.DeleteAsync(kt.NewsId ?? 0, null, kt.Language);
                if (!check.Success)
                {
                    await _iContentPageRepository.Database().RollbackTransactionAsync();
                    _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]Xóa đồng bộ tin tức thất bại: {0}", check.ErrorMessage);
                    return new ResponseModel() { Output = 0, Message = "Xóa Tin tức thất bại, không thể xóa tin ở DC.", Type = ResponseTypeMessage.Danger, IsClosePopup = true };
                }
                else
                {
                    await _iContentPageRepository.Database().CommitTransactionAsync();
                    return new ResponseModel() { Output = 1, Message = "Xóa Tin tức thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [Tree Category]

        public async Task<List<TreeRoleModel>> TreeCategory(int id, string language = "vi", int portalId = 1)
        {
            var allowCategorys =  new List<ECategoryType>() { ECategoryType.ContentPage_Blog,  ECategoryType.ContentPage_Publications };
            var listCurent = await _iContentPageCategoryRepository.SearchAsync(true, 0, 0, x => x.ContentPageId == id);
            var listCategory = await _iCategoryRepository.SearchAsync(true, 0, 0, x =>  x.Status && allowCategorys.Contains(x.CategoryType ?? ECategoryType.ContentPage_Blog) && x.Language == language && x.PortalId ==portalId);
            var abc = listCategory.Select(x =>
           new TreeRoleModel
           {
               Id = x.Id.ToString(),
               Parent = x.ParentId == 0 ? "#" : x.ParentId.ToString(),
               Text = x.Name,
               State = new TreeRoleStateModel { Disabled = false, Opened = true, Selected = listCurent.Any(m => m.CategoryId == x.Id && !listCategory.Any(z => z.ParentId == x.Id)) },
               Icon = null
           }
            ).ToList();
            return abc;
        }
        #endregion

        #region [Tags]
        [HttpPost, AuthorizePermission("Index"), ActionName("AddTag")]
        public async Task<object> AddTag(string name, string language, int portalId = 1)
        {
            try
            {
                return await AddTagLink(name, language, portalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
                return null;
            }
        }
        #endregion

        #region [Search Category]
        [HttpGet, Authorize]
        public async Task<object> SearchCategory(string language = "vi")
        {
            var listCategory = await _iCategoryRepository.SearchAsync(true, 0, 0, x => x.Language == language && x.Status && x.Type == CategoryType.CategoryBlog);
            return GenSelectCategory(listCategory, 0, 0);
        }
        [NonAction]
        private List<SelectListItem> GenSelectCategory(List<Category> listCategory, int parentId = 0, int level = 0)
        {
            var list = new List<SelectListItem>();
            foreach (var item in listCategory.Where(x => x.ParentId == parentId).OrderBy(x => x.Order).ToList())
            {
                string spl = System.String.Concat(Enumerable.Repeat("---", level));
                list.Add(new SelectListItem { Text = $"{spl} {item.Name}", Value = item.Id.ToString() });
                list.AddRange(GenSelectCategory(listCategory, item.Id, level + 1));
            }
            return list;
        }
        #endregion

        #region [Select tag]
        [HttpGet, Authorize]
        public async Task<object> SearchTag(int? portalId, string language = "vi")
        {
            return (await _iTagRepository.SearchAsync(true, 0, 0, x => x.Status && x.Language == language && (x.PortalId == portalId || portalId == null), x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Language = x.Language, Status = x.Status })).Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
        }
        #endregion

        [HttpPost, Authorize]
        public async Task<List<SelectListItem>> SearchContentPage(string q, int top = 10, string language = "vi", int portalId = 0)
        {
            top = top > 100 ? 100 : top;
            return (await _iContentPageRepository.SearchAsync(true, 0, top, x => x.Name.ToLower().Contains(q.ToLower()) &&  x.Status && x.SlugType == ESlugType.ContentPage && x.Language == language && x.PortalId == portalId, x => x.OrderBy(y => y.Name),
                x => new ContentPage { Id = x.Id, Name = x.Name,  Status = x.Status, Language = x.Language, Type = x.Type })).Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
        }

        #region [Upload file]
        [HttpPost, ActionName("UploadImage")]
        [AuthorizePermission("Index")]
        public async Task<object> UploadImagePost(string altId, int id, int type =0, int portalId = 1)
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
                    return new ResponseModel<FileDataModel> { Output =0, Message = "Không có tệp được gửi.", Type = ResponseTypeMessage.Warning };

                var ext = Path.GetExtension(file.FileName);
                if (!allowed.Contains(ext))
                    return new ResponseModel<FileDataModel> { Output =2, Message = "Tệp tải lên không đúng định dạng.", Type = ResponseTypeMessage.Warning };

                if (_baseSettings.Value.ImagesMaxSize < file.Length)
                    return new ResponseModel<FileDataModel> { Output =3, Message = "Tệp tải lên vượt quá kích thước cho phép.", Type = ResponseTypeMessage.Warning };

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
                    if (img.Width > _baseSettings.Value.ImageMaxWith)
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

                // Chèn thêm domain vào publicUrl để chia sẻ cho các đơn vị khác
                var portal = await _iPortalRepository.SingleOrDefaultAsync(true, x => x.Id == portalId);
                if (portal != null)
                {
                    var domain = portal.Domain.TrimEnd('/');
                    if(_iWebHostEnvironment.IsDevelopment())
                    {
                        domain = portal.DomainDev;
                    }
                    publicUrl = domain + publicUrl;
                }

                if (type ==1)
                {
                    return new FileDataCKEditerModel
                    {
                        FileName = fileName,
                        Number =200,
                        Uploaded =1,
                        Url = publicUrl
                    };
                }

                return new ResponseModel<FileDataModel>
                {
                    Output =1,
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

        public async Task<List<Category>> CategorysAsync(string language, int portalId)
        {
            // Lấy các loại category có tiền tố Category
            var allowCategorys = _iCategoryRepository.GetCategoryPrefixedTypes();
            // Lấy danh sách category áp dụng cho portal và ngôn ngữ
            return await _iCategoryRepository.SearchAsync(true, 0, 0, x => x.Status && allowCategorys.Contains(x.CategoryType ?? ECategoryType.ContentPage_Blog) && x.Language == language && x.PortalId == portalId);
        }

        public async Task<List<SelectListItem>> GetPortalSelectList(List<Category> listCategory, string language, int portalId, int? selectedValue = null)
        {
            // Sắp xếp và sinh SelectListItem theo cấu trúc cây
            var items = new List<SelectListItem>();

            void AddChildren(int parentId, int level)
            {
                var children = listCategory.Where(x => x.ParentId == parentId).OrderBy(x => x.Order).ToList();
                foreach (var c in children)
                {
                    var prefix = string.Concat(Enumerable.Repeat("-----", level));
                    var text = string.IsNullOrWhiteSpace(prefix) ? c.Name : $"{prefix} {c.Name}";

                    // Kiểm tra xem category hiện tại có con hay không
                    var hasChildren = listCategory.Any(x => x.ParentId == c.Id);

                    items.Add(new SelectListItem
                    {
                        Text = text,
                        Value = c.Id.ToString(),
                        Selected = selectedValue.HasValue && selectedValue.Value == c.Id,
                        // Nếu có con thì disable (không enable), chỉ enable các node lá
                        Disabled = hasChildren
                    });

                    AddChildren(c.Id, level +1);
                }
            }

            AddChildren(0,0);

            return items;
        }
    }
}