using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.Domain.Model;
using PT.Infrastructure.Repositories;
using PT.Infrastructure.Interfaces;
using PT.BE.Areas.Base.Controllers;
using PT.Shared;
using static PT.Base.DataUserInfo;
using PT.Base;

namespace PT.BE.Areas.Manager.Controllers
{
    [Area("User")]
    [IsSupperAdminAuthorizePermission]
    public class RoleAreaManagerController : Controller
    {
        private readonly ILogger _logger;
        private readonly IRoleAreaRepository _iRoleAreaRepository;
        private readonly IRoleControllerRepository _iRoleControllerRepository;
        public RoleAreaManagerController(
            ILogger<RoleAreaManagerController> logger,
            ILogRepository iLogRepository,
            IRoleAreaRepository iRoleAreaRepository,
            IRoleControllerRepository iRoleControllerRepository
        )
        {
            _logger = logger;
            _iRoleAreaRepository = iRoleAreaRepository;
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
            var data = await _iRoleAreaRepository.SearchPagedListAsync(
            page ?? 1,
            limit ?? 10,
            m => (m.Name.Contains(key) || key == null || m.Id.Contains(key)),
            OrderByExtention(ordertype, orderby));
            data.ReturnUrl = Url.Action("Index", new { page, limit, key, ordertype, orderby });
            return View("IndexAjax", data);
        }
        private Func<IQueryable<RoleArea>, IOrderedQueryable<RoleArea>> OrderByExtention(string ordertype, string orderby)
        {
            Func<IQueryable<RoleArea>, IOrderedQueryable<RoleArea>> functionOrder = null;
            switch (orderby)
            {
                case "name":
                    functionOrder = ordertype == "asc" ? EntityExtention<RoleArea>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<RoleArea>.OrderBy(m => m.OrderByDescending(x => x.Name));
                    break;
                default:
                    functionOrder = ordertype == "asc" ? EntityExtention<RoleArea>.OrderBy(m => m.OrderBy(x => x.Order)) : EntityExtention<RoleArea>.OrderBy(m => m.OrderByDescending(x => x.Order));
                    break;
            }
            return functionOrder;
        }
        #endregion

        #region [Create]
        [HttpGet]

        public IActionResult Create()
        {
            var dl = new RoleAreaModel();
            return View(dl);
        }
        [HttpPost, ActionName("Create")]

        public async Task<ResponseModel> CreatePost(RoleAreaModel use)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var ktId = await _iRoleAreaRepository.AnyAsync(m => m.Id == use.Id);
                    if (ktId)
                    {
                        return new ResponseModel() { Output = 1, Message = "Mã quyền area đã tồn tại", Type = ResponseTypeMessage.Warning, IsClosePopup = false };
                    }
                    var data = new RoleArea
                    {
                        Id = use.Id,
                        Status = use.Status,
                        Name = use.Name,
                        Order = use.Order
                    };
                    await _iRoleAreaRepository.AddAsync(data);
                    await _iRoleAreaRepository.CommitAsync();
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
            var dl = await _iRoleAreaRepository.SingleOrDefaultAsync(true,m => m.Id == id);
            if (dl == null)
            {
                return View("404");
            }
            var model = MapModel<RoleAreaModel>.Go(dl);
            return View(model);
        }
        [HttpPost, ActionName("Edit")]

        public async Task<ResponseModel> EditPost(RoleAreaModel use, string id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iRoleAreaRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    dl.Status = use.Status;
                    dl.Name = use.Name;
                    dl.Order = use.Order;
                    _iRoleAreaRepository.Update(dl);
                    await _iRoleAreaRepository.CommitAsync();
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
                var ktC = await _iRoleControllerRepository.AnyAsync(x => x.AreaId == id);
                if (ktC)
                {
                    return new ResponseModel() { Output = 9, IsUse = true, Message = "Dữ liệu này đang được sử dụng ở quản lý quyền controller, vui lòng bỏ chọn dữ liệu này rồi thực hiện lại.", Type = ResponseTypeMessage.Warning };
                }

                var kt = await _iRoleAreaRepository.SingleOrDefaultAsync(false,m => m.Id == id);
                if (kt == null)
                {
                    return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                _iRoleAreaRepository.Delete(kt);
                await _iRoleAreaRepository.CommitAsync();

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