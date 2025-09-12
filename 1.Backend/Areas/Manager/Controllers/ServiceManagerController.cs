using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System.Linq;
using PT.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using PT.Base;

namespace PT.BE.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class ServiceManagerController : Base.Controllers.BaseController
    {
        private readonly ILogger _logger;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ILinkRepository _iLinkRepository;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly ITagRepository _iTagRepository;
        private readonly IContentPageCategoryRepository _iContentPageCategoryRepository;
        private readonly IContentPageTagRepository _iContentPageTagRepository;
        private readonly IContentPageReferenceRepository _iContentPageReferenceRepository;
        private readonly IContentPageRelatedRepository _iContentPageRelatedRepository;

        public ServiceManagerController(
            ILogger<BlogManagerController> logger,
            IOptions<BaseSettings> baseSettings,
            IContentPageRepository iContentPageRepository,
            ILinkRepository iLinkRepository,
            ICategoryRepository iCategoryRepository,
            ITagRepository iTagRepository,
            IContentPageCategoryRepository iContentPageCategoryRepository,
            IContentPageTagRepository iContentPageTagRepository,
            IContentPageReferenceRepository iContentPageReferenceRepository,
            IContentPageRelatedRepository iContentPageRelatedRepository
        )
        {
            controllerName = "ServiceManager";
            tableName = "Blog";
            _logger = logger;
            _baseSettings = baseSettings;
            _iContentPageRepository = iContentPageRepository;
            _iLinkRepository = iLinkRepository;
            _iCategoryRepository = iCategoryRepository;
            _iTagRepository = iTagRepository;
            _iContentPageCategoryRepository = iContentPageCategoryRepository;
            _iContentPageTagRepository = iContentPageTagRepository;
            _iContentPageReferenceRepository = iContentPageReferenceRepository;
            _iContentPageRelatedRepository = iContentPageRelatedRepository;
        }

        #region [Index]
        [AuthorizePermission]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, int? categoryId, int? tagId, bool? status, string language = "vi", string ordertype = "asc", string orderby = "name")
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iContentPageRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                categoryId,
                tagId,
                    m => (m.Name.Contains(key) || key == null || m.Content.Contains(key) || m.Summary.Contains(key)) &&
                        (m.Language == language) &&
                        (m.Status == status || status == null) &&
                        m.Type == CategoryType.Service &&
                        !m.Delete,
                        OrderByExtention(ordertype, orderby), x => new ContentPage
                        {
                            Category = x.Category,
                            Id = x.Id,
                            Author = x.Author,
                            Banner = x.Banner,
                            DatePosted = x.DatePosted,
                            Delete = x.Delete,
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
                            IsHome = x.IsHome
                        });
            data.ReturnUrl = Url.Action("Index",
        new
        {
            page,
            limit,
            key,
            categoryId,
            tagId,
            status,
            ordertype,
            orderby
        });
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
                    functionOrder = ordertype == "asc" ? EntityExtention<ContentPage>.OrderBy(m => m.OrderBy(x => x.Id)) : EntityExtention<ContentPage>.OrderBy(m => m.OrderByDescending(x => x.Id));
                    break;
            }
            return functionOrder;
        }
        #endregion

        #region [Create]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Create(string language = "vi")
        {
            var dl = new ServiceModel
            {
                Language = language,
                TagSelectList = new MultiSelectList(await _iTagRepository.SearchAsync(true, 0, 0, x => x.Status && x.Language == language && !x.Delete, x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Language = x.Language, Status = x.Status, Delete = x.Delete }), "Id", "Name")
            };
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
            return View(dl);
        }
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(ServiceModel use, string categoryIds)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var data = new ContentPage
                    {
                        Name = use.Name,
                        Banner = use.Banner,
                        Content = use.Content,
                        Delete = false,
                        Status = use.Status,
                        Language = use.Language,
                        Type = CategoryType.Service,
                        Summary = use.Summary,
                        DatePosted = DateTime.Now,
                        Price = use.Price
                    };
                    await _iContentPageRepository.AddAsync(data);
                    await _iContentPageRepository.CommitAsync();

                    await AddSeoLink(CategoryType.Service, data.Language, data.Id, MapModel<SeoModel>.Go(use),data.Name, "", "ContentPageHome", "Details");

                    await UpdateCategory(data.Id, categoryIds);
                    await UpdateTag(data.Id, use.TagIds);
                    await UpdateReference(data.Id, use.ContentPageReferenceIds);
                    await UpdateRelated(data.Id, use.ContentPageRelatedIds);
                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới Dịch vụ \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới Dịch vụ thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
            if (dl == null || (dl != null && dl.Delete && dl.Type == CategoryType.Service))
            {
                return View("404");
            }
            var model = MapModel<ServiceModel>.Go(dl);
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{dl.Language}" : "";

            var ktLink = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.ObjectId == id && x.Type == CategoryType.Service);
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
            model.TagSelectList = new MultiSelectList(await _iTagRepository.SearchAsync(true, 0, 0, x => x.Status && x.Language == model.Language && !x.Delete, x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Language = x.Language, Status = x.Status, Delete = x.Delete }), "Id", "Name");
            model.TagIds = blogTagIds;
            model.CategoryIds = string.Join(",",(await _iContentPageCategoryRepository.SearchAsync(true, 0, 0, x => x.ContentPageId == id)).Select(x=>x.CategoryId));

            var listReference = (await _iContentPageReferenceRepository.SearchAsync(true, 0, 0, x => x.ContentPageId == id)).Select(x => new ContentPageReferenceModel { ContentPageId = x.ContentPageId, Href = x.Href, Id = x.Id, Name = x.Name, Rel = x.Rel, Stt = 0, Type = 2, Target = x.Target });
            model.ReferenceString = Newtonsoft.Json.JsonConvert.SerializeObject(listReference);

            var listRelated = (await _iContentPageRelatedRepository.GetContentPageAsync(id, 0, 0, x => !x.Delete, x => x.OrderBy(m => m.DatePosted), x => new ContentPage { Id = x.Id, DatePosted = x.DatePosted, Delete = x.Delete, Status = x.Status, Name = x.Name })).Select(x => new { id = x.Id, text = x.Name });
            model.ContentPageRelatedIds = string.Join(',', listRelated.Select(x => x.id));
            model.RelatedString = Newtonsoft.Json.JsonConvert.SerializeObject(listRelated);

            return View(model);
        }
        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(ServiceModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iContentPageRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null || (dl != null && dl.Delete))
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    dl.Name = use.Name;
                    dl.Banner = use.Banner;
                    dl.Content = use.Content;
                    dl.Status = use.Status;
                    dl.Summary = use.Summary;
                    dl.Price = use.Price;
                    _iContentPageRepository.Update(dl);
                    await _iContentPageRepository.CommitAsync();

                    await UpdateSeoLink(use.ChangeSlug, CategoryType.Service, dl.Id, dl.Language, MapModel<SeoModel>.Go(use), dl.Name, "", "ContentPageHome", "Details");

                    await UpdateCategory(id, use.CategoryIds);
                    await UpdateReference(dl.Id, use.ContentPageReferenceIds);
                    await UpdateTag(id, use.TagIds);
                    await UpdateRelated(id, use.ContentPageRelatedIds);
                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật Dịch vụ \"{dl.Name}\".",
                        Type = LogType.Edit
                    });

                    return new ResponseModel() { Output = 1, Message = "Cập nhật Dịch vụ thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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

        private async Task UpdateCategory(int blogId, string categoryIds)
        {
            var list = new List<int>();
            if (categoryIds != null)
            {
                list = categoryIds.Split(',').Select(x => int.Parse(x)).ToList();
            }
            var _currentCategory = await _iContentPageCategoryRepository.SearchAsync(true, 0, 0, x => x.ContentPageId == blogId);
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
                var kt = await _iContentPageRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null || (kt != null && kt.Delete && kt.Type == CategoryType.Service))
                {
                    return new ResponseModel() { Output = 0, Message = "Dịch vụ không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                kt.Delete = true;
                await _iContentPageRepository.CommitAsync();
                await DeleteSeoLink(CategoryType.Service, kt.Id);
                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa Dịch vụ \"{kt.Name}\".",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa dịch vụ thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [Tree Category]
        [HttpPost, Authorize]
        public async Task<List<TreeRoleModel>> TreeCategory(int id, string language = "vi")
        {
            var listCurent = await _iContentPageCategoryRepository.SearchAsync(true, 0, 0, x => x.ContentPageId == id);
            var listCategory = await _iCategoryRepository.SearchAsync(true, 0, 0, x => !x.Delete && x.Status && x.Type == CategoryType.CategoryService && x.Language == language);
            var abc = listCategory.Select(x =>
           new TreeRoleModel
           {
               Id = x.Id.ToString(),
               Parent = x.ParentId == 0 ? "#" : x.ParentId.ToString(),
               Text = x.Name,
               State = new TreeRoleStateModel { Disabled = false, Opened = true, Selected = listCurent.Any(m => m.CategoryId == x.Id && !listCategory.Any(z=> z.ParentId==x.Id)) },
               Icon = null
           }
            ).ToList();
            return abc;
        }
        #endregion

        #region [List Tag]
        [HttpPost, Authorize]
        public async Task<object> SearchTag(string q, int top = 20, string languegu = "vi", bool? isSearch = false)
        {
            top = top > 100 ? 20 : top;
            var listData = (await _iTagRepository.SearchAsync(true, 0, top, x => (x.Name.Contains(q)) && !x.Delete && x.Status && x.Language == languegu, x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Delete = x.Delete, Status = x.Status })).Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
            var list = new List<SelectListItem>();
            if (isSearch == true)
            {
                list.Add(new SelectListItem { Text = "Tất cả", Value = "" });
            }
            list.AddRange(listData);
            return list;
        }
        #endregion

        #region [Search Category]
        [HttpGet, Authorize]
        public async Task<object> SearchCategory(string language = "vi")
        {
            var listCategory = await _iCategoryRepository.SearchAsync(true, 0, 0, x => x.Language == language && x.Status && !x.Delete && x.Type == CategoryType.CategoryService);
            return GenSelectCategory(listCategory, 0, 0);
        }
        [NonAction]
        private List<SelectListItem> GenSelectCategory(List<Category> listCategory, int parentId = 0, int level = 0)
        {
            var list = new List<SelectListItem>();
            foreach (var item in listCategory.Where(x => x.ParentId == parentId).OrderBy(x => x.Order).ToList())
            {
                string spl = String.Concat(Enumerable.Repeat("---", level));
                list.Add(new SelectListItem { Text = $"{spl} {item.Name}", Value = item.Id.ToString() });
                list.AddRange(GenSelectCategory(listCategory, item.Id, level + 1));
            }
            return list;
        }
        #endregion

         #region [Select tag]
        [HttpGet, Authorize]
        public async Task<object> SearchTag(string language = "vi")
        {
            return (await _iTagRepository.SearchAsync(true, 0, 0, x => x.Status && x.Language == language && !x.Delete, x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Language = x.Language, Status = x.Status, Delete = x.Delete })).Select(x=> new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
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
                if (listDelete.Count() > 0)
                {
                    _iContentPageReferenceRepository.DeleteWhere(x => x.ContentPageId == blogId && listDelete.Select(m => m.Id).Contains(x.Id));
                    await _iContentPageReferenceRepository.CommitAsync();
                }
                if (listAdd.Count() > 0)
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

        [HttpPost, Authorize]
        public async Task<List<SelectListItem>> SearchContentPage(string q, int top = 10, string language = "vi")
        {
            top = top > 100 ? 100 : top;
            return (await _iContentPageRepository.SearchAsync(true, 0, top, x => x.Name.ToLower().Contains(q.ToLower()) && !x.Delete && x.Status && (x.Type == CategoryType.Blog || x.Type== CategoryType.Service) && x.Language == language, x => x.OrderBy(y => y.Name),
                x => new ContentPage { Id = x.Id, Name = x.Name, Delete = x.Delete, Status = x.Status, Language = x.Language, Type = x.Type })).Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
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
            await _iContentPageCategoryRepository.CommitAsync();
        }
    }
}