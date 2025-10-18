// -------------------
// Lưu ý: Các phần chú thích dưới đây được viết bằng tiếng Việt để giúp bạn dễ hiểu hơn về chức năng của từng đoạn mã.
// -------------------
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
using PT.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PT.BE.Areas.Manager.Controllers
{
    /// <summary>
    /// Controller quản lý Slide hình ảnh (PhotoSlide) trong khu vực Quản trị (Area = Manager).
    /// 
    /// Mô tả chung:
    /// - Cung cấp các action để thực hiện CRUD cho nhóm banner (Banner) có kiểu Slide và
    ///   quản lý các BannerItem (hình ảnh con).
    /// - Hỗ trợ phân biệt dữ liệu theo `PortalId` (dùng cho multi-portal / multi-site).
    /// - Sinh `Content` (HTML) từ template của Banner và các BannerItem, lưu vào trường `Banner.Content`
    ///   để tái sử dụng (giảm chi phí sinh lại) và gọi clear cache / generate module khi thay đổi.
    /// 
    /// Lưu ý cho người bàn giao:
    /// - Các action liên quan tới thay đổi items sẽ rebuild `Content` cho banner cha và lưu lại
    ///   vào database, đồng thời gọi `CommonFunctions.TriggerCacheModuleClear` để cập nhật cache.
    /// - Nếu mở rộng lên nhiều level hoặc cấu trúc template phức tạp hơn, hãy kiểm tra kỹ hàm
    ///   `UpdateGroupBanner` và `LopUpdateGroupBanner` vì đây là nơi thực hiện thay token.
    /// - Các repository được inject qua constructor: `IBannerRepository`, `IBannerItemRepository`, `IPortalRepository`.
    /// </summary>
    [Area("Manager")]
    public class PhotoSlideManagerController : Base.Controllers.BaseController
    {
        // Biến môi trường web
        private readonly IWebHostEnvironment _iHostingEnvironment;
        // Biến ghi log
        private readonly ILogger _logger;
        // Repository quản lý banner
        private readonly IBannerRepository _iBannerRepository;
        // Repository quản lý item của banner
        private readonly IBannerItemRepository _iBannerItemRepository;
        // Repository quản lý portal (dùng để phân biệt website)
        private readonly IPortalRepository _iPortalRepository;

        /// <summary>
        /// Hàm khởi tạo controller PhotoSlideManagerController.
        /// </summary>
        /// <param name="logger">Đối tượng ghi log.</param>
        /// <param name="iHostingEnvironment">Môi trường web.</param>
        /// <param name="iBannerRepository">Repository banner.</param>
        /// <param name="iBannerItemRepository">Repository item banner.</param>
        public PhotoSlideManagerController(
            ILogger<PhotoSlideManagerController> logger,
            IWebHostEnvironment iHostingEnvironment,
            IBannerRepository iBannerRepository,
            IBannerItemRepository iBannerItemRepository,
            IPortalRepository iPortalRepository
        )
        {
            // Gán tên controller và bảng dữ liệu
            controllerName = "PhotoSlideManager";
            tableName = "Banner";
            // Khởi tạo các biến dùng trong controller
            _logger = logger;
            _iHostingEnvironment = iHostingEnvironment;
            _iBannerRepository = iBannerRepository;
            _iBannerItemRepository = iBannerItemRepository;
            _iPortalRepository = iPortalRepository;
        }


        #region [Index]
        /// <summary>
        /// Hiển thị giao diện quản lý slide ảnh.
        /// </summary>
        [AuthorizePermission]
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách portal để hiển thị bộ lọc portal trên giao diện quản trị
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            ViewBag.PortalSelectList = new SelectList(portals, "Id", "Name");
            // Trả về view mặc định cho quản lý slide ảnh
            return View();
        }

        /// <summary>
        /// Xử lý tìm kiếm và phân trang slide ảnh (AJAX).
        /// </summary>
        /// <param name="id">Lọc theo ID banner.</param>
        /// <param name="page">Trang hiện tại.</param>
        /// <param name="limit">Số lượng mỗi trang.</param>
        /// <param name="code">Lọc theo mã banner.</param>
        /// <param name="key">Từ khóa tìm kiếm.</param>
        /// <param name="status">Lọc trạng thái.</param>
        /// <param name="language">Ngôn ngữ.</param>
        /// <param name="ordertype">Kiểu sắp xếp (asc/desc).</param>
        /// <param name="orderby">Trường sắp xếp.</param>
        [HttpPost, ActionName("Index")]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> IndexPost(int? id, int? page, int? limit, string code, string key, bool? status, string language = "vi", string ordertype = "asc", string orderby = "name", int? portalId = null)
        {
            // Kiểm tra tham số phân trang, nếu sai thì gán giá trị mặc định
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            // Tìm kiếm banner theo điều kiện truyền vào
            var data = await _iBannerRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                    m => (m.Name.Contains(key) || key == null) &&
                        (m.Language == language) &&
                        (m.PortalId == portalId || portalId == null) &&
                        (m.Status == status || status == null) &&
                        (m.Code == code || code == null) &&
                        m.Type == BannerType.Slide &&
                        !m.Delete
                        && (m.Id==id || id==null),
                OrderByExtention(ordertype, orderby));
            // Gắn portal entity cho từng banner trả về để hiển thị tên portal trên UI
            var portals = await _iPortalRepository.SearchAsync(true);
            foreach (var item in data.Data)
            {
                item.Portal = portals.FirstOrDefault(x => x.Id == item.PortalId);
            }
            data.ReturnUrl = Url.Action("Index", new { page, limit, key, ordertype, orderby });
            // Trả về view ajax với dữ liệu đã lọc
            return View("IndexAjax", data);
        }

        /// <summary>
        /// Hàm trả về function sắp xếp banner theo trường và kiểu sắp xếp.
        /// </summary>
        /// <param name="ordertype">Kiểu sắp xếp (asc/desc).</param>
        /// <param name="orderby">Trường sắp xếp.</param>
        /// <returns>Function sắp xếp.</returns>
        private Func<IQueryable<Banner>, IOrderedQueryable<Banner>> OrderByExtention(string ordertype, string orderby)
        {
            // Khởi tạo biến functionOrder để xác định cách sắp xếp
            Func<IQueryable<Banner>, IOrderedQueryable<Banner>> functionOrder = null;
            switch (orderby)
            {
                case "name":
                    // Sắp xếp theo tên
                    functionOrder = ordertype == "asc" ? EntityExtention<Banner>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<Banner>.OrderBy(m => m.OrderByDescending(x => x.Name));
                    break;
                default:
                    // Sắp xếp theo Id nếu không chọn trường name
                    functionOrder = ordertype == "asc" ? EntityExtention<Banner>.OrderBy(m => m.OrderBy(x => x.Id)) : EntityExtention<Banner>.OrderBy(m => m.OrderByDescending(x => x.Id));
                    break;
            }
            return functionOrder;
        }
        #endregion

        #region [Create]
        /// <summary>
        /// Hiển thị giao diện tạo mới banner.
        /// </summary>
        /// <param name="language">Ngôn ngữ banner mới.</param>
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Create(string language = "vi")
        {
            // Khởi tạo model banner với ngôn ngữ truyền vào
            var dl = new BannerModel
            {
                Language = language
            };
            // Gắn danh sách portal để người quản trị chọn (nếu hệ thống có nhiều portal)
            // Lưu ý: ở đây dùng .Result vì action GET, nhưng nếu muốn tránh block thread có thể
            // chuyển sang async/await hoàn toàn. View sẽ lấy SelectList từ ViewBag.
            var portals = _iPortalRepository.SearchAsync(true,0,0).Result;
            ViewBag.PortalSelectList = new SelectList(portals, "Id", "Name");
            // Trả về view tạo mới banner
            return View(dl);
        }

        /// <summary>
        /// Xử lý tạo mới banner.
        /// </summary>
        /// <param name="use">Model banner cần tạo.</param>
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(BannerModel use)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Kiểm tra mã banner đã tồn tại chưa
                    var checkCode = await _iBannerRepository.SingleOrDefaultAsync(false, m => m.Code == use.Code && m.Type == BannerType.Slide && m.Language == use.Language && m.PortalId == use.PortalId);
                    if (checkCode != null)
                    {
                        // Nếu mã đã tồn tại thì trả về cảnh báo
                        return new ResponseModel() { Output = 0, Message = "Mã slide ảnh đã tồn tại, vui lòng chọn mã khác.", Type = ResponseTypeMessage.Warning };
                    }
                    // Tạo mới đối tượng banner
                    var data = new Banner
                    {
                        Name = use.Name,
                        Delete = false,
                        Status = use.Status,
                        Language = use.Language,
                        Template = use.Template,
                        Type = BannerType.Slide,
                        ClassActive = use.ClassActive,
                        Code = use.Code,
                        PortalId = use.PortalId
                    };
                    // Thêm banner vào database
                    await _iBannerRepository.AddAsync(data);
                    await _iBannerRepository.CommitAsync();
                    // Tạo module cho banner mới
                    var content = await UpdateGroupBanner(data);
                    data.Content = content;
                    _iBannerRepository.Update(data);
                    await _iBannerRepository.CommitAsync();
                    CommonFunctions.TriggerCacheModuleClear(content, ModuleType.PhotoSlide, data.Code,data.Language, data.PortalId);

                    // Ghi log thao tác thêm mới
                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới slide ảnh \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    // Trả về kết quả thành công
                    return new ResponseModel() { Output = 1, Message = "Thêm mới slide ảnh thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                // Nếu dữ liệu chưa đủ thì trả về cảnh báo
                return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu có exception
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            // Trả về lỗi hệ thống
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [Edit]
        /// <summary>
        /// Hiển thị giao diện chỉnh sửa banner.
        /// </summary>
        /// <param name="id">ID banner cần chỉnh sửa.</param>
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Edit(int id)
        {
            // Lấy thông tin banner theo id
            var dl = await _iBannerRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            // Nếu không tồn tại hoặc đã bị xóa thì trả về trang 404
            if (dl == null || (dl != null && dl.Delete))
            {
                return View("404");
            }
            // Map dữ liệu sang model để hiển thị lên view
            var model = MapModel<BannerModel>.Go(dl);
            // Đưa danh sách portal vào ViewBag để view select danh sách portal
            var portals = await _iPortalRepository.SearchAsync(true,0,0);
            ViewBag.PortalSelectList = new SelectList(portals, "Id", "Name");
            return View(model);
        }

        /// <summary>
        /// Xử lý cập nhật banner.
        /// </summary>
        /// <param name="use">Model banner cập nhật.</param>
        /// <param name="id">ID banner cần cập nhật.</param>
        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(BannerModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Lấy banner cần cập nhật theo id
                    var dl = await _iBannerRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    // Nếu không tồn tại hoặc đã bị xóa thì trả về cảnh báo
                    if (dl == null || (dl != null && dl.Delete))
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }
                    // Kiểm tra mã banner đã tồn tại chưa
                    var checkCode = await _iBannerRepository.SingleOrDefaultAsync(false, m => m.Code == use.Code && m.Type == BannerType.Slide && m.Language == use.Language && m.PortalId == use.PortalId && m.Id != id);
                    if (checkCode != null)
                    {
                        // Nếu mã đã tồn tại thì trả về cảnh báo
                        return new ResponseModel() { Output = 0, Message = "Mã slide ảnh đã tồn tại, vui lòng chọn mã khác.", Type = ResponseTypeMessage.Warning };
                    }

                    // Cập nhật các trường dữ liệu của banner
                    dl.Name = use.Name;
                    dl.Status = use.Status;
                    dl.Language = use.Language;
                    dl.Template = use.Template;
                    dl.Code = use.Code;
                    dl.ClassActive = use.ClassActive;
                    dl.PortalId = use.PortalId;

                    // Lưu thay đổi vào database
                    _iBannerRepository.Update(dl);
                    await _iBannerRepository.CommitAsync();

                    // Cập nhật module cho banner
                    var content = await UpdateGroupBanner(dl);
                    dl.Content = content;
                    _iBannerRepository.Update(dl);
                    await _iBannerRepository.CommitAsync();
                    CommonFunctions.TriggerCacheModuleClear(content, ModuleType.PhotoSlide, dl.Code,dl.Language, dl.PortalId);

                    // Ghi log thao tác cập nhật
                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật slide ảnh \"{dl.Name}\".",
                        Type = LogType.Edit
                    });
                    // Trả về kết quả thành công
                    return new ResponseModel() { Output = 1, Message = "Cập nhật slide ảnh thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = false };
                }
                // Nếu dữ liệu chưa đủ thì trả về cảnh báo
                return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin.", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu có exception
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            // Trả về lỗi hệ thống
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [Delete]
        /// <summary>
        /// Xử lý xóa banner.
        /// </summary>
        /// <param name="id">ID banner cần xóa.</param>
        [HttpPost, ActionName("Delete")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeletePost(int id)
        {
            try
            {
                // Lấy banner cần xóa theo id
                var kt = await _iBannerRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                // Nếu không tồn tại hoặc đã bị xóa thì trả về cảnh báo
                if (kt == null || (kt != null && kt.Delete))
                {
                    return new ResponseModel() { Output = 0, Message = "Slide ảnh không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                // Đánh dấu banner là đã xóa
                kt.Delete = true;

                // Cập nhật lại group banner
                var contentDelete = await UpdateGroupBanner(kt);
                kt.Content = contentDelete;
                _iBannerRepository.Update(kt);

                // Lưu thay đổi vào database
                await _iBannerRepository.CommitAsync();

                CommonFunctions.TriggerCacheModuleClear(null, ModuleType.PhotoSlide, kt.Code, kt.Language, kt.PortalId);

                // Ghi log thao tác xóa
                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa slide ảnh \"{kt.Name}\".",
                    Type = LogType.Delete
                });

                // Trả về kết quả thành công
                return new ResponseModel() { Output = 1, Message = "Xóa slide ảnh thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu có exception
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            // Trả về lỗi hệ thống
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [DeleteItem]
        /// <summary>
        /// Xử lý xóa item của banner (ảnh slide).
        /// </summary>
        /// <param name="id">ID item cần xóa.</param>
        [HttpPost, ActionName("DeleteItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeleteItemPost(int id)
        {
            try
            {
                // Lấy item cần xóa theo id
                var kt = await _iBannerItemRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                // Nếu không tồn tại thì trả về cảnh báo
                if (kt == null)
                {
                    return new ResponseModel() { Output = 0, Message = "Banner không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                // Xóa item khỏi database
                _iBannerItemRepository.Delete(kt);
                // Lấy thông tin banner cha để cập nhật lại module
                var dataParrent = await _iBannerRepository.SingleOrDefaultAsync(true, x => x.Id == kt.BannerId);
                if(dataParrent != null)
                {
                    var content = await UpdateGroupBanner(dataParrent);
                    dataParrent.Content = content;
                    _iBannerRepository.Update(dataParrent);
                    CommonFunctions.TriggerCacheModuleClear(null, ModuleType.PhotoSlide, dataParrent.Code, dataParrent.Language, dataParrent.PortalId);
                    await _iBannerRepository.CommitAsync();
                }
                await _iBannerItemRepository.CommitAsync();
                // Ghi log thao tác xóa
                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa ảnh slide \"{kt.Name}\".",
                    Type = LogType.Delete
                });

                // Trả về kết quả thành công
                return new ResponseModel() { Output = 1, Message = "Xóa ảnh thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = false };
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu có exception
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            // Trả về lỗi hệ thống
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [Functions]
        /// <summary>
        /// Chuyển đổi danh sách DataSortModel dạng cây sang danh sách phẳng, cập nhật parent và thứ tự.
        /// </summary>
        /// <param name="list">Danh sách cần chuyển đổi.</param>
        /// <param name="parentId">ID cha.</param>
        /// <returns>Danh sách phẳng DataSortModel.</returns>
        private List<DataSortModel> ConverData(List<DataSortModel> list, int parentId = 0)
        {
            // Biến order dùng để đánh số thứ tự cho các item
            int order = 1;
            var newList = new List<DataSortModel>();
            foreach (var item in list)
            {
                // Gán parentId cho item hiện tại
                item.ParentId = parentId;
                // Gán thứ tự cho item
                item.Order = order;
                // Thêm item vào danh sách mới
                newList.Add(item);
                // Nếu item có con thì tiếp tục chuyển đổi các con
                if (item.Children.Count() > 0)
                {
                    newList.AddRange(ConverData(item.Children, item.Id));
                }
                order++;
            }
            // Trả về danh sách đã chuyển đổi
            return newList;
        }

        /// <summary>
        /// Hiển thị cây các item của banner dưới dạng HTML.
        /// </summary>
        /// <param name="list">Danh sách item banner.</param>
        /// <param name="parrentId">ID cha.</param>
        /// <returns>Chuỗi HTML thể hiện cây.</returns>
        private string ShowTree(List<BannerItem> list, int parrentId)
        {
            // Nếu không có dữ liệu thì trả về thông báo
            if (list.Count() == 0)
            {
                return "<span>Hiện tại chưa có dữ liệu</span>";
            }
            var listCulture = Shared.ListData.ListLanguage;
            StringBuilder str = new System.Text.StringBuilder();
            if (list.Count() > 0)
            {
                // Bắt đầu danh sách cây
                str.Append($"<ol class=\"dd-list\">");
                foreach (var item in list.OrderBy(x => x.Order))
                {
                    // Nếu item là cha thì bỏ qua để tránh lặp vô hạn
                    if (item.Id == parrentId)
                    {
                        return "";
                    }
                    // Hiển thị thông tin item
                    str.Append($"<li class=\"dd-item {(item.Status ? "treeTrue" : "")} dd3-item\" data-id=\"{item.Id}\">");
                    str.Append($"<div class=\"dd-handle dd3-handle\"></div>");
                    str.Append($"<div class=\"dd3-content\">");
                    str.Append($"{item.Name}");
                    str.Append($"<span class=\"button-icon\"><a button-static href='{Url.Action("EditItem", new { id = item.Id })}'  title=\"Cập nhật\"><i class=\"material-icons iconcontrol text-primary\">edit</i></a><span>");
                    str.Append("</div>");
                    // Đệ quy hiển thị các con của item
                    str.Append(ShowTree(list, item.Id));
                    str.Append("</li>");
                }
                // Kết thúc danh sách cây
                str.Append($"</ol>");
            }
            // Trả về chuỗi HTML cây
            return str.ToString();
        }

        /// <summary>
        /// Lấy cây HTML các item của banner theo ID banner.
        /// </summary>
        /// <param name="id">ID banner.</param>
        /// <returns>Chuỗi HTML các item.</returns>
        [HttpGet]
        public async Task<string> Items(int id)
        {
            // Lấy danh sách item theo bannerId
            var list = await _iBannerItemRepository.SearchAsync(true, 0, 0, x => x.BannerId == id);
            // Trả về cây HTML các item
            return ShowTree(list, 0);
        }
        #endregion

        #region [CreateItem]
        /// <summary>
        /// Hiển thị giao diện tạo mới item cho banner.
        /// </summary>
        /// <param name="bannerId">ID banner cần thêm item.</param>
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult CreateItem2(int bannerId = 0)
        {
            // Khởi tạo model item với bannerId truyền vào
            var dl = new BannerItemModel
            {
                BannerId = bannerId
            };
            // Trả về view tạo mới item
            return View(dl);
        }

        /// <summary>
        /// Xử lý tạo mới item cho banner.
        /// </summary>
        /// <param name="use">Model item cần tạo.</param>
        [HttpPost, ActionName("CreateItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreateItemPost(BannerItemModel use)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Lấy thứ tự lớn nhất cho item mới
                    var maxOrder = _iBannerItemRepository.MaxOrder(x => x.BannerId == use.BannerId);
                    // Tạo mới đối tượng item
                    var data = new BannerItem
                    {
                        Name = use.Name,
                        Status = use.Status,
                        Order = maxOrder + 1,
                        Href = use.Href,
                        BannerId = use.BannerId,
                        Banner = use.Banner,
                        Template = use.Template,
                        Target = use.Target,
                        Content = use.Content
                    };
                    // Thêm item vào database
                    await _iBannerItemRepository.AddAsync(data);
                    await _iBannerItemRepository.CommitAsync();

                    // Lấy thông tin banner cha để cập nhật lại module
                    var dataParrent = await _iBannerRepository.SingleOrDefaultAsync(true, x => x.Id == data.BannerId);
                    if(dataParrent != null)
                    {
                        var content = await UpdateGroupBanner(dataParrent);
                        dataParrent.Content = content;
                        _iBannerRepository.Update(dataParrent);
                        await _iBannerRepository.CommitAsync();
                        CommonFunctions.TriggerCacheModuleClear(content, ModuleType.PhotoSlide, dataParrent.Code, dataParrent.Language, dataParrent.PortalId);
                    }    
                  
                    // Ghi log thao tác thêm mới
                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm ảnh slide \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    // Trả về kết quả thành công
                    return new ResponseModel() { Output = 1, Message = "Thêm mới ảnh thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = false };
                }
                // Nếu dữ liệu chưa đủ thì trả về cảnh báo
                return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu có exception
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            // Trả về lỗi hệ thống
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [EditItem]
        /// <summary>
        /// Hiển thị giao diện chỉnh sửa item của banner.
        /// </summary>
        /// <param name="id">ID item cần chỉnh sửa.</param>
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> EditItem(int id)
        {
            // Lấy thông tin item theo id
            var dl = await _iBannerItemRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            // Nếu không tồn tại thì trả về trang 404
            if (dl == null)
            {
                return View("404");
            }
            // Map dữ liệu sang model để hiển thị lên view
            var model = MapModel<BannerItemModel>.Go(dl);
            return View(model);
        }

        /// <summary>
        /// Xử lý cập nhật item của banner.
        /// </summary>
        /// <param name="use">Model item cập nhật.</param>
        /// <param name="id">ID item cần cập nhật.</param>
        [HttpPost, ActionName("EditItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditItemPost(BannerItemModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Lấy item cần cập nhật theo id
                    var dl = await _iBannerItemRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    // Nếu không tồn tại thì trả về cảnh báo
                    if (dl == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }
                    // Cập nhật các trường dữ liệu của item
                    dl.Name = use.Name;
                    dl.Status = use.Status;
                    dl.Href = use.Href;
                    dl.Template = use.Template;
                    dl.Target = use.Target;
                    dl.Banner = use.Banner;
                    dl.Content = use.Content;
                    // Lưu thay đổi vào database
                    _iBannerItemRepository.Update(dl);
                    await _iBannerItemRepository.CommitAsync();
                    // Lấy thông tin banner cha để cập nhật lại module
                    // Lấy thông tin banner cha để cập nhật lại module
                    var dataParrent = await _iBannerRepository.SingleOrDefaultAsync(true, x => x.Id == dl.BannerId);
                    if (dataParrent != null)
                    {
                        var content = await UpdateGroupBanner(dataParrent);
                        dataParrent.Content = content;
                        _iBannerRepository.Update(dataParrent);
                        await _iBannerRepository.CommitAsync();
                        CommonFunctions.TriggerCacheModuleClear(content, ModuleType.PhotoSlide, dataParrent.Code, dataParrent.Language, dataParrent.PortalId);
                    }

                    // Ghi log thao tác cập nhật
                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật ảnh slide \"{dl.Name}\".",
                        Type = LogType.Edit
                    });
                    // Trả về kết quả thành công
                    return new ResponseModel() { Output = 1, Message = "Cập nhật ảnh thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = false };
                }
                // Nếu dữ liệu chưa đủ thì trả về cảnh báo
                return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin.", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu có exception
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            // Trả về lỗi hệ thống
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }

        /// <summary>
        /// Cập nhật thứ tự các item của banner.
        /// </summary>
        /// <param name="data">Danh sách DataSortModel dạng chuỗi.</param>
        /// <param name="id">ID banner.</param>
        [HttpPost, ActionName("UpdateOrder")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> UpdateOrderPost([FromBody]string data, int id)
        {
            try
            {
                // Chuyển đổi chuỗi dữ liệu sang danh sách DataSortModel
                var listItem = ConverData(Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataSortModel>>(data));
                // Lấy danh sách id các item cần cập nhật
                var listItemIds = listItem.Select(x => x.Id).ToList();
                // Lấy danh sách item từ database
                var listCa = await _iBannerItemRepository.SearchAsync(false, 0, 0, x => listItemIds.Contains(x.Id));
                foreach (var item in listCa)
                {
                    // Tìm item trong danh sách đã chuyển đổi để lấy thứ tự mới
                    var objIn = listItem.FirstOrDefault(x => x.Id == item.Id);
                    item.Order = objIn.Order;
                    // Cập nhật thứ tự cho item
                    _iBannerItemRepository.Update(item);
                }
                // Lưu thay đổi vào database
                await _iBannerItemRepository.CommitAsync();
                // Ghi log thao tác cập nhật thứ tự
                await AddLog(new LogModel
                {
                    ObjectId = id,
                    ActionTime = DateTime.Now,
                    Name = $"Cập nhật thứ tự trình diễn ảnh #\"{id}\".",
                    Type = LogType.Edit
                });
                // Lấy thông tin banner cha để cập nhật lại module
                var dataParrent = await _iBannerRepository.SingleOrDefaultAsync(true, x => x.Id == id);
                CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupBanner(dataParrent), ModuleType.PhotoSlide, dataParrent.Code , dataParrent?.Language);
                // Trả về kết quả thành công
                return new ResponseModel() { Output = 1, Message = "Cập nhật thành công.", Type = ResponseTypeMessage.Success };
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu có exception
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            // Trả về lỗi hệ thống
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        private readonly string TokenFor = "For";
        private readonly string TokenForPage = "ForPage";
        private readonly string TokenI = "[I]";
        private readonly string TokenUrl = "[Url]";
        private readonly string TokenTarget = "[Target]";
        private readonly string TokenName = "[Name]";
        private readonly string TokenImg = "[Img]";
        private readonly string TokenNote = "[Note]";
        public string TokenClassActive = "[ClassActive]";

        /// <summary>
        /// Cập nhật HTML cho group banner bằng cách áp dụng template lên các item đang sử dụng.
        /// </summary>
        /// <param name="dlGroup">Banner group cần cập nhật.</param>
        /// <returns>Chuỗi HTML sinh ra.</returns>
        private async Task<string> UpdateGroupBanner(Banner dlGroup)
        {
            // Kiểm tra banner có hợp lệ không
            if (dlGroup == null || dlGroup.Delete) return "";
            if (dlGroup.Template == null) return "";
            // Lấy danh sách item đang sử dụng của banner
            var list = (await _iBannerItemRepository.SearchAsync(true, 0, 0, m => m.BannerId == dlGroup.Id && m.Status == true)).OrderBy(m => m.Order).ToList();
            // Áp dụng template lên danh sách item
            var output = LopUpdateGroupBanner(list,dlGroup);
            // Lưu nội dung đã sinh vào trường Content để dùng lại (tiết kiệm thao tác sinh lại)
            dlGroup.Content = output;
            return output;
        }

        /// <summary>
        /// Áp dụng template lên danh sách item của banner và trả về HTML.
        /// </summary>
        /// <param name="list">Danh sách item banner.</param>
        /// <param name="dlGroup">Banner group.</param>
        /// <returns>Chuỗi HTML sinh ra.</returns>
        private string LopUpdateGroupBanner(List<BannerItem> list, Banner dlGroup)
        {
            // Lấy template cho từng item và từng trang
            string _TokenFor = Functions.TrimToken(dlGroup.Template, TokenFor);
            string _TokenForPage = Functions.TrimToken(dlGroup.Template, TokenForPage);
            int i = 0;
            var StrFor = new StringBuilder();
            var StrForPage = new StringBuilder();
            foreach (var item in list)
            {
                // Nếu có template cho từng item thì áp dụng
                if (!string.IsNullOrEmpty(_TokenFor))
                {
                    if (i == 0)
                    {
                        StrFor.Append(_TokenFor
                         .Replace(TokenUrl, item.Href)
                         .Replace(TokenTarget, item.Target).Replace(TokenNote, item.Content)
                         .Replace(TokenImg, item.Banner)
                         .Replace(TokenName, item.Name)
                         .Replace(TokenClassActive, dlGroup.ClassActive)
                         .Replace(TokenI, i.ToString())
                         );
                    }
                    else
                    {
                        StrFor.Append(_TokenFor
                         .Replace(TokenUrl, item.Href)
                         .Replace(TokenTarget, item.Target).Replace(TokenNote, item.Content)
                         .Replace(TokenImg, item.Banner)
                         .Replace(TokenName, item.Name)
                         .Replace(TokenI, i.ToString())
                         );
                    }
                }
                // Nếu có template cho từng trang thì áp dụng
                if (!string.IsNullOrEmpty(_TokenForPage))
                {
                    if (i == 0)
                    {
                        StrForPage.Append(_TokenForPage
                         .Replace(TokenUrl, item.Href)
                         .Replace(TokenTarget, item.Target)
                         .Replace(TokenImg, item.Banner)
                         .Replace(TokenName, item.Name)
                          .Replace(TokenNote, item.Content)
                         .Replace(TokenClassActive, dlGroup.ClassActive).Replace(TokenI, i.ToString())
                         );
                    }
                    else
                    {
                        StrForPage.Append(_TokenForPage
                         .Replace(TokenUrl, item.Href)
                         .Replace(TokenTarget, item.Target)
                         .Replace(TokenImg, item.Banner)
                         .Replace(TokenName, item.Name)
                          .Replace(TokenNote, item.Content)
                         .Replace(TokenI, i.ToString())
                         );
                    }
                }
                i++;
            }
            // Thay thế template bằng chuỗi đã sinh ra
            string OutPut = dlGroup.Template;
            if (!string.IsNullOrEmpty(_TokenFor))
            {
                OutPut = OutPut.Replace(_TokenFor, StrFor.ToString()).Replace("[" + TokenFor + "]", "").Replace("[/" + TokenFor + "]", "");
            }
            if (!string.IsNullOrEmpty(_TokenForPage))
            {
                OutPut = OutPut.Replace(_TokenForPage, StrForPage.ToString()).Replace("[" + TokenForPage + "]", "").Replace("[/" + TokenForPage + "]", "");
            }
            // Trả về chuỗi HTML đã sinh ra
            return OutPut;
        }
    }
}