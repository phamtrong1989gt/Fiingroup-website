using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System.Linq;
using PT.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using PT.Base;
using System.IO;
using System.Net.Http.Headers;

namespace PT.UI.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class PageManagerController : Base.Controllers.BaseController
    {
        private readonly ILogger _logger;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ILinkRepository _iLinkRepository;
        private readonly ITagRepository _iTagRepository;
        private readonly IContentPageTagRepository _iContentPageTagRepository;
        private readonly IWebHostEnvironment _iWebHostEnvironment;
        private readonly IFileRepository _iFileRepository;

        public PageManagerController(
            ILogger<PageManagerController> logger,
            IOptions<BaseSettings> baseSettings,
            IContentPageRepository iContentPageRepository,
            ILinkRepository iLinkRepository,
            ITagRepository iTagRepository,
            IContentPageTagRepository iContentPageTagRepository,
            IWebHostEnvironment iWebHostEnvironment,
            IFileRepository iFileRepository
        )
        {
            controllerName = "PageManager";
            tableName = "Blog";
            _logger = logger;
            _baseSettings = baseSettings;
            _iContentPageRepository = iContentPageRepository;
            _iLinkRepository = iLinkRepository;
            _iTagRepository = iTagRepository;
            _iContentPageTagRepository = iContentPageTagRepository;
            _iWebHostEnvironment = iWebHostEnvironment;
            _iFileRepository = iFileRepository;
        }

        #region [Index]
        [AuthorizePermission]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, int? categoryId, int? tagId, bool? status, string language ="vi", string ordertype = "asc", string orderby = "name")
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
                        m.Type==CategoryType.Page &&
                        !m.Delete,
                OrderByExtention(ordertype, orderby), 
                x=> new ContentPage {
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
                data.ReturnUrl =  Url.Action("Index", 
                    new {
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
                    functionOrder = ordertype == "asc" ? EntityExtention<ContentPage>.OrderBy(m => m.OrderBy(x => x.DatePosted)) : EntityExtention<ContentPage>.OrderBy(m => m.OrderByDescending(x => x.DatePosted));
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
            var dl = new PageModel
            {
                Language = language
            };
            dl.TagSelectList = new MultiSelectList(await _iTagRepository.SearchAsync(true, 0, 0, x => x.Status && x.Language == language && !x.Delete, x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Language = x.Language, Status = x.Status, Delete = x.Delete }), "Id", "Name");
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
            return View(dl);
        }

        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(PageModel use, string altId)
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
                        Type = CategoryType.Page,
                        Summary = use.Summary,
                        DatePosted =  DateTime.Now
                    };
                    await _iContentPageRepository.AddAsync(data);
                    await _iContentPageRepository.CommitAsync();

                     await AddSeoLink(CategoryType.Page, data.Language, data.Id, MapModel<SeoModel>.Go(use), data.Name, "", "ContentPageHome", "Details");

                    await UpdateTag(data.Id, use.TagIds);
                    await UpdateFileData(data.Id, CategoryType.Page, altId);

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
            if (dl == null || (dl != null && dl.Delete && dl.Type==CategoryType.Page))
            {
                return View("404");
            }
            var model = MapModel<PageModel>.Go(dl);
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{dl.Language}" : "";
            var ktLink = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.ObjectId == id && x.Type == CategoryType.Page);
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
            model.TagSelectList = new MultiSelectList(await _iTagRepository.SearchAsync(true, 0, 0, x => x.Status && x.Language == model.Language && !x.Delete, x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Language = x.Language, Status = x.Status, Delete = x.Delete }), "Id", "Name");
            model.TagIds = blogTagIds;
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

                    _iContentPageRepository.Update(dl);
                    await _iContentPageRepository.CommitAsync();

                    await UpdateSeoLink(use.ChangeSlug, CategoryType.Page, dl.Id, dl.Language, MapModel<SeoModel>.Go(use),dl.Name, "", "ContentPageHome", "Details");

                    await UpdateTag(id, use.TagIds);
                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật Trang nội dung \"{dl.Name}\".",
                        Type = LogType.Edit
                    });

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
                var kt = await _iContentPageRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null || (kt != null && kt.Delete && kt.Type==CategoryType.Page))
                {
                    return new ResponseModel() { Output = 0, Message = "Trang nội dung không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                kt.Delete = true;
                await _iContentPageRepository.CommitAsync();

                await DeleteSeoLink(CategoryType.Page, kt.Id);
                await RemoveFileData(id, CategoryType.Page);
                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa Trang nội dung \"{kt.Name}\".",
                    Type = LogType.Delete
                });

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
            return (await _iTagRepository.SearchAsync(true, 0, 0, x => x.Status && x.Language == language && !x.Delete, x => x.OrderBy(m => m.Name), x => new Tag { Id = x.Id, Name = x.Name, Language = x.Language, Status = x.Status, Delete = x.Delete })).Select(x=> new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
        }
        #endregion

        #region [Upload file]
        [HttpPost, ActionName("UploadImage")]
        [AuthorizePermission("Index")]
        public async Task<object> UploadImagePost(string altId, int id, int type = 0)
        {
            try
            {
                string[] allowedExtensions = _baseSettings.Value.ImagesType.Split(',');
                string path = $"{_iWebHostEnvironment.WebRootPath}/Data" + Functions.GenFolderByDate();
                string pathServer = $"/Data" + Functions.GenFolderByDate();

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var files = Request.Form.Files;
                foreach (var file in files)
                {
                    if (!allowedExtensions.Contains(Path.GetExtension(file.FileName)))
                    {
                        return new ResponseModel<FileDataModel>() { Output = 2, Message = "Tệp tải lên không đúng định dạng.", Type = ResponseTypeMessage.Warning };
                    }
                    else if (_baseSettings.Value.ImagesMaxSize < file.Length)
                    {
                        return new ResponseModel<FileDataModel>() { Output = 3, Message = "Tệp tải lên vượt quá kích thước cho phép.", Type = ResponseTypeMessage.Warning };
                    }
                    else
                    {
                        var newFilename = Path.GetFileName(file.FileName);
                        if (System.IO.File.Exists(path + file.Name))
                        {
                            newFilename = $"{Path.GetFileName(file.FileName)}_{id}_{DateTime.Now:yyyyMMddHHmmss}";
                        }

                        string pathFile = ContentDispositionHeaderValue
                        .Parse(file.ContentDisposition)
                        .FileName
                        .Trim('"');

                        pathFile = $"{path}{newFilename}";
                        pathServer = $"{pathServer}{newFilename}";

                        using var image = System.Drawing.Image.FromStream(file.OpenReadStream());
                        if (image.Width > _baseSettings.Value.ImageMaxWith)
                        {
                            _iFileRepository.ResizeImage(file, pathFile, _baseSettings.Value.ImageMaxWith, false);
                        }
                        else
                        {
                            using var stream = new FileStream(pathFile, FileMode.Create);
                            await file.CopyToAsync(stream);
                        }

                        await AddFileData(id, pathServer, CategoryType.Page, altId);

                        if (type == 1)
                        {
                            return new FileDataCKEditerModel()
                            {
                                FileName = Path.GetFileName(pathServer),
                                Number = 200,
                                Uploaded = 1,
                                Url = pathServer
                            };
                        }
                        else
                        {
                            return new ResponseModel<FileDataModel>()
                            {
                                Output = 1,
                                Message = "Tải tệp lên thành công.",
                                Type = ResponseTypeMessage.Success,
                                Data = new FileDataModel
                                {
                                    CreatedDate = DateTime.Now,
                                    CreatedUser = DataUserInfo.UserId,
                                    Path = pathServer,
                                    FileName = Path.GetFileName(pathServer)
                                },
                                IsClosePopup = false
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel<FileDataModel>() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion
    }
}