using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.BE.Areas.Base.Controllers;
using PT.Shared;
using PT.Base;

namespace PT.BE.Areas.Manager.Controllers
{
    [Area("User")]
    [IsSupperAdminAuthorizePermission]
    public class RoleActionManagerController : BaseController
    {
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly ILogger _logger;
        private readonly LogSettings _logSettings;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly ILogRepository _iLogRepository;
        private readonly IRoleActionRepository _iRoleActionRepository;

        private readonly IRoleControllerRepository _iRoleControllerRepository;
        private readonly IRoleGroupRepository _iRoleGroupRepository;
        private readonly IRoleAreaRepository _iRoleAreaRepository;

        private readonly IRoleDetailRepository _iRoleDetailRepository;


        public RoleActionManagerController(
            ILogger<RoleActionManagerController> logger,
            IWebHostEnvironment hostingEnvironment,
            IOptions<BaseSettings> baseSettings,
            IWebHostEnvironment iHostingEnvironment,
            ILogRepository iLogRepository,
            IOptions<LogSettings> logSettings,
            IRoleActionRepository iRoleActionRepository,
            IRoleControllerRepository iRoleControllerRepository,
            IRoleGroupRepository iRoleGroupRepository,
            IRoleAreaRepository iRoleAreaRepository,
            IRoleDetailRepository iRoleDetailRepository
        )
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _baseSettings = baseSettings;
            _iHostingEnvironment = iHostingEnvironment;
            _iLogRepository = iLogRepository;
            _logSettings = logSettings.Value;
            _iRoleActionRepository = iRoleActionRepository;
            _iRoleControllerRepository = iRoleControllerRepository;
            _iRoleGroupRepository = iRoleGroupRepository;
            _iRoleAreaRepository = iRoleAreaRepository;
            _iRoleDetailRepository = iRoleDetailRepository;
        }

        #region [Index]
        public async Task<IActionResult> Index()
        {
            ViewData["RoleAreaSelectList"] = new SelectList(await _iRoleAreaRepository.SearchAsync(true,0,0,x => x.Status == true), "Id", "Name");
            return View();
        }
        [HttpPost, ActionName("Index")]
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, string ordertype, string orderby,string areaId,string controllerId)
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iRoleActionRepository.SearchPagedListAdvance(
            page ?? 1,
            limit ?? 10,
            m => (m.Name.Contains(key) || m.ActionName.Contains(key) || key == null)
            && (m.ControllerId== controllerId || controllerId == null)
            && (m.RoleController.AreaId == areaId || areaId == null)
            ,
            OrderByExtention(ordertype, orderby));
            data.ReturnUrl = Url.Action("Index", new { page, limit, key, ordertype, orderby, areaId, controllerId });
            return View("IndexAjax", data);
        }
        private Func<IQueryable<RoleAction>, IOrderedQueryable<RoleAction>> OrderByExtention(string ordertype, string orderby)
        {
            Func<IQueryable<RoleAction>, IOrderedQueryable<RoleAction>> functionOrder = null;
            switch (orderby)
            {
                case "order":
                    functionOrder = ordertype == "asc" ? EntityExtention<RoleAction>.OrderBy(m => m.OrderBy(x => x.Order)) : EntityExtention<RoleAction>.OrderBy(m => m.OrderByDescending(x => x.Order));
                    break;
                default:
                    functionOrder = ordertype == "asc" ? EntityExtention<RoleAction>.OrderBy(m => m.OrderBy(x => x.Order).OrderBy(x => x.ControllerId)) : EntityExtention<RoleAction>.OrderBy(m => m.OrderByDescending(x => x.Order).OrderBy(x => x.ControllerId));
                    break;
            }
            return functionOrder;
        }
        #endregion

        #region [Create]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var dl = new RoleActionModel
            {
                RoleAreaSelectList = new SelectList(await _iRoleAreaRepository.SearchAsync(true,0,0, x => x.Status == true), "Id", "Name")
            };
            return View(dl);
        }
        [HttpPost, ActionName("Create")]
        public async Task<ResponseModel> CreatePost(RoleActionModel use, string returnurl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var data = new RoleAction
                    {
                        ActionName = use.ActionName,
                        ControllerId = use.ControllerId,
                        Status = use.Status,
                        Name = use.Name,
                        Order = use.Order
                    };
                    await _iRoleActionRepository.AddAsync(data);
                    await _iRoleActionRepository.CommitAsync();
                  
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
        private class RoleAcctionDefaultModel
        {
            public int Stt { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
        }
        [HttpPost, ActionName("CreateAuto")]
        public async Task<ResponseModel> CreateAutoPost(string controllerId)
        {
            try
            {
                if(controllerId==null)
                {
                    return new ResponseModel() { Output = 1, Message = "Bạn chưa chọn quyền controller.", Type = ResponseTypeMessage.Warning, IsClosePopup = false };
                }
                var ktController = await _iRoleControllerRepository.AnyAsync(x => x.Id == controllerId);
                if(!ktController)
                {
                    return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                var listRole = new List<RoleAcctionDefaultModel> {
                    new RoleAcctionDefaultModel {Stt = 1,Code="Index",Name="Danh sách" },
                    new RoleAcctionDefaultModel {Stt = 2,Code="Create",Name="Thêm mới" },
                    new RoleAcctionDefaultModel {Stt = 3,Code="Edit",Name="Chỉnh sửa" },
                    new RoleAcctionDefaultModel {Stt = 4,Code="Delte",Name="Xóa" }
                };
                foreach(var item in listRole)
                {
                    var kt = await _iRoleActionRepository.AnyAsync(x => x.ControllerId == controllerId && x.Name == item.Code);
                    if(!kt)
                    {
                        var data = new RoleAction
                        {
                            ActionName = item.Name,
                            ControllerId = controllerId,
                            Status = true,
                            Name = item.Code,
                            Order = item.Stt
                        };
                        await _iRoleActionRepository.AddAsync(data);
                        await _iRoleActionRepository.CommitAsync();
                    }
                }
             

                return new ResponseModel() { Output = 1, Message = "Thêm mới quền mặc định thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
            var dl = await _iRoleActionRepository.SingleOrDefaultAsync(true, m => m.Id == id,x=>x.RoleController);
            if (dl == null)
            {
                return View("404");
            }
            var model = MapModel<RoleActionModel>.Go(dl);
            model.AreaId = dl.RoleController?.AreaId;
            model.RoleAreaSelectList = new SelectList(await _iRoleAreaRepository.SearchAsync(true,0,0,x => x.Status == true), "Id", "Name");
            return View(model);
        }
        [HttpPost, ActionName("Edit")]
        public async Task<ResponseModel> EditPost(RoleActionModel use, int id, string returnurl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iRoleActionRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    dl.ActionName = use.ActionName;
                    dl.ControllerId = use.ControllerId;
                    dl.Status = use.Status ;
                    dl.Name = use.Name;
                    dl.Order = use.Order;
                    dl.ControllerId = use.ControllerId;

                    _iRoleActionRepository.Update(dl);
                    await _iRoleActionRepository.CommitAsync();
                    
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
                var ktC = await _iRoleDetailRepository.AnyAsync(x => x.ActionId == id);
                if (ktC)
                {
                    return new ResponseModel() { Output = 9, IsUse = true, Message = "Dữ liệu này đang được sử dụng ở quản lý quyền tài khoản, vui lòng bỏ chọn dữ liệu này rồi thực hiện lại.", Type = ResponseTypeMessage.Warning };
                }
                var kt = await _iRoleActionRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null)
                {
                    return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                
                _iRoleActionRepository.Delete(kt);
                await _iRoleActionRepository.CommitAsync();
                return new ResponseModel() { Output = 1, Message = "Xóa dữ liệu thành công.", Type = ResponseTypeMessage.Success, IsClosePopup=true };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion
        #region [API]
        [HttpPost]
        public async Task<string> GetController(string areaId="0", string controllerId = "")
        {
            var data = await _iRoleControllerRepository.SearchAsync(true, 0,0,m => m.AreaId == areaId, x => x.OrderBy(m => m.Name));
            var strb = new StringBuilder();
            strb.Append($"<option value=''>Chọn controller</option>");
            foreach (var item in data)
            {
                strb.Append($"<option {((controllerId == item.Id) ? "selected" : "")} value='{item.Id}'>{item.Name}</option>");
            }
            return strb.ToString();
        }
        #endregion
    }
}