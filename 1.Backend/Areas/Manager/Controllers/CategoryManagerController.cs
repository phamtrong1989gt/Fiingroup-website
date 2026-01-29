using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Base;
using PT.Base.Services;
using PT.BE.Areas.Manager.Controllers;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Infrastructure.Repositories;
using PT.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PT.BE.Areas.User.Controllers
{
    /// <summary>
    /// Controller quản lý danh mục (Category) trong khu vực Quản trị (Area = Manager).
    /// 
    /// Mục đích:
    /// - Cung cấp các action cho quản lý danh mục: danh sách, tạo, sửa, xóa, sắp xếp.
    /// - Hỗ trợ upload ảnh, quản lý file liên quan và tích hợp SEO (link, slug, sitemap).
    /// - Hỗ trợ phân biệt dữ liệu theo Portal và Language.
    /// 
    /// Lưu ý cho người bảo trì:
    /// - Hầu hết action trả về kiểu chuẩn hóa ResponseModel để frontend xử lý popup/hiện thông báo.
    /// - Những thao tác thay đổi dữ liệu cần gọi CommitAsync và cập nhật SEO / File liên quan.
    /// - Hàm helper ở cuối file (ShowTree, ConverData, BindReferenLanguage) sinh HTML động dùng trong UI dạng cây.
    /// </summary>
    [Area("Manager")]
    public class CategoryManagerController : Base.Controllers.BaseController
    {
        private readonly ILogger _logger;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly ILinkRepository _iLinkRepository;
        private readonly IWebHostEnvironment _iWebHostEnvironment;
        private readonly IFileRepository _iFileRepository;
        private readonly IPortalRepository _iPortalRepository;
        private readonly IContentPageCategoryRepository _iContentPageCategoryRepository;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly IAsyncNewsService _iAsyncNewsService;
        /// <summary>
        /// Hàm khởi tạo controller, inject các repository và service cần thiết.
        /// </summary>
        /// <param name="baseSettings">Cấu hình hệ thống</param>
        /// <param name="logger">Logger</param>
        /// <param name="iCategoryRepository">Repository Category</param>
        /// <param name="iLinkRepository">Repository Link (SEO)</param>
        /// <param name="iWebHostEnvironment">IWebHostEnvironment dùng để upload file</param>
        /// <param name="iFileRepository">Repository quản lý file (resize, lưu)</param>
        /// <param name="iPortalRepository">Repository Portal (multi-portal)</param>
        public CategoryManagerController(
            IOptions<BaseSettings> baseSettings,
            ILogger<CategoryManagerController> logger,
            ICategoryRepository iCategoryRepository,
            ILinkRepository iLinkRepository,
            IWebHostEnvironment iWebHostEnvironment,
            IFileRepository iFileRepository,
            IPortalRepository iPortalRepository,
            IContentPageCategoryRepository iContentPageCategoryRepository,
            IContentPageRepository iContentPageRepository,
            IAsyncNewsService iAsyncNewsService
            )
        {
            _logger = logger;
            _baseSettings = baseSettings;
            _iCategoryRepository = iCategoryRepository;
            _iLinkRepository = iLinkRepository;
            _iWebHostEnvironment = iWebHostEnvironment;
            _iFileRepository = iFileRepository;
            _iPortalRepository = iPortalRepository;
            _iAsyncNewsService = iAsyncNewsService;
            _iContentPageCategoryRepository = iContentPageCategoryRepository;
            _iContentPageRepository = iContentPageRepository;
        }

        /// <summary>
        /// Trang danh sách danh mục.
        /// Gọi ra view, dữ liệu sẽ được lấy qua AJAX (IndexPost/partial).
        /// </summary>
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Index()
        {
            return View();
        }

        #region [Create]
        /// <summary>
        /// Hiển thị form Tạo mới danh mục.
        /// - Thiết lập model mặc định (language, parentId, type) và danh sách Portal cho select.
        /// </summary>
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Create(int portalId,string language = "vi", int parrentId = 0)
        {
            var dl = new CategoryModel
            {
                Language = language,
                ParentId = parrentId,
                Type = ESlugType.Category,
                SlugType = ESlugType.Category
            };
            // Đổ danh sách portal để view hiển thị chọn portal
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            dl.PortalSelectList = new SelectList(portals, "Id", "Name");
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
            dl.PortalId = portalId;
            dl.PortalName = portals.FirstOrDefault(x => x.Id == portalId)?.Name;

            dl.PortalSelectList = new SelectList(portals, "Id", "Name");
            try
            {
                var cmCategorys = await _iAsyncNewsService.GetCategoriesAsync(dl.Language);
                var cmCategorysForDisplay = BuildNewsCategoryOptions(cmCategorys, language);
                dl.CMCategorySelectList = new SelectList(cmCategorysForDisplay, "Value", "Text");
            }
            catch
            {
            }
            return View(dl);
        }

        // Helper: build tree-like options from flat NewsCategoryItem list
        private List<SelectListItem> BuildNewsCategoryOptions(List<NewsCategoryItem> categories, string language)
        {
            var result = new List<SelectListItem>();
            if (categories == null || categories.Count == 0)
                return result;

            // build lookup by parent id for quick child lookup
            var lookup = categories.GroupBy(x => x.ParentNewsCategoryId)
                                   .ToDictionary(g => g.Key, g => g.OrderBy(y => (language == "en" ? y.En_NewsCategoryName : y.NewsCategoryName)).ToList());

            var added = new HashSet<int>();

            void AddNodeAndChildren(NewsCategoryItem node, int depth)
            {
                if (node == null) return;
                // prevent cycles / re-processing
                if (added.Contains(node.NewsCategoryId)) return;

                var prefix = string.Concat(Enumerable.Repeat("-----", depth));
                var text = $"({node.NewsCategoryId}){(language == "en" ? node.En_NewsCategoryName : node.NewsCategoryName)}";
                var display = string.IsNullOrEmpty(prefix) ? text : prefix + text;
                result.Add(new SelectListItem { Value = node.NewsCategoryId.ToString(), Text = display });
                added.Add(node.NewsCategoryId);

                if (lookup.ContainsKey(node.NewsCategoryId))
                {
                    foreach (var child in lookup[node.NewsCategoryId])
                    {
                        AddNodeAndChildren(child, depth + 1);
                    }
                }
            }

            // Determine root level: the smallest NewsCategoryLevel in the list
            int minLevel = categories.Min(c => c.NewsCategoryLevel);

            var roots = categories.Where(c => c.NewsCategoryLevel == minLevel)
                                  .OrderBy(c => (language == "en" ? c.En_NewsCategoryName : c.NewsCategoryName))
                                  .ToList();

            // Start from each root and add children recursively
            foreach (var root in roots)
            {
                AddNodeAndChildren(root, 0);
            }

            // Append any remaining items that were not added (safety)
            if (added.Count < categories.Count)
            {
                var remaining = categories.Where(c => !added.Contains(c.NewsCategoryId))
                                          .OrderBy(c => (language == "en" ? c.En_NewsCategoryName : c.NewsCategoryName));
                foreach (var item in remaining)
                {
                    var text = $"({item.NewsCategoryId}){(language == "en" ? item.En_NewsCategoryName : item.NewsCategoryName)}";
                    result.Add(new SelectListItem { Value = item.NewsCategoryId.ToString(), Text = text });
                }
            }

            return result;
        }

 
        /// <summary>
        /// Xử lý POST tạo mới danh mục.
        /// Các bước:
        /// 1. Validate input (ModelState).
        /// 2. Kiểm tra trùng tên trên cùng portal/language/type (trim + case-insensitive).
        /// 3. Tạo Category, lưu vào DB, commit.
        /// 4. Tạo/điền SEO link và cập nhật file liên quan.
        /// 5. Ghi log.
        /// </summary>
        /// 
     
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(CategoryModel use, string altId, int portalId)
        {
            try
            {
                if (!ModelState.IsValid)
                    // Nếu dữ liệu không hợp lệ theo DataAnnotation đã khai báo trên model -> trả về cảnh báo
                    return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
                await _iCategoryRepository.BeginTransaction();
            
                var nameCheck = (use.Name ?? string.Empty).Trim()?.ToUpper();
                var candidates = await _iCategoryRepository.AnyAsync(x => x.Language == use.Language && x.PortalId == portalId && x.Name.ToUpper() == nameCheck);
                if (candidates)
                {
                    return new ResponseModel() { Output = 0, Message = "Danh mục đã tồn tại trên hệ thống, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }

                // Tạo đối tượng Category mới
                // --- Bước 3: Tạo entity Category và lưu vào DB ---
                // Lấy order lớn nhất hiện tại để sắp xếp (nếu cần)
                var maxOrder = _iCategoryRepository.MaxOrder(x => x.ParentId == 0);
                var data = new Category
                {
                    // Gán tên đã trim
                    Name = (use.Name ?? string.Empty).Trim(),
                    Banner = use.Banner,
                    Content = use.Content,
                    Status = use.Status,
                    Language = use.Language,
                    Order = maxOrder + 1,
                    ParentId = 0,
                    Summary = use.Summary,
                    Type = CategoryType.CategoryService,
                    PortalId = portalId,
                    SlugType = ESlugType.Category,
                    CategoryType = use.CategoryType,
                    ExCategoryIds = use.ExCategoryIds,
                    ReferentCategoryId = use.ReferentCategoryId
                };

                await _iCategoryRepository.AddAsync(data);
                await _iCategoryRepository.CommitAsync();

                // Thêm SEO link, cập nhật file và ghi log
                // --- Bước 4: Cập nhật SEO/link, file liên quan và ghi log ---
                // Thêm liên kết SEO cho đối tượng mới (tạo slug, link nếu cần)
                await CreateLinkAsync(data.SlugType ?? ESlugType.Category, data.Language, data.Id, MapModel<SeoModel>.Go(use), data.Name, "", "Category", "Details", data.PortalId);
                // Cập nhật file (nếu người dùng upload trước khi lưu)
                await UpdateFileData(data.Id, data.SlugType ?? ESlugType.Category, altId);
                await _iCategoryRepository.CommitTransaction();
                // Ghi log hành động tạo
                await AddLog(new LogModel
                {
                    ObjectId = data.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Thêm mới danh mục \"{data.Name}\".",
                    Type = LogType.Create
                });

                return new ResponseModel() { Output = 1, Message = "Thêm mới danh mục thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
                return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
            }
        }
        #endregion

        #region [Edit]
        /// <summary>
        /// Hiển thị form Edit danh mục.
        /// - Lấy đối tượng theo id, map sang CategoryModel và nạp dữ liệu SEO nếu có.
        /// - Đổ danh sách Portal để view hiển thị select portal.
        /// </summary>
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Edit(int id)
        {
            var dl = await _iCategoryRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null)
                return View("404");

            var model = MapModel<CategoryModel>.Go(dl);
            // Nạp thông tin SEO nếu tồn tại
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
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            model.PortalSelectList = new SelectList(portals, "Id", "Name");
            model.PortalId = dl.PortalId;
            model.PortalName = portals.FirstOrDefault(x => x.Id == dl.PortalId)?.Name;
            model.CategoryType = dl.CategoryType;
            model.FullPath = await _iPortalRepository.GetFullPathAsync(model.PortalId ?? 1, model.Slug ?? string.Empty, portals, model.Language, _baseSettings.Value.MultipleLanguage);

            try
            {
                var cmCategorys = await _iAsyncNewsService.GetCategoriesAsync(dl.Language);
                var cmCategorysForDisplay = BuildNewsCategoryOptions(cmCategorys, ktLink.Language);
                model.CMCategorySelectList = new SelectList(cmCategorysForDisplay, "Value", "Text", dl.ReferentCategoryId);
            }
            catch
            {
            }

            return View(model);
        }
        /// <summary>
        /// Xử lý POST cập nhật danh mục.
        /// - Validate input.
        /// - Kiểm tra trùng tên tương tự Create (trim + case-insensitive), loại trừ chính record đang sửa.
        /// - Cập nhật Category, commit, cập nhật SEO và ghi log.
        /// </summary>
        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(CategoryModel use, int id, int portalId)
        {

            try
            {
                if (!ModelState.IsValid)
                    return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin.", Type = ResponseTypeMessage.Warning };

                var dl = await _iCategoryRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (dl == null)
                    return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
               await  _iCategoryRepository.BeginTransaction();
                // Nếu thay đổi CategoryType thì cập nhật tất cả các contentPage có danh mục chính là danh mục này thì update lại CategoryType
                if(use.CategoryType != dl.CategoryType)
                {
                    var listContentPageCategory = await _iContentPageRepository.SearchAsync(false, 0, 0, x => x.CategoryId == id);
                    foreach(var item in listContentPageCategory)
                    {
                        item.CategoryType = use.CategoryType;
                        _iContentPageRepository.Update(item);
                    }
                    await _iContentPageRepository.CommitAsync();
                }
                var name = (use.Name ?? string.Empty).Trim();
        
                var nameCheck = (use.Name ?? string.Empty).Trim()?.ToUpper();
                var candidates = await _iCategoryRepository.AnyAsync(x => x.Language == use.Language && x.PortalId == portalId && x.Name.ToUpper() == nameCheck && x.Id !!= id);
                if (candidates)
                {
                    return new ResponseModel() { Output = 0, Message = "Danh mục đã tồn tại trên hệ thống, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }

                await UpdateLinkAsync(use.ChangeSlug, dl.SlugType ?? ESlugType.Category, dl.Id, dl.Language, MapModel<SeoModel>.Go(use), dl.Name, "", "Category", "Details");

                dl.CategoryType = use.CategoryType;
                dl.Name = name;
                dl.Banner = use.Banner;
                dl.Content = use.Content;
                dl.Status = use.Status;
                dl.Summary = use.Summary;
                dl.ExCategoryIds = use.ExCategoryIds;
                dl.ReferentCategoryId = use.ReferentCategoryId;
                _iCategoryRepository.Update(dl);
                await _iCategoryRepository.CommitAsync();
                _iCategoryRepository.Update(dl);
                await _iCategoryRepository.CommitAsync();
                await _iCategoryRepository.CommitTransaction();
                await AddLog(new LogModel
                {
                    ObjectId = dl.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Cập nhật danh mục \"{dl.Name}\".",
                    Type = LogType.Edit
                });

                return new ResponseModel() { Output = 1, Message = "Cập nhật danh mục thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
                return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
            }
        }
        #endregion

        #region [Delete]
        /// <summary>
        /// Xóa danh mục (thực hiện soft-delete hoặc theo repository triển khai).
        /// - Sau khi xóa cần xóa SEO link, file liên quan và ghi log.
        /// </summary>
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeletePost(int id)
        {
            try
            {
                await _iCategoryRepository.BeginTransaction();
                var kt = await _iCategoryRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null)
                {
                    return new ResponseModel() { Output = 0, Message = "Danh mục không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                // Kiểm tra xem item có dữ liệu con không, nếu có thì không cho xóa
                var hasChild = await _iCategoryRepository.AnyAsync(x => x.ParentId == id);
                if (hasChild)
                {
                    return new ResponseModel() { Output = 0, Message = "Danh mục đang có danh mục con, vui lòng xóa danh mục con trước khi xóa danh mục này.", Type = ResponseTypeMessage.Warning };
                }
                // 
                _iCategoryRepository.Delete(kt);
                await _iCategoryRepository.CommitAsync();

                _iContentPageCategoryRepository.DeleteWhere(x=>x.CategoryId == id);
                await _iContentPageCategoryRepository.CommitAsync();
                await DeleteSeoLink(kt.SlugType ?? ESlugType.Category, kt.Id);
                await RemoveFileData(id, kt.SlugType ?? ESlugType.Category);
                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa danh mục  tin tức \"{kt.Name}\".",
                    Type = LogType.Delete
                });
                await _iCategoryRepository.CommitTransaction();
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
        /// <summary>
        /// Các action liên quan tới cấu hình cấu trúc danh mục (cây):
        /// - Setting: hiển thị giao diện quản lý cấu trúc
        /// - Categorys: trả về HTML tree của danh mục (dùng trong tree view)
        /// - SettingPost: cập nhật cấu trúc (parent/order) từ client
        /// </summary>
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Setting(string language = "vi")
        {
            ViewData["portals"] = await _iPortalRepository.SearchAsync();
            return View(nameof(Setting), language);
        }
        [HttpGet]
        public async Task<string> Categorys(string language)
        {
            var list = await _iCategoryRepository.FindByLinkReference(0, 0, x => x.Language == language);
            var portals = await _iPortalRepository.SearchAsync();
            foreach(var item in list)
            {
                item.FullPath = await _iPortalRepository.GetFullPathAsync(item.PortalId, item.Link?.Slug ?? string.Empty, portals, item.Language, _baseSettings.Value.MultipleLanguage);
            }
            return ShowTree(list, 0, portals);
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

        /// <summary>
        /// Chuyển cấu trúc cây DataSortModel sang danh sách phẳng, gán parentId và order.
        /// Dùng khi client gửi cấu trúc mới (nestable) về server.
        /// </summary>
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

        /// <summary>
        /// Sinh HTML dạng cây cho danh sách Category.
        /// - Hàm trả về chuỗi HTML <ol><li>... dùng render trực tiếp lên client.
        /// - Tham số portals truyền danh sách portal để hiển thị thông tin portal tương ứng.
        /// </summary>
        private string ShowTree(List<Category> list, int parrentId, List<Portal> portals)
        {
            var listCulture = Shared.ListData.ListLanguage;
            StringBuilder str = new();
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
                    str.Append($"<strong title='{portals.FirstOrDefault(x=>x.Id == item.PortalId)?.Name}' style='margin-right: 10px;color: #FF5722;'>#{item.PortalId}</strong>");
                    str.Append($"<span  class='label label-info' style='margin-right: 10px;'>{item.CategoryType?.GetDisplayName()}</span> ");
                    //
                    str.Append($"{item.Name}");
                    str.Append($"<span class='can-span-category'>{BindReferenLanguage(new Tuple<List<LinkReference>, string, string, bool>(item.LinkReferences, item.Language, Url.Action("Edit", new { id = "#id#" }), false))}</span>");
                    str.Append($"<span class=\"button-icon\"><a title='Đến trang' target='_blank' href=\"{item.FullPath}\" title=\"Lấy đường dẫn\"><i class=\"material-icons iconcontrol text-info\">link</i></a><a onclick=\"onCopyClipboard('{item.FullPath}')\" title=\"Lấy đường dẫn\"><i class=\"material-icons iconcontrol text-danger\">share</i></a>");
                    str.Append($"<a button-popup  data-target=\"#myModalBig\" href='{Url.Action("Edit", new { id = item.Id, language = item.Language })}'  title=\"Cập nhật\"><i class=\"material-icons iconcontrol text-primary\">edit</i></a><span>");
                    str.Append("</div>");
                    str.Append(ShowTree(list, item.Id, portals));
                    str.Append("</li>");
                }
                str.Append($"</ol>");
            }
            return str.ToString();
        }
        /// <summary>
        /// Sinh các nút / label cho từng ngôn ngữ thể hiện trạng thái liên kết (link reference) của category.
        /// - Trả về chuỗi HTML chứa các label (check/block) hoặc link để mở popup edit ngôn ngữ tương ứng.
        /// </summary>
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
        /// <summary>
        /// Upload image cho category: validate extension, kích thước, resize nếu cần và ghi file metadata.
        /// - Trả về ResponseModel hoặc CKEditor response tuỳ type.
        /// </summary>
        [HttpPost, ActionName("UploadImage")]
        [AuthorizePermission("Index")]
        public async Task<object> UploadImagePost(string altId, int id, int type = 0, int portalId = 2)
        {
            try
            {
                var allowed = (_baseSettings.Value.ImagesType ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                if (!allowed.Contains(".webp"))
                {
                    allowed.Add(".webp");
                }
                var folderByDate = Functions.GenFolderByDate();
                string configuredDataPath = _baseSettings.Value.DataPath;
                if(portalId == 1)
                {
                    configuredDataPath = _baseSettings.Value.DataPathFR;
                }    
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

        #region [Tree DC Category]
        /// <summary>
        /// Lấy cây danh mục DC dạng tree view để chọn một hoặc nhiều danh mục
        /// </summary>
        [HttpPost]
        [AuthorizePermission("Index")]
        [Authorize]
        public async Task<List<TreeRoleModel>> TreeDCCategory(int id, string language = "vi")
        {
            try
            {
                var listCurrent = new List<int>();
                if (id > 0)
                {
                    var category = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Id == id);
                    if (category != null && !string.IsNullOrWhiteSpace(category.ExCategoryIds))
                    {
                        listCurrent = category.ExCategoryIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                           .Select(x => int.Parse(x))
                                                           .ToList();
                    }
                }

                var dcCategories = await _iAsyncNewsService.GetCategoriesAsync(language);
                
                var result = dcCategories.Select(x =>
                    new TreeRoleModel
                    {
                        Id = x.NewsCategoryId.ToString(),
                        Parent = x.ParentNewsCategoryId == 0 ? "#" : x.ParentNewsCategoryId.ToString(),
                        Text = $"({x.NewsCategoryId}) {(language == "en" ? x.En_NewsCategoryName : x.NewsCategoryName)}",
                        State = new TreeRoleStateModel 
                        { 
                            Disabled = false, 
                            Opened = true, 
                            Selected = listCurrent.Contains(x.NewsCategoryId) && !dcCategories.Any(z => z.ParentNewsCategoryId == x.NewsCategoryId)
                        },
                        Icon = null
                    }
                ).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
                return new List<TreeRoleModel>();
            }
        }
        #endregion
    }
}