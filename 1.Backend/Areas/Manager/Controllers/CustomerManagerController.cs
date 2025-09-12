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
    public class CustomerManagerController : Base.Controllers.BaseController
    {
        private readonly ILogger _logger;
        private readonly ICustomerRepository _iCustomerRepository;

        public CustomerManagerController(
            ILogger<CustomerManagerController> logger,
            ICustomerRepository iCustomerRepository
        )
        {
            controllerName = "CustomerManager";
            tableName = "Customer";
            _logger = logger;
            _iCustomerRepository = iCustomerRepository;
        }

        #region [Index]
        [AuthorizePermission]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, int? categoryId, int? tagId, bool? status, string ordertype = "asc", string orderby = "name")
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iCustomerRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                    m =>(m.FullName.Contains(key) || key == null) && 
                        (m.Status==status || status ==null) && 
                        !m.Delete,
                OrderByExtention(ordertype, orderby));
                data.ReturnUrl =  Url.Action("Index", 
                    new {
                        page,
                        limit,
                        key,
                        categoryId,
                        tagId,
                        status,
                        ordertype,
                        orderby
            });
            return View("IndexAjax", data);
        }
        private Func<IQueryable<Customer>, IOrderedQueryable<Customer>> OrderByExtention(string ordertype, string orderby)
        {
            Func<IQueryable<Customer>, IOrderedQueryable<Customer>> functionOrder = null;
            switch (orderby)
            {
                case "name":
                    functionOrder = ordertype == "asc" ? EntityExtention<Customer>.OrderBy(m => m.OrderBy(x => x.FullName)) : EntityExtention<Customer>.OrderBy(m => m.OrderByDescending(x => x.FullName));
                    break;
                default:
                    functionOrder = ordertype == "asc" ? EntityExtention<Customer>.OrderBy(m => m.OrderBy(x => x.Id)) : EntityExtention<Customer>.OrderBy(m => m.OrderByDescending(x => x.Id));
                    break;
            }
            return functionOrder;
        }
        #endregion

        #region [Create]
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Create()
        {
            var dl = new CustomerModel
            {
            };
            return View(dl);
        }
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(CustomerModel use)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check tài khoản tồn tại theo email
                    var check = await _iCustomerRepository.AnyAsync(x => x.Email == use.Email && !x.Delete);
                    if(check)
                    {
                        return new ResponseModel() { Output = 1, Message = "Email đã có người sử dụng, vui lòng chọn email khác.", Type = ResponseTypeMessage.Warning, IsClosePopup = false };
                    }
                    var data = new Customer
                    {
                        FullName = use.FullName,
                        Banner = use.Banner,
                        Content = use.Content,
                        Delete = false,
                        Status = use.Status,
                        Email = use.Email,
                        Gender = use.Gender,
                        Phone = use.Phone,
                        Birthday = use.Birthday,
                        ContactCount = 0,
                        Address = use.Address,
                        Country = use.Country
                    };
                    await _iCustomerRepository.AddAsync(data);
                    await _iCustomerRepository.CommitAsync();

                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới khách hàng \"{data.FullName}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới khách hàng thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
            var dl = await _iCustomerRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null || (dl != null && dl.Delete))
            {
                return View("404");
            }
            var model = MapModel<CustomerModel>.Go(dl);
            return View(model);
        }
        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(CustomerModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iCustomerRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null || (dl != null && dl.Delete))
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    // Check tài khoản tồn tại theo email
                    var check = await _iCustomerRepository.AnyAsync(x => x.Email == use.Email && !x.Delete && x.Id!=id);
                    if (check)
                    {
                        return new ResponseModel() { Output = 1, Message = "Email đã có người sử dụng, vui lòng chọn email khác.", Type = ResponseTypeMessage.Warning, IsClosePopup = false };
                    }
                    dl.FullName = use.FullName;
                    dl.Banner = use.Banner;
                    dl.Content = use.Content;
                    dl.Status = use.Status;
                    dl.Email = use.Email;
                    dl.Gender = use.Gender;
                    dl.Phone = use.Phone;
                    dl.Birthday = use.Birthday;
                    dl.ContactCount = 0;
                    dl.Address = use.Address;
                    dl.Country = use.Country;
                    _iCustomerRepository.Update(dl);
                    await _iCustomerRepository.CommitAsync();
                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật khách hàng \"{dl.FullName}\".",
                        Type = LogType.Edit
                    });

                    return new ResponseModel() { Output = 1, Message = "Cập nhật khách hàng thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
                var kt = await _iCustomerRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null || (kt != null && kt.Delete))
                {
                    return new ResponseModel() { Output = 0, Message = "khách hàng không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                kt.Delete = true;
                await _iCustomerRepository.CommitAsync();

                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa khách hàng \"{kt.FullName}\".",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa khách hàng thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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