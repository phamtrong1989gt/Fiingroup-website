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
    public class ServicePriceManagerController : Base.Controllers.BaseController
    {

        private readonly ILogger _logger;
        private readonly IServicePriceRepository _iServicePriceRepository;
        private readonly ICategoryRepository _iCategoryRepository;

        public ServicePriceManagerController(
            ILogger<ServicePriceManagerController> logger,
            IServicePriceRepository iServicePriceRepository,
            ICategoryRepository iCategoryRepository
        )
        {
            controllerName = "ServicePriceManager";
            tableName = "ServicePrice";
            _logger = logger;
            _iServicePriceRepository = iServicePriceRepository;
            _iCategoryRepository = iCategoryRepository;
        }

        #region [Index]
        [AuthorizePermission]
        public IActionResult Index(string language = "vi")
        {
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(int? page, string key, bool? status, string language = "vi")
        {
            var data = await _iServicePriceRepository.SearchPagedListAsync(
                page ?? 1,
                99999999,
                    m => (m.Name.Contains(key) || key == null)
                        && (m.Status == status || status == null)
                        && !m.Delete && m.Language == language,
               x => x.OrderBy(m => m.Order));
            return View("IndexAjax", data);
        }


        [HttpPost, ActionName("IndexItem")]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> IndexItemPost(int? page, int? parentId)
        {
            var data = await _iServicePriceRepository.SearchPagedListAsync(page ?? 1, 10, m => m.ParentId == parentId && !m.Delete , x => x.OrderBy(m => m.Order));
            return View("IndexItemAjax", data);
        }
        #endregion

        #region [Create]
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Create(string language = "vi")
        {
            var model = new CategoryServicePriceModel
            {
                Language = language
            };
            return View(model);
        }
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(CategoryServicePriceModel use)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var data = new ServicePrice
                    {
                        Language = use.Language,
                        Delete = false,
                        Name = use.Name,
                        Order = use.Order,
                        Status = use.Status,
                        ParentId = 0
                    };
                    await _iServicePriceRepository.AddAsync(data);
                    await _iServicePriceRepository.CommitAsync();

                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới danh mục giá dịch vụ #{data.Id}",
                        Type = LogType.Create
                    });
                    return new ResponseModel() { Output = 1, Message = "Thêm mới danh mục giá dịch vụ thành công", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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

        #region [CreateItem]
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult CreateItem(int parentId, string language = "vi")
        {
            var model = new ServicePriceModel
            {
                Language = language,
                ParentId = parentId
            };
            return View(model);
        }
        [HttpPost, ActionName("CreateItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreateItemPost(ServicePriceModel use)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var data = new ServicePrice
                    {
                        Language = use.Language,
                        Delete = false,
                        Name = use.Name,
                        Order = use.Order,
                        Status = use.Status,
                        ParentId = use.ParentId,
                        ToPrice = use.ToPrice,
                        FromPrice = use.FromPrice,
                        Type = use.Type,
                        SubParent = use.SubParent,
                        ContactPrice = use.ContactPrice
                    };
                    await _iServicePriceRepository.AddAsync(data);
                    await _iServicePriceRepository.CommitAsync();

                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới giá dịch vụ #{data.Id}",
                        Type = LogType.Create
                    });
                    return new ResponseModel() { Output = 1, Message = "Thêm mới giá dịch vụ thành công", Type = ResponseTypeMessage.Success, IsClosePopup = false };
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
            var dl = await _iServicePriceRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null)
            {
                return View("404");
            }
            var model = MapModel<CategoryServicePriceModel>.Go(dl);
            return View(model);
        }
        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(CategoryServicePriceModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iServicePriceRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }
                    dl.Name = use.Name;
                    dl.Order = use.Order;
                    dl.Status = use.Status;
                    _iServicePriceRepository.Update(dl);
                    await _iServicePriceRepository.CommitAsync();

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật danh mục giá dịch vụ #{dl.Id}",
                        Type = LogType.Edit
                    });

                    return new ResponseModel() { Output = 1, Message = "Cập nhật dữ liệu thành công", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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

        #region [EditItem]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> EditItem(int id)
        {
            var dl = await _iServicePriceRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null)
            {
                return View("404");
            }
            var model = MapModel<ServicePriceModel>.Go(dl);
            return View(model);
        }
        [HttpPost, ActionName("EditItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditItemPost(ServicePriceModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iServicePriceRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }
                    dl.Name = use.Name;
                    dl.Order = use.Order;
                    dl.Status = use.Status;
                    dl.Type = use.Type;
                    dl.FromPrice = use.FromPrice;
                    dl.ToPrice = use.ToPrice;
                    dl.SubParent = use.SubParent;
                    dl.ContactPrice = use.ContactPrice;
                    _iServicePriceRepository.Update(dl);
                    await _iServicePriceRepository.CommitAsync();

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật giá dịch vụ #{dl.Id}",
                        Type = LogType.Edit
                    });

                    return new ResponseModel() { Output = 1, Message = "Cập nhật dữ liệu thành công", Type = ResponseTypeMessage.Success, IsClosePopup = false };
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
        [HttpPost, ActionName("Delete")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeletePost(int id)
        {
            try
            {
                var kt = await _iServicePriceRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null)
                {
                    return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại", Type = ResponseTypeMessage.Warning };
                }
                kt.Delete = true;
                _iServicePriceRepository.Update(kt);
                await _iServicePriceRepository.CommitAsync();

                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa giá dịch vụ #{kt.Id}",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa dữ liệu thành công", Type = ResponseTypeMessage.Success, IsClosePopup = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion
    }
}