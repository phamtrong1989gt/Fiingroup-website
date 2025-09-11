using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;
namespace PT.UI.Areas.Manager.Controllers
{
    [Area("User")]
    [IsSupperAdminAuthorizePermission]
    public class RoleGroupManagerController : Controller
    {
        private readonly ILogger _logger;
        private readonly IRoleGroupRepository _iRoleGroupRepository;
        private readonly IRoleControllerRepository _iRoleControllerRepository;
        public RoleGroupManagerController(
            ILogger<RoleGroupManagerController> logger,
            ILogRepository iLogRepository,
            IRoleGroupRepository iRoleGroupRepository,
            IRoleControllerRepository iRoleControllerRepository
        )
        {
            _logger = logger;
            _iRoleGroupRepository = iRoleGroupRepository;
            _iRoleControllerRepository = iRoleControllerRepository;
        }

        #region [Index]
        
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost, ActionName("Index")]
        
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, string ordertype, string orderby)
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iRoleGroupRepository.SearchPagedListAsync(
            page ?? 1,
            limit ?? 10,
            m => (m.Name.Contains(key) || key == null),
            OrderByExtention(ordertype, orderby));
            data.ReturnUrl = Url.Action("Index", new { page, limit, key, ordertype, orderby });
            return View("IndexAjax", data);
        }
        private Func<IQueryable<RoleGroup>, IOrderedQueryable<RoleGroup>> OrderByExtention(string ordertype, string orderby)
        {
            Func<IQueryable<RoleGroup>, IOrderedQueryable<RoleGroup>> functionOrder = null;
            switch (orderby)
            {
                case "name":
                    functionOrder = ordertype == "asc" ? EntityExtention<RoleGroup>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<RoleGroup>.OrderBy(m => m.OrderByDescending(x => x.Name));
                    break;
                default:
                    functionOrder = ordertype == "asc" ? EntityExtention<RoleGroup>.OrderBy(m => m.OrderBy(x => x.Order)) : EntityExtention<RoleGroup>.OrderBy(m => m.OrderByDescending(x => x.Order));
                    break;
            }
            return functionOrder;
        }
        #endregion

        #region [Create]
        [HttpGet]
        
        public IActionResult Create()
        {
            var dl = new RoleGroupModel();
            return View(dl);
        }
        [HttpPost, ActionName("Create")]
        
        public async Task<ResponseModel> CreatePost(RoleGroupModel use)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var data = new RoleGroup
                    {
                        Status = use.Status,
                        Name = use.Name,
                        Order = use.Order
                    };
                    await _iRoleGroupRepository.AddAsync(data);
                    await _iRoleGroupRepository.CommitAsync();
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
        
        public async Task<IActionResult> Edit(int id)
        {
            var dl = await _iRoleGroupRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null)
            {
                return View("404");
            }
            var model = MapModel<RoleGroupModel>.Go(dl);
            return View(model);
        }
        [HttpPost, ActionName("Edit")]
        
        public async Task<ResponseModel> EditPost(RoleGroupModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iRoleGroupRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    dl.Id = use.Id;
                    dl.Status = use.Status;
                    dl.Name = use.Name;
                    dl.Order = use.Order;
                    _iRoleGroupRepository.Update(dl);
                    await _iRoleGroupRepository.CommitAsync();

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
        public async Task<ResponseModel> DeletePost(int id)
        {
            try
            {
                // Kiểm tra dữ liệu đang sử dụng ở controller
                var ktC = await _iRoleControllerRepository.AnyAsync(x => x.GroupId == id);
                if(ktC)
                {
                    return new ResponseModel() { Output = 9, IsUse = true, Message = "Dữ liệu này đang được sử dụng ở quản lý quyền controller, vui lòng bỏ chọn dữ liệu này rồi thực hiện lại.", Type = ResponseTypeMessage.Warning };
                }
                var kt = await _iRoleGroupRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null)
                {
                    return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                _iRoleGroupRepository.Delete(kt);
                await _iRoleGroupRepository.CommitAsync();

                return new ResponseModel() { Output = 1, Message = "Xóa dữ liệu thành công.", Type = ResponseTypeMessage.Success, IsClosePopup=true };
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