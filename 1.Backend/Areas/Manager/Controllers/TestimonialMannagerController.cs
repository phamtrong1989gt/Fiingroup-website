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

namespace PT.UI.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class TestimonialMannagerController : Base.Controllers.BaseController
    {

        private readonly ILogger _logger;
        private readonly IContactRepository _iContactRepository;

        public TestimonialMannagerController(
            ILogger<TestimonialMannagerController> logger,
            IContactRepository iContactRepository
        )
        {
            controllerName = "TestimonialMannager";
            tableName = "Contact";
            _logger = logger;
            _iContactRepository = iContactRepository;
        }

        #region [Index]
        [AuthorizePermission]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(int? page, int? limit, bool? isHome, string key, bool? status, string ordertype = "asc", string orderby = "name", string language = "vi")
        {
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iContactRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                    m => (m.FullName.Contains(key) || m.Email.Contains(key) || m.Phone.Contains(key) || key == null) 
                    && (m.Status == status || status == null)
                     && (m.IsHome == isHome || isHome == null)
                        && (m.Type == Contact.ContactType.Testimonial || m.Type == Contact.ContactType.TestimonialVideo) && 
                        !m.Delete && m.Language == language,
                OrderByExtention(ordertype, orderby));
            data.ReturnUrl = Url.Action("Index",
                new
                {
                    page,
                    limit,
                    key,
                    status,
                    ordertype,
                    orderby
                });
            return View("IndexAjax", data);
        }

        private Func<IQueryable<Contact>, IOrderedQueryable<Contact>> OrderByExtention(string ordertype, string orderby)
        {
            Func<IQueryable<Contact>, IOrderedQueryable<Contact>> functionOrder = null;
            switch (orderby)
            {
                case "sta":
                    functionOrder = ordertype == "asc" ? EntityExtention<Contact>.OrderBy(m => m.OrderBy(x => x.Rating)) : EntityExtention<Contact>.OrderBy(m => m.OrderByDescending(x => x.Rating));
                    break;
                default:
                    functionOrder = ordertype == "asc" ? EntityExtention<Contact>.OrderBy(m => m.OrderBy(x => x.Id)) : EntityExtention<Contact>.OrderBy(m => m.OrderByDescending(x => x.Id));
                    break;
            }
            return functionOrder;
        }
        #endregion

        #region [Create]
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Create(string language="vi")
        {
            var dl = new TestimonialModel
            {
                Language = language
            };
            return View(dl);
        }
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(TestimonialModel use)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var data = new Contact
                    {
                        FullName = use.FullName,
                        Content = use.Content,
                        Delete = false,
                        Email = use.Email,
                        Rating = use.Rating ?? 1,
                        IsHome = use.IsHome,
                        Avatar = use.Avatar,
                        Type = use.Type,
                        Language = use.Language,
                        Status = true,
                        CreatedDate = DateTime.Now,
                        AppointmentDate = DateTime.Now,
                        Address = use.Address
                    };
                    await _iContactRepository.AddAsync(data);
                    await _iContactRepository.CommitAsync();

                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới đánh giá khách hàng \"#{data.Id}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
            var dl = await _iContactRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null || (dl != null && dl.Delete && dl.Type == Contact.ContactType.Testimonial))
            {
                return View("404");
            }
            var model = MapModel<TestimonialModel>.Go(dl);
            return View(model);
        }
        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(TestimonialModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iContactRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null || (dl != null && dl.Delete))
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }
                    dl.Type = use.Type;
                    dl.FullName = use.FullName;
                    dl.Content = use.Content;
                    dl.Delete = false;
                    dl.Email = use.Email;
                    dl.Rating = use.Rating ?? 1;
                    dl.IsHome = use.IsHome;
                    dl.Avatar = use.Avatar;
                    dl.Status = true;
                    dl.CreatedDate = DateTime.Now;
                    dl.Address = use.Address;

                    dl.AppointmentDate = DateTime.Now;
                    _iContactRepository.Update(dl);
                    await _iContactRepository.CommitAsync();

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật đánh giá khách hàng \"#{dl.Id}\".",
                        Type = LogType.Edit
                    });

                    return new ResponseModel() { Output = 1, Message = "Cập nhật thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
                var kt = await _iContactRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null || (kt != null && kt.Delete && kt.Type==Contact.ContactType.Testimonial))
                {
                    return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                kt.Delete = true;
                await _iContactRepository.CommitAsync();
                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa đánh giá khách hàng \"#{kt.Id}\".",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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