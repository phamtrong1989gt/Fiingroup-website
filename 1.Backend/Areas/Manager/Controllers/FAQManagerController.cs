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

namespace PT.UI.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class FAQManagerController : Base.Controllers.BaseController
    {
        private readonly ILogger _logger;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ILinkRepository _iLinkRepository;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly ITagRepository _iTagRepository;
        private readonly IContentPageTagRepository _iContentPageTagRepository;

        public FAQManagerController(
            ILogger<FAQManagerController> logger,
            IContentPageRepository iContentPageRepository,
            ILinkRepository iLinkRepository,
            ICategoryRepository iCategoryRepository,
            ITagRepository iTagRepository,
            IContentPageTagRepository iContentPageTagRepository,
            IOptions<BaseSettings> baseSettings
        )
        {
            controllerName = "FAQManager";
            tableName = "Blog";
            _logger = logger;
            _iContentPageRepository = iContentPageRepository;
            _iLinkRepository = iLinkRepository;
            _iCategoryRepository = iCategoryRepository;
            _iTagRepository = iTagRepository;
            _iContentPageTagRepository = iContentPageTagRepository;
            _baseSettings = baseSettings;
        }

        #region [Index]
        [AuthorizePermission("Index")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, int? categoryId, int? tagId, bool? status, string language = "vi", string ordertype = "asc", string orderby = "name")
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iContentPageRepository.FAQSearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                categoryId,
                tagId,
                m => (m.Name.Contains(key) || key == null || m.Content.Contains(key) || m.Summary.Contains(key)) && m.Type == CategoryType.FAQ &&
                    (m.Language == language) &&
                    (m.Status == status || status == null) &&
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
                    CategoryId = x.CategoryId,
               
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
            return orderby switch
            {
                "name" => ordertype == "asc" ? EntityExtention<ContentPage>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<ContentPage>.OrderBy(m => m.OrderByDescending(x => x.Name)),
                _ => ordertype == "asc" ? EntityExtention<ContentPage>.OrderBy(m => m.OrderBy(x => x.DatePosted)) : EntityExtention<ContentPage>.OrderBy(m => m.OrderByDescending(x => x.DatePosted)),
            };
        }
        #endregion

        #region [Create]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Create(string language = "vi")
        {
            var dl = new FAQModel
            {
                Language = language
            };
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
            var listCategory = await _iCategoryRepository.SearchAsync(true, 0, 0, x => x.Language == language && x.Status && !x.Delete && x.Type == CategoryType.CategoryService);
            dl.CategorySelectList = GenSelectCategory(listCategory, 0, 0);
            dl.TagSelectList = new MultiSelectList(await _iTagRepository.SearchAsync(true, 0, 0, x => x.Status && x.Language == language && !x.Delete, x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Language = x.Language, Status = x.Status, Delete = x.Delete }), "Id", "Name");
            return View(dl);
        }
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(FAQModel use)
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
                        Type = CategoryType.FAQ,
                        Summary = use.Summary,
                        DatePosted = use.DatePosted,
                        Author = use.Author,
                        CategoryId = use.CategoryId,
                        IsHome = use.IsHome
                    };
                    await _iContentPageRepository.AddAsync(data);
                    await _iContentPageRepository.CommitAsync();

                    await AddSeoLink(CategoryType.FAQ, data.Language, data.Id, MapModel<SeoModel>.Go(use), data.Name, "", "ContentPageHome", "Details");

                    await UpdateTag(data.Id, use.TagIds);

                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới câu hỏi thường gặp \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới câu hỏi thường gặp thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
            if (dl == null || (dl != null && dl.Delete && dl.Type == CategoryType.FAQ))
            {
                return View("404");
            }
            var model = MapModel<FAQModel>.Go(dl);
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{dl.Language}" : "";
            var ktLink = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.ObjectId == id && x.Type == CategoryType.FAQ);
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
            var listCategory = await _iCategoryRepository.SearchAsync(true, 0, 0, x => x.Language == dl.Language && x.Status && !x.Delete && x.Type == CategoryType.CategoryService);

            model.TagSelectList = new MultiSelectList(await _iTagRepository.SearchAsync(true, 0, 0, x => x.Status && x.Language == model.Language && !x.Delete, x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Language = x.Language, Status = x.Status, Delete = x.Delete }), "Id", "Name");

            model.TagIds = blogTagIds;
            model.CategorySelectList = GenSelectCategory(listCategory, 0, 0);
            return View(model);
        }
        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(FAQModel use, int id)
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
                    dl.DatePosted = use.DatePosted;
                    dl.CategoryId = use.CategoryId;
                    dl.Author = use.Author;
                    dl.IsHome = use.IsHome;
                    _iContentPageRepository.Update(dl);
                    await _iContentPageRepository.CommitAsync();

                    await UpdateSeoLink(use.ChangeSlug, CategoryType.FAQ, dl.Id, dl.Language, MapModel<SeoModel>.Go(use), dl.Name, "", "ContentPageHome", "Details");

                    await UpdateTag(id, use.TagIds);

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật câu hỏi thường gặp \"{dl.Name}\".",
                        Type = LogType.Edit
                    });

                    return new ResponseModel() { Output = 1, Message = "Cập nhật câu hỏi thường gặp thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
                if (kt == null || (kt != null && kt.Delete && kt.Type == CategoryType.FAQ))
                {
                    return new ResponseModel() { Output = 0, Message = "câu hỏi thường gặp không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                kt.Delete = true;
                await _iContentPageRepository.CommitAsync();
                await DeleteSeoLink(CategoryType.Blog, kt.Id);
                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa câu hỏi thường gặp \"{kt.Name}\".",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa câu hỏi thường gặp thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
    }
}