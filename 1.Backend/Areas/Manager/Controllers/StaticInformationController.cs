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

namespace PT.BE.Areas.Manager.Controllers
{
    [Area("Manager")]
    /// <summary>
    /// Controller quản lý thông tin tĩnh (Static Information) cho khu vực Manager.
    /// Bao gồm các chức năng: xem danh sách, tạo mới, chỉnh sửa, xóa thông tin tĩnh.
    /// </summary>
    public class StaticInformationManagerController : Base.Controllers.BaseController
    {
        // Biến môi trường hosting, dùng cho thao tác file hệ thống
        private readonly IWebHostEnvironment _iHostingEnvironment;
        // Logger ghi log hệ thống
        private readonly ILogger _logger;
        // Cấu hình log
        private readonly LogSettings _logSettings;
        // Cấu hình cơ bản của hệ thống
        private readonly IOptions<BaseSettings> _baseSettings;
        // Repository ghi log
        private readonly ILogRepository _iLogRepository;
        // Repository thao tác dữ liệu thông tin tĩnh
        private readonly IStaticInformationRepository _iStaticInformationRepository;
        private readonly IPortalRepository _iPortalRepository;
        /// <summary>
        /// Hàm khởi tạo controller, inject các dịch vụ cần thiết
        /// </summary>
        public StaticInformationManagerController(
            ILogger<StaticInformationManagerController> logger,
            IOptions<BaseSettings> baseSettings,
            IWebHostEnvironment iHostingEnvironment,
            ILogRepository iLogRepository,
            IOptions<LogSettings> logSettings,
            IOptions<EmailSettings> emailSettings,
            IStaticInformationRepository iStaticInformationRepository,
            IPortalRepository iPortalRepository
        )
        {
            controllerName = "StaticInformationManager";
            tableName = "StaticInformation";
            _logger = logger;
            _baseSettings = baseSettings;
            _iHostingEnvironment = iHostingEnvironment;
            _iLogRepository = iLogRepository;
            _logSettings = logSettings.Value;
            _iStaticInformationRepository = iStaticInformationRepository;
            _iPortalRepository = iPortalRepository;
        }

        #region [Index]
        /// <summary>
        /// Hiển thị trang danh sách thông tin tĩnh
        /// </summary>
        /// <param name="language">Ngôn ngữ hiển thị (mặc định: "vi")</param>
        [AuthorizePermission]
        public IActionResult Index(string language = "vi")
        {
            return View();
        }

        /// <summary>
        /// Xử lý tìm kiếm, phân trang danh sách thông tin tĩnh (AJAX)
        /// </summary>
        /// <param name="id">Id thông tin tĩnh</param>
        /// <param name="page">Trang hiện tại</param>
        /// <param name="limit">Số lượng bản ghi/trang</param>
        /// <param name="key">Từ khóa tìm kiếm theo tên</param>
        /// <param name="code">Mã thông tin tĩnh</param>
        /// <param name="status">Trạng thái sử dụng</param>
        /// <param name="language">Ngôn ngữ</param>
        /// <param name="ordertype">Kiểu sắp xếp (asc/desc)</param>
        /// <param name="orderby">Trường sắp xếp (name/id)</param>
        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(int? id, int? page, int? limit, string key, string code, bool? status, int? portalId, string language = "vi", string ordertype = "asc", string orderby = "name")
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var portals = await _iPortalRepository.SearchAsync(true);
            var data = await _iStaticInformationRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                m => (m.Name.Contains(key) || key == null) &&
                     (m.Code == code || code == null) &&
                     (m.Status == status || status == null) &&
                     (m.PortalId == portalId || portalId == null) &&
                     (m.Language == language) &&
                     !m.Delete && (m.Id == id || id == null),
                OrderByExtention(ordertype, orderby));
            foreach (var item in data.Data)
            {
                item.Portal = portals.FirstOrDefault(x=>x.Id == item.PortalId);
            }
            return View("IndexAjax", data);
        }

        /// <summary>
        /// Hàm hỗ trợ sắp xếp danh sách thông tin tĩnh
        /// </summary>
        /// <param name="ordertype">Kiểu sắp xếp (asc/desc)</param>
        /// <param name="orderby">Trường sắp xếp (name/id)</param>
        /// <returns>Hàm sắp xếp cho IQueryable</returns>
        private Func<IQueryable<StaticInformation>, IOrderedQueryable<StaticInformation>> OrderByExtention(string ordertype, string orderby)
        {
            Func<IQueryable<StaticInformation>, IOrderedQueryable<StaticInformation>> functionOrder = null;
            switch (orderby)
            {
                case "name":
                    functionOrder = ordertype == "asc" ? EntityExtention<StaticInformation>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<StaticInformation>.OrderBy(m => m.OrderByDescending(x => x.Name));
                    break;
                default:
                    functionOrder = ordertype == "asc" ? EntityExtention<StaticInformation>.OrderBy(m => m.OrderBy(x => x.Id)) : EntityExtention<StaticInformation>.OrderBy(m => m.OrderByDescending(x => x.Id));
                    break;
            }
            return functionOrder;
        }
        #endregion

        #region [Create]
        /// <summary>
        /// Hiển thị giao diện tạo mới thông tin tĩnh
        /// </summary>
        /// <param name="language">Ngôn ngữ</param>
        /// <param name="parrentId">Id cha (nếu có)</param>
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Create(string language = "vi", int parrentId = 0)
        {
            // StaticInformationModel: Model dùng để tạo mới thông tin tĩnh
            // Các thuộc tính:
            // - Name: Tên thông tin tĩnh
            // - Code: Mã thông tin tĩnh
            // - Content: Nội dung
            // - Status: Trạng thái sử dụng
            // - Language: Ngôn ngữ
            // - Delete: Đánh dấu đã xóa
            var dl = new StaticInformationModel
            {
                Language = language
            };
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
            return View(dl);
        }

        /// <summary>
        /// Xử lý tạo mới thông tin tĩnh
        /// </summary>
        /// <param name="use">Model thông tin tĩnh cần tạo mới</param>
        /// <returns>ResponseModel: Kết quả thực hiện (Output, Message, Type, ...)</returns>
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(StaticInformationModel use)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Kiểm tra mã đã tồn tại chưa
                    var checkCode = await _iStaticInformationRepository.SingleOrDefaultAsync(false, m => m.Code == use.Code && !m.Delete && m.PortalId != use.PortalId);
                    if (checkCode != null)
                    {
                        return CreateResponse(0, "Mã đã tồn tại, vui lòng kiểm tra lại.", ResponseTypeMessage.Warning);
                    }

                    // StaticInformation: Entity lưu vào database
                    // Các thuộc tính:
                    // - Name: Tên thông tin tĩnh
                    // - Content: Nội dung
                    // - Delete: Đánh dấu đã xóa
                    // - Status: Trạng thái sử dụng
                    // - Language: Ngôn ngữ
                    // - Code: Mã thông tin tĩnh
                    var data = new StaticInformation
                    {
                        Name = use.Name,
                        Content = use.Content,
                        Delete = false,
                        Status = use.Status,
                        Language = use.Language,
                        Code = use.Code,
                        PortalId =use.PortalId
                    };
                    await _iStaticInformationRepository.AddAsync(data);
                    await _iStaticInformationRepository.CommitAsync();
                    CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, data.Content, ModuleType.StaticInformation, data.Code, data.Language);
                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới thông tin tĩnh \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    return CreateResponse(1, "Thêm mới thông tin tĩnh thành công ", ResponseTypeMessage.Success, true, false);
                }
                return CreateResponse(0, "Bạn chưa nhập đầy đủ thông tin", ResponseTypeMessage.Warning);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return CreateResponse(-1, "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", ResponseTypeMessage.Danger);
        }
        #endregion

        #region [Edit]
        /// <summary>
        /// Hiển thị giao diện chỉnh sửa thông tin tĩnh
        /// </summary>
        /// <param name="id">Id thông tin tĩnh cần chỉnh sửa</param>
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Edit(int id)
        {
            var dl = await _iStaticInformationRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null || (dl != null && dl.Delete))
            {
                return View("404");
            }
            var model = MapModel<StaticInformationModel>.Go(dl);
            return View(model);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin tĩnh
        /// </summary>
        /// <param name="use">Model thông tin tĩnh cần cập nhật</param>
        /// <param name="id">Id thông tin tĩnh</param>
        /// <param name="categoryIds">Danh sách Id danh mục liên quan</param>
        /// <param name="tags">Danh sách tag liên quan</param>
        /// <returns>ResponseModel: Kết quả thực hiện</returns>
        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Edit")]
        public async Task<ResponseModel> EditPost(StaticInformationModel use, int id, string categoryIds, string tags)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Kiểm tra mã đã tồn tại chưa (trừ chính bản ghi đang sửa)
                    var checkCode = await _iStaticInformationRepository.SingleOrDefaultAsync(false, m => m.Code == use.Code && m.Id != id && !m.Delete && m.PortalId != use.PortalId);
                    if (checkCode != null)
                    {
                        return CreateResponse(0, "Mã đã tồn tại, vui lòng kiểm tra lại.", ResponseTypeMessage.Warning);
                    }
                    var dl = await _iStaticInformationRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null || (dl != null && dl.Delete))
                    {
                        return CreateResponse(0, "Dữ liệu không tồn tại, vui lòng thử lại.", ResponseTypeMessage.Warning, false, false);
                    }

                    // Cập nhật các thuộc tính của thông tin tĩnh
                    dl.Name = use.Name;
                    dl.Code = use.Code;
                    dl.Content = use.Content;
                    dl.Status = use.Status;
                    dl.Language = use.Language;
                    dl.PortalId = use.PortalId;

                    _iStaticInformationRepository.Update(dl);
                    await _iStaticInformationRepository.CommitAsync();

                    CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, dl.Content, ModuleType.StaticInformation, dl.Code, dl.Language);

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật thông tin tĩnh \"{dl.Name}\".",
                        Type = LogType.Edit
                    });
                    return CreateResponse(1, "Cập nhật thông tin tĩnh thành công.", ResponseTypeMessage.Success, false, false);
                }
                return CreateResponse(-2, "Bạn chưa nhập đầy đủ thông tin.", ResponseTypeMessage.Warning);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return CreateResponse(-1, "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", ResponseTypeMessage.Danger);
        }
        #endregion

        #region [Delete]
        /// <summary>
        /// Xử lý xóa thông tin tĩnh
        /// </summary>
        /// <param name="id">Id thông tin tĩnh cần xóa</param>
        /// <returns>ResponseModel: Kết quả thực hiện</returns>
        [HttpPost, ActionName("Delete")]
        [AuthorizePermission("Delete")]
        public async Task<ResponseModel> DeletePost(int id)
        {
            try
            {
                var kt = await _iStaticInformationRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null || (kt != null && kt.Delete))
                {
                    return CreateResponse(0, "thông tin tĩnh không tồn tại, vui lòng thử lại.", ResponseTypeMessage.Warning, true);
                }

                CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, "", ModuleType.StaticInformation, kt.Code, kt.Language);

                kt.Delete = true;
                await _iStaticInformationRepository.CommitAsync();

                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa thông tin tĩnh \"{kt.Name}\".",
                    Type = LogType.Delete
                });

                return CreateResponse(1, "Xóa thông tin tĩnh thành công.", ResponseTypeMessage.Success, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return CreateResponse(-1, "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", ResponseTypeMessage.Danger);
        }
        #endregion
    }
}