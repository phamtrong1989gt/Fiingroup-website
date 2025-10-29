using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.UI.Areas.Base.Controllers;
using PT.Shared;
using PT.Base;

namespace PT.UI.Areas.Manager.Controllers
{
    [Area("User")]
    [IsSupperAdminAuthorizePermission]
    public class RoleControllerManagerController : BaseController
    {
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly ILogger _logger;
        private readonly LogSettings _logSettings;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly ILogRepository _iLogRepository;
        private readonly IRoleControllerRepository _iRoleControllerRepository;
        private readonly IRoleGroupRepository _iRoleGroupRepository;
        private readonly IRoleAreaRepository _iRoleAreaRepository;
        private readonly IRoleActionRepository _iRoleActionRepository;

        public RoleControllerManagerController(
            ILogger<RoleControllerManagerController> logger,
            IWebHostEnvironment hostingEnvironment,
            IOptions<BaseSettings> baseSettings,
            IWebHostEnvironment iHostingEnvironment,
            ILogRepository iLogRepository,
            IOptions<LogSettings> logSettings,
            IRoleControllerRepository iRoleControllerRepository,
            IRoleGroupRepository iRoleGroupRepository,
            IRoleAreaRepository iRoleAreaRepository,
            IRoleActionRepository iRoleActionRepository
        )
        {
            controllerName = "Role";
            tableName = "User";
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _baseSettings = baseSettings;
            _iHostingEnvironment = iHostingEnvironment;
            _iLogRepository = iLogRepository;
            _logSettings = logSettings.Value;
            _iRoleControllerRepository = iRoleControllerRepository;
            _iRoleGroupRepository = iRoleGroupRepository;
            _iRoleAreaRepository = iRoleAreaRepository;
            _iRoleActionRepository = iRoleActionRepository;
        }
        #region [Index]
        public async Task<IActionResult> Index()
        {
            ViewData["RoleGroupSelectListItem"] = new SelectList(await _iRoleGroupRepository.SearchAsync(true, 0, 0,x => x.Status == true), "Id", "Name");
            ViewData["RoleAreaSelectList"] = new SelectList(await _iRoleAreaRepository.SearchAsync(true, 0, 0,x => x.Status == true), "Id", "Name");
            return View();
        }
        [HttpPost, ActionName("Index")]
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, string ordertype, string orderby, int? groupId, string areaId)
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iRoleControllerRepository.SearchPagedListAsync(
            page ?? 1,
            limit ?? 10,
            m => (m.Name.Contains(key) || key == null)
            &&(m.GroupId== groupId|| groupId==null)
            && (m.AreaId == areaId || areaId == null)
            ,
            OrderByExtention(ordertype, orderby),null,x=>x.RoleGroups,x=>x.RoleArea);
            data.ReturnUrl = Url.Action("Index", new { page, limit, key, ordertype, orderby });
            return View("IndexAjax", data);
        }
        private Func<IQueryable<RoleController>, IOrderedQueryable<RoleController>> OrderByExtention(string ordertype, string orderby)
        {
            Func<IQueryable<RoleController>, IOrderedQueryable<RoleController>> functionOrder = null;
            switch (orderby)
            {
                case "createdDate":
                    functionOrder = ordertype == "asc" ? EntityExtention<RoleController>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<RoleController>.OrderBy(m => m.OrderByDescending(x => x.Name));
                    break;
                case "order":
                    functionOrder = ordertype == "asc" ? EntityExtention<RoleController>.OrderBy(m => m.OrderBy(x => x.Order)) : EntityExtention<RoleController>.OrderBy(m => m.OrderByDescending(x => x.Order));
                    break;
                default:
                    functionOrder = ordertype == "asc" ? EntityExtention<RoleController>.OrderBy(m => m.OrderBy(x=>x.Order).OrderBy(x => x.GroupId)) : EntityExtention<RoleController>.OrderBy(m => m.OrderByDescending(x => x.Order).OrderBy(x => x.GroupId));
                    break;
            }
            return functionOrder;
        }
        #endregion
        #region [Create]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View(new RoleControllerModel
            {
                RoleGroupSelectList = new SelectList(await _iRoleGroupRepository.SearchAsync(true, 0, 0,x => x.Status == true), "Id", "Name"),
                RoleAreaSelectList = new SelectList(await _iRoleAreaRepository.SearchAsync(true, 0, 0,x => x.Status == true), "Id", "Name")
            });
        }
        [HttpPost, ActionName("Create")]
        public async Task<ResponseModel> CreatePost(RoleControllerModel use, string returnurl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var ktId = await _iRoleControllerRepository.AnyAsync(m => m.Id == use.Id);
                    if (ktId)
                    {
                        return new ResponseModel() { Output = 10, Message = "Mã này đã tồn tại, vui lòng chọn mã khác", Type = ResponseTypeMessage.Warning, IsClosePopup = false };
                    }
                    var data = new RoleController
                    {
                        Id = use.Id,
                        AreaId = use.AreaId,
                        GroupId = use.GroupId,
                        Status = use.Status,
                        Name = use.Name,
                        Order = use.Order
                    };
                    await _iRoleControllerRepository.AddAsync(data);
                    await _iRoleControllerRepository.CommitAsync();
                    
                    return new ResponseModel() { Output = 1, Message = "Thêm mới dữ liệu thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
        public async Task<IActionResult> Edit(string id)
        {
            var dl = await _iRoleControllerRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null)
            {
                return View("404");
            }
            var model = MapModel<RoleControllerModel>.Go(dl);
            model.RoleGroupSelectList = new SelectList(await _iRoleGroupRepository.SearchAsync(true, 0, 0, x => x.Status == true), "Id", "Name");
            model.RoleAreaSelectList = new SelectList(await _iRoleAreaRepository.SearchAsync(true, 0, 0,x => x.Status == true), "Id", "Name");
            return View(model);
        }
        [HttpPost, ActionName("Edit")]
        public async Task<ResponseModel> EditPost(RoleControllerModel use, string id, string returnurl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iRoleControllerRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }
                    dl.AreaId = use.AreaId;
                    dl.GroupId = use.GroupId;
                    dl.Status = use.Status;
                    dl.Name = use.Name;
                    dl.Order = use.Order;
                    _iRoleControllerRepository.Update(dl);
                    await _iRoleControllerRepository.CommitAsync();
                    
                    return new ResponseModel() { Output = 1, Message = "Cập nhật dữ liệu thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
        public async Task<ResponseModel> DeletePost(string id)
        {
            try
            {
                // Kiểm tra dữ liệu đang sử dụng ở controller
                var ktC = await _iRoleActionRepository.AnyAsync(x => x.ControllerId == id);
                if (ktC)
                {
                    return new ResponseModel() { Output = 9, IsUse = true, Message = "Dữ liệu này đang được sử dụng ở quản lý quyền action, vui lòng bỏ chọn dữ liệu này rồi thực hiện lại.", Type = ResponseTypeMessage.Warning };
                }

                var kt = await _iRoleControllerRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null)
                {
                    return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                _iRoleControllerRepository.Delete(kt);
                await _iRoleControllerRepository.CommitAsync();
                return new ResponseModel() { Output = 1, Message = "Xóa dữ liệu thành công.", Type = ResponseTypeMessage.Success,IsClosePopup=true };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion
    }
}