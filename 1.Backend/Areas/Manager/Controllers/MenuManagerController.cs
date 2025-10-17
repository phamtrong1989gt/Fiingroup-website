using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PT.BE.Areas.Manager.Controllers
{
    /// <summary>
    /// Area: Manager
    /// Controller: MenuManagerController
    /// 
    /// Responsibilities:
    /// - Provide CRUD operations for menu definitions and menu items used by the frontend.
    /// - Render hierarchical menu HTML from menu items using templates stored on the Menu entity.
    /// - Expose endpoints used by the manager UI (index, create, edit, delete, order update, search link).
    /// 
    /// Notes for maintainers:
    /// - This controller relies on repositories for data access: IMenuRepository, IMenuItemRepository,
    ///   ILinkRepository and IPortalRepository. Repositories return domain entities (Menu, MenuItem, Link).
    /// - Templates are simple string templates (Template, Template1, Template2, Template3). The
    ///   private methods `UpdateGroupMenu` and `LopUpdateGroupMenu` combine templates with menu items
    ///   to produce the final HTML stored in `Menu.Content` and propagated to cache via
    ///   `CommonFunctions.TriggerCacheModuleClear` after changes.
    /// - Token constants (e.g. [Url], [Name]) are used by the template engine; keep them in sync
    ///   with the template syntax used in views or theme files.
    /// - All action methods are protected with custom authorization attributes like
    ///   `[AuthorizePermission("Index")]` or `[Authorize]` where appropriate.
    /// </summary>
    [Area("Manager")]
    public class MenuManagerController : Base.Controllers.BaseController
    {
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly ILogger _logger;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IMenuRepository _iMenuRepository;
        private readonly IMenuItemRepository _iMenuItemRepository;
        private readonly ILinkRepository _iLinkRepository;
        private readonly IPortalRepository _iPortalRepository;
        public MenuManagerController(
            ILogger<MenuManagerController> logger,
            IOptions<BaseSettings> baseSettings,
            IWebHostEnvironment iHostingEnvironment,
            IMenuRepository iMenuRepository,
            IMenuItemRepository iMenuItemRepository,
            ILinkRepository iLinkRepository,
            IPortalRepository iPortalRepository
        )
        {
            controllerName = "MenuManager";
            tableName = "Menu";
            _logger = logger;
            _baseSettings = baseSettings;
            _iHostingEnvironment = iHostingEnvironment;
            _iMenuRepository = iMenuRepository;
            _iMenuItemRepository = iMenuItemRepository;
            _iLinkRepository = iLinkRepository;
            _iPortalRepository = iPortalRepository;
        }

        // Constructor summary:
        // - Dependency injection supplies logger, settings, hosting environment and repositories.
        // - controllerName and tableName are initialized for use by the base controller logging helpers.

        #region [Index]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Index()
        {

            // Lấy danh sách portal để hiển thị trong dropdown trên trang quản lý
            // Tham số: true = only active, 0,0 = không phân trang
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            // Đưa danh sách portal vào ViewData để view có thể bind vào SelectList
            ViewData["PortalSelectList"] = new SelectList(portals, "Id", "Name");
            // Trả về view chính. Dữ liệu bảng sẽ được nạp bằng Ajax gọi IndexPost
            return View();
        }
        /// <summary>
        /// GET: Manager/MenuManager/Index
        /// Display the main listing page for menus. The page will request data via the POST Index action
        /// (IndexPost) to populate the datagrid. Returns view with portal dropdown values in ViewData.
        /// </summary>
        [HttpPost, ActionName("Index")]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> IndexPost(int? id, int? page, int? limit, string key, string code, bool? status, string language = "vi", string ordertype = "asc", string orderby = "name",int? portalId = null)
        {

            // Chuẩn hóa tham số phân trang
            page = page < 0 ? 1 : page;
            // Giới hạn limit để tránh truy vấn trả về quá nhiều bản ghi
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iMenuRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                    m => (m.Name.Contains(key) || key == null) 
                    && (m.Code == code || code == null)
                    && (m.PortalId == portalId || portalId == null)
                    && (m.Status == status || status == null)
                    && (m.Language == language) 
                    && !m.Delete
                    && (m.Id==id || id==null)
                    ,
                OrderByExtention(ordertype, orderby));

            // Gắn thông tin Portal vào từng menu để view có thể hiển thị tên cổng
            var portals = await _iPortalRepository.SearchAsync(true);
            foreach (var item in data.Data)
            {
                item.Portal = portals.FirstOrDefault(x => x.Id == item.PortalId);
            }

            // Trả về partial view chứa dữ liệu (dùng cho Ajax load bảng)
            return View("IndexAjax", data);
        }
        /// <summary>
        /// POST: Manager/MenuManager/Index
        /// Returns a paged list of menus filtered by parameters for server-side pagination and search.
        /// - `key` filters by menu name (contains), `code` filters exact code, `language` filters language.
        /// - The function also attaches the related Portal to each Menu for display.
        /// </summary>
        private Func<IQueryable<Menu>, IOrderedQueryable<Menu>> OrderByExtention(string ordertype, string orderby)
        {
            Func<IQueryable<Menu>, IOrderedQueryable<Menu>> functionOrder = null;
            switch (orderby)
            {
                case "name":
                    // Sắp xếp theo tên (tăng dần hoặc giảm dần)
                    functionOrder = ordertype == "asc" ? EntityExtention<Menu>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<Menu>.OrderBy(m => m.OrderByDescending(x => x.Name));
                    break;
                default:
                    // Mặc định sắp xếp theo Id
                    functionOrder = ordertype == "asc" ? EntityExtention<Menu>.OrderBy(m => m.OrderBy(x => x.Id)) : EntityExtention<Menu>.OrderBy(m => m.OrderByDescending(x => x.Id));
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
            var dl = new MenuModel
            {
                Language = language
            };
            // Nếu ứng dụng hỗ trợ đa ngôn ngữ thì truyền prefix ngôn ngữ dùng trong URL
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            dl.PortalSelectList = new SelectList(portals, "Id", "Name");
            return View(dl);
        }
        /// <summary>
        /// GET: Manager/MenuManager/Create
        /// Return the view to create a new Menu. A MenuModel with default language is prepared.
        /// </summary>
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(MenuModel use)
        {
            try
            {
                // Kiểm tra dữ liệu model hợp lệ theo DataAnnotations
                if (ModelState.IsValid)
                {
                    // Kiểm tra trùng mã menu (Code) — phải duy nhất
                    var ktCode = await _iMenuRepository.SingleOrDefaultAsync(true, m => m.Code == use.Code);
                    if (ktCode != null)
                    {
                        // Trả về warning cho UI để người dùng đổi mã khác
                        return new ResponseModel() { Output = 0, Message = "Mã menu đã tồn tại, vui lòng chọn mã khác.", Type = ResponseTypeMessage.Warning };
                    }

                    // Tạo entity Menu từ model nhận vào. Lưu ý: chưa gán Content (nội dung HTML)
                    // Content sẽ được sinh dựa trên template và menu item hiện có (thường rỗng khi mới tạo)
                    var data = new Menu
                    {
                        Name = use.Name,
                        Delete = false,
                        Status = use.Status,
                        Language = use.Language,
                        HasChildrentClass1 = use.HasChildrentClass1,
                        HasChildrentClass2 = use.HasChildrentClass2,
                        HasChildrentClass3 = use.HasChildrentClass3,
                        Template = use.Template,
                        Template1 = use.Template1,
                        Template2 = use.Template2,
                        Template3 = use.Template3,
                        Code = use.Code,
                        PortalId = use.PortalId
                    };

                    // Sinh nội dung Content mặc định từ template và dữ liệu hiện có (nếu có)
                    // Khi tạo mới chưa có menu item, UpdateGroupMenu sẽ trả về chuỗi rỗng hoặc template mặc định
                    data.Content = await UpdateGroupMenu(data);

                    // Lưu entity vào repository và commit
                    await _iMenuRepository.AddAsync(data);
                    await _iMenuRepository.CommitAsync();
                    
                    // Sau khi lưu xong, xóa bộ nhớ đệm liên quan đến module Menu để frontend lấy dữ liệu mới
                    // Lưu ý: TriggerCacheModuleClear sẽ tuỳ implementation của CommonFunctions để clear cache
                    CommonFunctions.TriggerCacheModuleClear(data.Content, ModuleType.Menu, data.Code, data.Language, data.PortalId);

                    // Ghi log thao tác để audit
                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới menu \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    // Trả về kết quả thành công cho UI. IsClosePopup = true giúp UI đóng dialog nếu cần
                    return new ResponseModel() { Output = 1, Message = "Thêm mới menu thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        /// <summary>
        /// POST: Manager/MenuManager/Create
        /// Create a new Menu record. Validates duplicate `Code` then builds the initial Content
        /// by calling `UpdateGroupMenu` and saves to repository. Clears cache and logs the creation.
        /// Returns a ResponseModel used by the manager UI to show result and optionally close popup.
        /// </summary>
        #endregion

        #region [Edit]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Edit(int id)
        {
            var dl = await _iMenuRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null || (dl != null && dl.Delete))
            {
                return View("404");
            }
            var model = MapModel<MenuModel>.Go(dl);
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            model.PortalSelectList = new SelectList(portals, "Id", "Name");
            // Trả về view edit với model đã được map sẵn
            return View(model);
        }
        /// <summary>
        /// GET: Manager/MenuManager/Edit/{id}
        /// Retrieve menu data for editing. Returns 404 view if not found or deleted.
        /// </summary>
        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(MenuModel use, int id)
        {
            try
            {
                // Kiểm tra model hợp lệ
                if (ModelState.IsValid)
                {
                    // Lấy thực thể cần cập nhật (lock để thay đổi) — false để lấy entity theo trạng thái track
                    var dl = await _iMenuRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null || (dl != null && dl.Delete))
                    {
                        // Dữ liệu không tồn tại hoặc đã bị soft-delete
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    // Kiểm tra trùng code trên các record khác
                    var ktCode = await _iMenuRepository.SingleOrDefaultAsync(true, m => m.Code == use.Code && m.Id != dl.Id);
                    if (ktCode != null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Mã menu đã tồn tại, vui lòng chọn mã khác.", Type = ResponseTypeMessage.Warning };
                    }

                    // Gán lại các trường từ model vào entity
                    dl.Name = use.Name;
                    dl.Status = use.Status;
                    dl.Language = use.Language;
                    dl.HasChildrentClass1 = use.HasChildrentClass1;
                    dl.HasChildrentClass2 = use.HasChildrentClass2;
                    dl.HasChildrentClass3 = use.HasChildrentClass3;
                    dl.Template = use.Template;
                    dl.Template1 = use.Template1;
                    dl.Template2 = use.Template2;
                    dl.Template3 = use.Template3;
                    dl.Code = use.Code;
                    dl.PortalId = use.PortalId;

                    // Rebuild lại Content dựa trên template mới và các menu item hiện có
                    dl.Content = await UpdateGroupMenu(dl);

                    // Cập nhật entity
                    _iMenuRepository.Update(dl);
                    await _iMenuRepository.CommitAsync();

                    // Xóa cache module liên quan để frontend lấy nội dung mới
                    CommonFunctions.TriggerCacheModuleClear(dl.Content, ModuleType.Menu, dl.Code, dl.Language, dl.PortalId);

                    // Ghi log hành động cập nhật
                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật menu \"{dl.Name}\".",
                        Type = LogType.Edit
                    });
                    return new ResponseModel() { Output = 1, Message = "Cập nhật menu thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = false };
                }
                return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin.", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        /// <summary>
        /// POST: Manager/MenuManager/Edit
        /// Updates a Menu record with the provided model. Rebuilds `Content` from templates and menu items
        /// and clears cache after commit. Returns standardized ResponseModel for the UI.
        /// </summary>
        #endregion

        #region [Delete]
        [HttpPost, ActionName("Delete")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeletePost(int id)
        {
            try
            {
                var kt = await _iMenuRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null || (kt != null && kt.Delete))
                {
                    return new ResponseModel() { Output = 0, Message = "menu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                kt.Delete = true;
                // Đánh dấu soft-delete. Không xóa thực tế khỏi DB để giữ lịch sử, backup...
                // Xóa cache liên quan để frontend không còn hiển thị menu này nữa.
                CommonFunctions.TriggerCacheModuleClear(null, ModuleType.Menu, kt.Code, kt.Language, kt.PortalId);
                await _iMenuRepository.CommitAsync();

                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa menu \"{kt.Name}\".",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa menu thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        /// <summary>
        /// POST: Manager/MenuManager/Delete
        /// Soft-deletes the menu (sets Delete flag true), clears module cache and logs the action.
        /// </summary>
        #endregion

        #region [DeleteItem]
        [HttpPost, ActionName("DeleteItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeleteItemPost(int id)
        {
            try
            {
                var kt = await _iMenuItemRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null)
                {
                    return new ResponseModel() { Output = 0, Message = "menu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                // Xóa menu item khỏi repository (thường là xóa vật lý trong DB tuỳ implement của repository)
                _iMenuItemRepository.Delete(kt);
                await _iMenuItemRepository.CommitAsync();

                var dataParent = await _iMenuRepository.SingleOrDefaultAsync(false, x => x.Id == kt.MenuId);
                if (dataParent != null)
                {
                    dataParent.Content = await UpdateGroupMenu(dataParent);
                    _iMenuRepository.Update(dataParent);
                    await _iMenuRepository.CommitAsync();
                    CommonFunctions.TriggerCacheModuleClear(dataParent.Content, ModuleType.Menu, dataParent.Code, dataParent.Language, dataParent.PortalId);
                }

                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa menu item \"{kt.Name}\".",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa menu item thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        /// <summary>
        /// POST: Manager/MenuManager/DeleteItem
        /// Permanently deletes a single MenuItem and rebuilds the parent Menu's Content template.
        /// </summary>
        #endregion

        #region [Hàm process]
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
        /// Flattens a nested list of DataSortModel (received from client-side nested sortable)
        /// into a single-level list with correct ParentId and Order values.
        /// - `list` parameter is expected to contain items where each item may have Children.
        /// - The method walks depth-first and assigns incremental Order values for siblings.
        /// - Use case: when the manager UI sends the new ordering after drag/drop, convert it
        ///   to a format that can be applied back to MenuItem entities.
        /// </summary>

        private string ShowTree(List<MenuItem> list, int parrentId)
        {
            if (list.Count() == 0)
            {
                return "<span>Hiện tại chưa có dữ liệu</span>";
            }
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
                foreach (var item in listitem)
                {
                    if (item.Id == parrentId)
                    {
                        return "";
                    }
                    str.Append($"<li class=\"dd-item {(item.Status ? "treeTrue" : "")} dd3-item\" data-id=\"{item.Id}\">");
                    str.Append($"<div class=\"dd-handle dd3-handle\"></div>");
                    str.Append($"<div class=\"dd3-content\">");
                    str.Append($"{item.Name}");
                    str.Append($"<span class=\"button-icon\"><a button-static href='{Url.Action("EditItem", new { id = item.Id })}'  title=\"Cập nhật\"><i class=\"material-icons iconcontrol text-primary\">edit</i></a><span>");
                    str.Append("</div>");
                    str.Append(ShowTree(list, item.Id));
                    str.Append("</li>");
                }
                str.Append($"</ol>");
            }
            return str.ToString();
        }
        /// <summary>
        /// Build the HTML representation of the menu tree used in the admin UI.
        /// - The format (<ol class="dd-list">, <li class="dd-item">) is required by the
        ///   javascript nested-sortable plugin used in the manager.
        /// - Each node includes an Edit link that opens the item editor.
        /// - Recursive: it appends children by calling itself.
        /// </summary>

        [HttpGet]
        public async Task<string> Items(int menuId)
        {
            // Lấy toàn bộ menu item của menu theo menuId (không phân trang)
            var list = await _iMenuItemRepository.SearchAsync(true, 0, 0, x => x.MenuId == menuId);
            // Trả về HTML cây để client hiển thị
            return ShowTree(list, 0);
        }
        /// <summary>
        /// GET: Manager/MenuManager/Items
        /// Returns the HTML representation of menu items for a given menu id. Used by the UI to display
        /// the current tree. The response is raw HTML string produced by ShowTree.
        /// </summary>
        #endregion

        #region [CreateItem]
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult CreateItem(int menuId = 0)
        {
            var dl = new MenuItemModel
            {
                MenuId = menuId
            };
            return View(dl);
        }
        /// <summary>
        /// GET: Manager/MenuManager/CreateItem
        /// Return the create view for a new MenuItem. MenuId is pre-filled when adding under a menu.
        /// </summary>
        [HttpPost, ActionName("CreateItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreateItemPost(MenuItemModel use)
        {
            try
            {
                // Kiểm tra hợp lệ
                if (ModelState.IsValid)
                {
                    // Lấy giá trị order lớn nhất hiện tại ở cấp cha = 0 để thêm vào cuối
                    var maxOrder = _iMenuItemRepository.MaxOrder(x => x.ParentId == 0);

                    // Tạo MenuItem mới từ dữ liệu nhận vào
                    var data = new MenuItem
                    {
                        Name = use.Name,
                        Status = use.Status,
                        Order = maxOrder + 1,
                        ParentId = 0,
                        Href = use.Href,
                        MenuId = use.MenuId,
                        Class = use.Class,
                        Icon = use.Icon,
                        Target = use.Target,
                        CategoryType = use.CategoryType ?? CategoryType.Tour,
                        IsLinkLocal = use.IsLinkLocal,
                        Language = use.Language,
                        LinkId = use.LinkId
                    };

                    // Nếu item liên kết tới Link nội bộ (IsLinkLocal = true) thì lấy slug + language từ Link
                    // và format Href đúng định dạng site
                    if (data.IsLinkLocal)
                    {
                        var dlLink = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.Id == data.LinkId);
                        data.Language = dlLink?.Language;
                        data.Href = Functions.FormatUrl(dlLink?.Language, dlLink?.Slug);
                    }

                    // Lưu MenuItem mới
                    await _iMenuItemRepository.AddAsync(data);
                    await _iMenuItemRepository.CommitAsync();

                    // Sau khi thêm item, phải rebuild lại Content của menu cha để frontend hiển thị menu mới
                    var dataParent = await _iMenuRepository.SingleOrDefaultAsync(false, x => x.Id == data.MenuId);
                    if (dataParent != null)
                    {
                        dataParent.Content = await UpdateGroupMenu(dataParent);
                        _iMenuRepository.Update(dataParent);
                        await _iMenuRepository.CommitAsync();
                        CommonFunctions.TriggerCacheModuleClear(dataParent.Content, ModuleType.Menu, dataParent.Code, dataParent.Language, dataParent.PortalId);
                    }

                    // Ghi log thao tác
                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm menu item \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới menu item thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = false };
                }
                return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        /// <summary>
        /// POST: Manager/MenuManager/CreateItem
        /// Creates a single MenuItem. If the item references a local Link (IsLinkLocal), the Href is
        /// constructed from the Link's slug and language. After saving, parent Menu.Content is rebuilt
        /// and cache cleared.
        /// </summary>
        #endregion

        #region [EditItem]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> EditItem(int id)
        {
            var dl = await _iMenuItemRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null)
            {
                return View("404");
            }
            var model = MapModel<MenuItemModel>.Go(dl);
            if(model.IsLinkLocal)
            {
                var dataLink = await _iLinkRepository.SearchAsync(true, 0, 1, x => x.Id == model.LinkId, null, x=> new Link { Id = x.Id , Name = $"({x.Language}|{x.Type.GetDisplayName()}) / {x.Name}" });
                model.LinkSelectList = new SelectList(dataLink, "Id", "Name");
            }    
            return View(model);
        }
        /// <summary>
        /// GET: Manager/MenuManager/EditItem/{id}
        /// Returns the edit view for a MenuItem. If the item uses a local link, builds a select list
        /// with the referenced Link for the UI.
        /// </summary>
        [HttpPost, ActionName("EditItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditItemPost(MenuItemModel use, int id)
        {
            try
            {
                if (use.IsLinkLocal)
                {
                    var dlLink = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.Id == use.LinkId);
                    use.Href = Functions.FormatUrl(dlLink?.Language, dlLink?.Slug);
                }

                // Kiểm tra hợp lệ model
                if (ModelState.IsValid)
                {
                    // Lấy menu item để cập nhật
                    var dl = await _iMenuItemRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null)
                    {
                        // Không tìm thấy dữ liệu (có thể đã bị xóa bởi thao tác khác)
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    // Cập nhật các trường thay đổi trên entity
                    dl.Name = use.Name;
                    dl.Status = use.Status;
                    dl.Href = use.Href;
                    dl.Class = use.Class;
                    dl.Icon = use.Icon;
                    dl.Target = use.Target;
                    dl.IsLinkLocal = use.IsLinkLocal;

                    // Lưu thay đổi
                    _iMenuItemRepository.Update(dl);
                    await _iMenuItemRepository.CommitAsync();

                    // Rebuild menu cha để cập nhật Content nếu cần
                    var dataParent = await _iMenuRepository.SingleOrDefaultAsync(false, x => x.Id == dl.MenuId);
                    if (dataParent != null)
                    {
                        dataParent.Content = await UpdateGroupMenu(dataParent);
                        _iMenuRepository.Update(dataParent);
                        await _iMenuRepository.CommitAsync();
                        CommonFunctions.TriggerCacheModuleClear(dataParent.Content, ModuleType.Menu, dataParent.Code, dataParent.Language, dataParent.PortalId);
                    }

                    // Ghi log hành động
                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật menu item \"{dl.Name}\".",
                        Type = LogType.Edit
                    });
                    return new ResponseModel() { Output = 1, Message = "Cập nhật menu thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = false };
                }
                return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin.", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        /// <summary>
        /// POST: Manager/MenuManager/EditItem
        /// Updates properties on a MenuItem. After saving, rebuilds parent Menu.Content and clears cache.
        /// </summary>

        [HttpPost, ActionName("UpdateOrder")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> UpdateOrderPost([FromBody]string data, int id)
        {
            try
            {
                // Bước 1: Deserialize payload JSON từ client (dữ liệu nested sortable)
                var listItem = ConverData(Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataSortModel>>(data));

                // Bước 2: Lấy danh sách Id cần cập nhật từ payload
                var listItemIds = listItem.Select(x => x.Id).ToList();

                // Bước 3: Lấy các MenuItem tương ứng từ database để cập nhật
                var listCa = await _iMenuItemRepository.SearchAsync(false, 0, 0, x => listItemIds.Contains(x.Id));

                // Bước 4: Áp dụng ParentId và Order mới cho từng entity
                // Lưu ý: nếu có nhiều client cùng sửa có thể gây race condition; nên cân nhắc lock/optimistic concurrency nếu cần
                foreach (var item in listCa)
                {
                    var objIn = listItem.FirstOrDefault(x => x.Id == item.Id);
                    item.ParentId = objIn == null ? 0 : objIn.ParentId;
                    item.Order = objIn.Order;
                    _iMenuItemRepository.Update(item);
                }

                // Bước 5: Commit tất cả thay đổi của MenuItem
                await _iMenuItemRepository.CommitAsync();

                // Bước 6: Rebuild lại nội dung menu cha (Content) sau khi order thay đổi
                var dataParent = await _iMenuRepository.SingleOrDefaultAsync(false, x => x.Id == id);
                if (dataParent != null)
                {
                    dataParent.Content = await UpdateGroupMenu(dataParent);
                    _iMenuRepository.Update(dataParent);
                    await _iMenuRepository.CommitAsync();
                    CommonFunctions.TriggerCacheModuleClear(dataParent.Content, ModuleType.Menu, dataParent.Code, dataParent.Language, dataParent.PortalId);
                }

                // Trả về kết quả thành công
                return new ResponseModel() { Output = 1, Message = "Cập nhật thành công.", Type = ResponseTypeMessage.Success };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        /// <summary>
        /// POST: Manager/MenuManager/UpdateOrder
        /// Accepts a JSON payload representing the nested order (from the UI), flattens it with
        /// `ConverData` and applies ParentId/Order updates to the MenuItem entities. Rebuilds parent
        /// menu content and clears cache after commit.
        /// </summary>
        #endregion

        #region [Static]
        private readonly string tokenUrl = "[Url]";
        private readonly string tokenTarget = "[Target]";
        private readonly string tokenName = "[Name]";
        private readonly string tokenIcon = "[Icon]";
        private readonly string tokenContent = "[Content]";
        private readonly string tokenHasChildrent = "[HasChildrent]";
        private readonly string tokenShowChildren = "[Children]";

        private async Task<string> UpdateGroupMenu(Menu dlGroup)
        {
            // Nếu menu null hoặc đã bị đánh dấu xóa thì trả về rỗng
            if (dlGroup == null || dlGroup.Delete) return "";
            // Nếu template chính hoặc template level1 không tồn tại, không thể build content
            if (dlGroup.Template == null || dlGroup.Template1 == null) return "";

            // Lấy các menu item đang active (Status = true) để build menu
            var list = (await _iMenuItemRepository.SearchAsync(true,0,0, m => m.MenuId == dlGroup.Id && m.Status == true)).OrderBy(m => m.Order).ToList();

            // Gọi hàm build mức lồng (level) để tạo ra HTML cho phần token Content
            var dlLop = LopUpdateGroupMenu(list, 0, dlGroup, 0);

            // Thay token [Content] trong Template bằng HTML sinh được và trả về
            return dlGroup.Template.Replace(tokenContent, dlLop);
        }
        /// <summary>
        /// Build the final `Menu.Content` HTML by combining the Menu templates with the active
        /// MenuItems. Returns the content string which is later stored on the Menu entity.
        /// - Calls `LopUpdateGroupMenu` which handles multi-level templates.
        /// </summary>
        private string LopUpdateGroupMenu(List<MenuItem> list, int idParent, Menu dlGroup, int level)
        {
            var str = new StringBuilder();
            // vòng lặp 1
            // Level 1: các item có ParentId == 0
            var ListMenuLevel1 = list.Where(m => m.ParentId == 0).OrderBy(m => m.Order);

            // Hàm TrimToken lấy phần '[For]...[/For]' bên trong template để lặp
            // TokenForLevelN là phần con được lặp cho mỗi item ở level tương ứng
            string TokenForLevel1 = Functions.TrimToken(dlGroup.Template1, "For") ?? "";
            string TokenForLevel2 = Functions.TrimToken(dlGroup.Template2, "For") ?? "";
            string TokenForLevel3 = Functions.TrimToken(dlGroup.Template3, "For") ?? "";

            // Nếu không có template level1 thì không thể build menu
            if (string.IsNullOrEmpty(dlGroup.Template1))
            {
                return "";
            }

            // stringlv1 sẽ chứa HTML các item level1 đã được xử lý (chứa cả HTML con nếu có)
            var stringlv1 = " ";

            // Duyệt từng item level1 và build phần con (level2 -> level3)
            foreach (var item1 in ListMenuLevel1)
            {
                if(TokenForLevel1 != "")
                {
                    // Build list level2 cho item1
                    var stringlv2 = " ";
                    var ListMenuLevel2 = list.Where(m => m.ParentId == item1.Id).OrderBy(m => m.Order);

                    if (!string.IsNullOrEmpty(dlGroup.Template2))
                    {
                        // Duyệt từng item level2
                        foreach (var item2 in ListMenuLevel2)
                        {
                            if (TokenForLevel2 != "")
                            {
                                var stringlv3 = " ";
                                // Kiểm tra vòng lặp không lồng nhau sai (safety)
                                if (item2.Id == item1.Id) return "";
                                var ListMenuLevel3 = list.Where(m => m.ParentId == item2.Id).OrderBy(m => m.Order);

                                if (!string.IsNullOrEmpty(dlGroup.Template3))
                                {
                                    // Build level3 nếu có template3
                                    foreach (var item3 in ListMenuLevel3)
                                    {
                                        if (TokenForLevel3 != "")
                                        {
                                            if (item3.Id == item2.Id) return ""; // safety
                                            // Thay token trong template level3 bằng giá trị thực tế từ item3
                                            stringlv3 += TokenForLevel3
                                                  .Replace(tokenUrl, item3.Href)
                                                  .Replace(tokenTarget, item3.Target)
                                                  .Replace(tokenIcon, item3.Icon)
                                                  .Replace(tokenName, item3.Name)
                                                  .Replace("[AddClass]", item3.Class)
                                                  .Replace(tokenHasChildrent, "");
                                        }
                                    }
                                }

                                // Thay token cho item level2 và chèn phần children level3 nếu có
                                stringlv2 += TokenForLevel2
                                   .Replace(tokenUrl, item2.Href)
                                   .Replace(tokenTarget, item2.Target)
                                   .Replace(tokenIcon, item2.Icon)
                                   .Replace(tokenName, item2.Name)
                                   .Replace("[AddClass]", item2.Class)
                                   .Replace(tokenShowChildren, ListMenuLevel3.Count() > 0 ? dlGroup.Template3.Replace(TokenForLevel3 == "" ? " " : TokenForLevel3, stringlv3) : "")
                                   .Replace(tokenHasChildrent, ListMenuLevel3.Count() > 0 ? dlGroup.HasChildrentClass2 : "");
                            }

                        }
                    }

                    // Thay token cho item level1 và chèn phần children level2 nếu có
                    stringlv1 += TokenForLevel1
                        .Replace(tokenUrl, item1.Href)
                        .Replace(tokenTarget, item1.Target)
                        .Replace(tokenIcon, item1.Icon)
                        .Replace(tokenName, item1.Name)
                         .Replace("[AddClass]", item1.Class)
                        .Replace(tokenShowChildren, ListMenuLevel2.Count() > 0 ? dlGroup.Template2.Replace(TokenForLevel2, stringlv2) : "")
                        .Replace(tokenHasChildrent, ListMenuLevel2.Count() > 0 ? dlGroup.HasChildrentClass1 : "");
                }
            }

            // Sau khi ghép xong nội dung các item, thay phần For trong Template1 bằng nội dung sinh được
            stringlv1 = dlGroup.Template1.Replace(TokenForLevel1, stringlv1).Replace("[For]", "").Replace("[/For]", "");
            return stringlv1;
        }
        /// <summary>
        /// Internal template engine that merges up to 3 levels of menu items into the provided
        /// Menu templates (`Template1`, `Template2`, `Template3`). The algorithm:
        /// - Iterates level 1 items (ParentId == 0), for each builds level 2 HTML and for each level 2
        ///   builds level 3 HTML. Tokens are replaced with MenuItem properties.
        /// - Returns the processed Template1 string with nested children injected.
        /// 
        /// Caution: Templates are string-based and rely on specific token names. Be careful when
        /// editing templates; malformed templates can cause incorrect HTML.
        /// </summary>
        #endregion


        [HttpPost, Authorize]
        public async Task<List<SelectListItem>> SearchLink(string q, string language = null, CategoryType? categoryType = null)
        {
            if (string.IsNullOrEmpty(q))
            {
                return new List<SelectListItem>();
            }
            return (await _iLinkRepository.SearchAsync(true, 0, 20, x => (x.Name.ToLower() == q.ToLower() || x.Name.ToLower().Contains(q.ToLower()) || q == null) && (x.Language == language || language == null) && (x.Type == categoryType || categoryType == null) && x.Status, x => x.OrderBy(y => y.Name),
                x => new Link { Id = x.Id, Name = x.Name, Status = x.Status, Type = x.Type, Language = x.Language  })).Select(x => new SelectListItem { Text = $"({x.Language}|{x.Type.GetDisplayName()}) / {x.Name}", Value = x.Id.ToString() }).ToList();
        }
    }
}