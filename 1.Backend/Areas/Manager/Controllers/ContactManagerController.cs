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
    public class ContactManagerController : Base.Controllers.BaseController
    {

        private readonly ILogger _logger;
        private readonly IContactRepository _iContactRepository;

        public ContactManagerController(
            ILogger<ContactManagerController> logger,
            IContactRepository iContactRepository
        )
        {
            controllerName = "ContactManager";
            tableName = "Contact";
            _logger = logger;
            _iContactRepository = iContactRepository;
        }

        #region [Index]
        [AuthorizePermission]
        public IActionResult Index(string language = "vi")
        {
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, bool? status, string ordertype = "asc", string orderby = "name")
        {
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iContactRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                    m => (m.FullName.Contains(key) || m.Email.Contains(key) || m.Phone.Contains(key) || key == null) &&
                        (m.Status == status || status == null) && m.Type==Contact.ContactType.Contact &&
                        !m.Delete,
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
                case "name":
                    functionOrder = ordertype == "asc" ? EntityExtention<Contact>.OrderBy(m => m.OrderBy(x => x.FullName)) : EntityExtention<Contact>.OrderBy(m => m.OrderByDescending(x => x.FullName));
                    break;
                default:
                    functionOrder = ordertype == "asc" ? EntityExtention<Contact>.OrderBy(m => m.OrderBy(x => x.Id)) : EntityExtention<Contact>.OrderBy(m => m.OrderByDescending(x => x.Id));
                    break;
            }
            return functionOrder;
        }
        #endregion

        #region [Details]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Details(int id)
        {
            var dl = await _iContactRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null || (dl != null && dl.Delete))
            {
                return View("404");
            }
            if(!dl.Status)
            {
                dl.Status = true;
                _iContactRepository.Update(dl);
                await _iContactRepository.CommitAsync();
            }
            var model = MapModel<ContactModel>.Go(dl);
            return View(model);
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
                if (kt == null || (kt != null && kt.Delete))
                {
                    return new ResponseModel() { Output = 0, Message = "khách hàng không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                kt.Delete = true;
                await _iContactRepository.CommitAsync();
                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa liên hệ khách hàng \"{kt.FullName}\".",
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