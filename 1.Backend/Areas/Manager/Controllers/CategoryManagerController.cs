using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.Options;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;
using PT.Base;
using System.Text;
using System.Net.Http.Headers;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using PT.BE.Areas.Manager.Controllers;

namespace PT.BE.Areas.User.Controllers
{
    [Area("Manager")]
    public class CategoryManagerController : Base.Controllers.BaseController
    {
        private readonly ILogger _logger;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly ILinkRepository _iLinkRepository;
        private readonly IWebHostEnvironment _iWebHostEnvironment;
        private readonly IFileRepository _iFileRepository;
        public CategoryManagerController(
            IOptions<BaseSettings> baseSettings,
            ILogger<CategoryManagerController> logger,
            ICategoryRepository iCategoryRepository,
            ILinkRepository iLinkRepository,
            IWebHostEnvironment iWebHostEnvironment,
            IFileRepository iFileRepository

            )
        {
            _logger = logger;
            _baseSettings = baseSettings;
            _iCategoryRepository = iCategoryRepository;
            _iLinkRepository = iLinkRepository;
            _iWebHostEnvironment = iWebHostEnvironment;
            _iFileRepository = iFileRepository;
        }

        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Index()
        {
            return View();
        }

        #region [Create]
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Create(string language = "vi", int parrentId = 0)
        {
            var dl = new CategoryModel
            {
                Language = language,
                ParentId = parrentId,
                Type = CategoryType.CategoryBlog
            };
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
            return View(dl);
        }
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(CategoryModel use, string altId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var maxOrder = _iCategoryRepository.MaxOrder(x => x.ParentId == 0);
                    var data = new Category
                    {
                        Name = use.Name,
                        Banner = use.Banner,
                        Content = use.Content,
                        Delete = false,
                        Status = use.Status,
                        Language = use.Language,
                        Order = maxOrder + 1,
                        ParentId = 0,
                        Summary = use.Summary,
                        Type = use.Type
                    };

                    await _iCategoryRepository.AddAsync(data);
                    await _iCategoryRepository.CommitAsync();

                    await AddSeoLink(data.Type, data.Language, data.Id, MapModel<SeoModel>.Go(use), data.Name, "", "CategoryHome", "Details");
                    await UpdateFileData(data.Id, data.Type, altId);
                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới danh mục tin tức \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới danh mục thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
            var dl = await _iCategoryRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null || dl.Delete == true)
            {
                return View("404");
            }
            var model = MapModel<CategoryModel>.Go(dl);
            var ktLink = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.ObjectId == id);
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
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{dl.Language}" : "";
            return View(model);
        }
        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(CategoryModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iCategoryRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }
                    dl.Name = use.Name;
                    dl.Banner = use.Banner;
                    dl.Content = use.Content;
                    dl.Status = use.Status;
                    dl.Summary = use.Summary;
                    _iCategoryRepository.Update(dl);
                    await _iCategoryRepository.CommitAsync();

                    await UpdateSeoLink(use.ChangeSlug, dl.Type, dl.Id, dl.Language, MapModel<SeoModel>.Go(use), dl.Name, "", "CategoryHome", "Details");

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật danh mục tin tức \"{dl.Name}\".",
                        Type = LogType.Edit
                    });

                    return new ResponseModel() { Output = 1, Message = "Cập nhật danh mục thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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

        #region [Delete]
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeletePost(int id)
        {
            try
            {
                var kt = await _iCategoryRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null)
                {
                    return new ResponseModel() { Output = 0, Message = "Danh mục không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                _iCategoryRepository.Delete(kt);
                await _iCategoryRepository.CommitAsync();
                await DeleteSeoLink(kt.Type, kt.Id);
                await RemoveFileData(id, kt.Type);
                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa danh mục  tin tức \"{kt.Name}\".",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa danh mục thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [Setting]
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Setting(string language = "vi")
        {
            return View(nameof(Setting), language);
        }
        [HttpGet]
        public async Task<string> Categorys(string language)
        {
            var list = await _iCategoryRepository.FindByLinkReference(0, 0, x => x.Language == language && !x.Delete);
            return ShowTree(list, 0);
        }
        [HttpPost, ActionName("Setting")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> SettingPost([FromBody]string data)
        {
            try

            {
                var listItem = ConverData(Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataSortModel>>(data));
                var listItemIds = listItem.Select(x => x.Id).ToList();
                var listCa = await _iCategoryRepository.SearchAsync(false, 0, 0, x => listItemIds.Contains(x.Id));
                foreach (var item in listCa)
                {
                    var objIn = listItem.FirstOrDefault(x => x.Id == item.Id);
                    item.ParentId = objIn == null ? 0 : objIn.ParentId;
                    item.Order = objIn.Order;
                    _iCategoryRepository.Update(item);
                }
                await _iCategoryRepository.CommitAsync();
                return new ResponseModel() { Output = 1, Message = "Cập nhật thành công.", Type = ResponseTypeMessage.Success };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        private List<DataSortModel> ConverData(List<DataSortModel> list, int parentId = 0)
        {
            int order = 1;
            var newList = new List<DataSortModel>();
            foreach (var item in list)
            {
                item.ParentId = parentId;
                item.Order = order;
                newList.Add(item);
                if (item.Children.Count() > 0)
                {
                    newList.AddRange(ConverData(item.Children, item.Id));
                }
                order++;
            }
            return newList;
        }

        private string ShowTree(List<Category> list, int parrentId)
        {
            var listCulture = Shared.ListData.ListLanguage;
            System.Text.StringBuilder str = new System.Text.StringBuilder();
            var listitem = list.Where(m => m.ParentId == parrentId).OrderBy(m => m.Order);
            if (listitem.Count() > 0)
            {

                if (parrentId == 0)
                {
                    str.Append($"<ol class=\"dd-list\">");
                }
                else
                {
                    str.Append($"<ol class=\"dd-list\">");
                }

                foreach (var item in listitem.OrderBy(x => x.Order))
                {
                    if (item.Id == parrentId)
                    {
                        return "";
                    }
                    str.Append($"<li class=\"dd-item {(item.Status ? "treeTrue" : "")} dd3-item\" data-id=\"{item.Id}\">");
                    str.Append($"<div class=\"dd-handle dd3-handle\"></div>");
                    str.Append($"<div class=\"dd3-content\">");
                    str.Append($"<span  class='label label-info' style='margin-right: 10px;'>{item.Type.GetDisplayName()}</span> ");
                    //
                    str.Append($"{item.Name}");
                    str.Append($"<span class='can-span-category'>{BindReferenLanguage(new Tuple<List<LinkReference>, string, string, bool>(item.LinkReferences, item.Language, Url.Action("Edit", new { id = "#id#" }), false))}</span>");
                    str.Append($"<span class=\"button-icon\"><a title='Đến trang' target='_blank' href=\"{Functions.FormatUrl(item.Language, item.Link?.Slug)}\" title=\"Lấy đường dẫn\"><i class=\"material-icons iconcontrol text-info\">link</i></a><a onclick=\"onCopyClipboard('{Functions.FormatUrl(item.Language, item.Link?.Slug)}')\" title=\"Lấy đường dẫn\"><i class=\"material-icons iconcontrol text-danger\">share</i></a>");
                    str.Append($"<a button-popup  data-target=\"#myModalBig\" href='{Url.Action("Edit", new { id = item.Id, language = item.Language })}'  title=\"Cập nhật\"><i class=\"material-icons iconcontrol text-primary\">edit</i></a><span>");
                    str.Append("</div>");
                    str.Append(ShowTree(list, item.Id));
                    str.Append("</li>");
                }
                str.Append($"</ol>");
            }
            return str.ToString();
        }
        private string BindReferenLanguage(Tuple<List<LinkReference>, string, string, bool> model)
        {
            if (_baseSettings.Value.MultipleLanguage)
            {
                var str = new StringBuilder();

                foreach (var item in PT.Shared.ListData.ListLanguage)
                {
                    var dl = model.Item1.FirstOrDefault(x => x.Language == item.Id);

                    if (item.Id == model.Item2)
                    {
                        str.Append($"<span class=\"label label-category label-info\"><i class=\"material-icons icon-label\">check</i>{item.Name}</span>");
                    }
                    else if (dl == null || (dl != null && dl.Link2 == null))
                    {
                        str.Append($"<span class=\"label label-category label-default\"><i class=\"material-icons icon-label\">block</i>{item.Name}</span>");
                    }
                    else if (dl != null && dl.Link2 != null && dl.Link2?.Delete == false)
                    {
                        string url = model.Item4 ? model.Item3.Replace("%23id%23", dl.LinkId2.ToString()) : model.Item3.Replace("%23id%23", dl.Link2?.ObjectId.ToString());
                        str.Append($"<a button-popup data-target=\"#myModalBig\" href=\"{url}\" title=\"Cập nhật\" class=\"link-edit-ngon-ngu\"><span class=\"label label-category  {(dl == null ? "label-default" : "label-success")}\"><i class=\"material-icons icon-label\">{(dl == null ? "block" : "check")}</i>{item.Name }</span></a>");
                    }
                    else
                    {
                        str.Append($"<span class=\"label label-default label-category\"><i class=\"material-icons icon-label\">block</i>{item.Name}</span>");
                    }
                }
                return str.ToString();
            }
            else
            {
                return "";
            }
        }


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

                        await AddFileData(id, pathServer, CategoryType.CategoryBlog, altId);

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