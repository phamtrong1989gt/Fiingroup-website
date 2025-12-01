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
    public class ContactManagerController : Base.Controllers.BaseController
    {

        private readonly ILogger _logger;
        private readonly IContactRepository _iContactRepository;
        private readonly IPortalRepository _iPortalRepository;
        private readonly IContentPageRepository _iContentPageRepository;
        public ContactManagerController(
            ILogger<ContactManagerController> logger,
            IContactRepository iContactRepository,
            IPortalRepository iPortalRepository,
            IContentPageRepository iContentPageRepository
        )
        {
            controllerName = "ContactManager";
            tableName = "Contact";
            _logger = logger;
            _iContactRepository = iContactRepository;
            _iPortalRepository = iPortalRepository;
            _iContentPageRepository = iContentPageRepository;
        }

        #region [Index]
        [AuthorizePermission]
        public async Task<IActionResult> Index(string language = null, int? portalId = 1)
        {
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            ViewData["PortalSelectList"] = new SelectList(portals, "Id", "Name");

            var listService = await _iContentPageRepository.SearchAsync(true, 0, 0, x=>x.Status && x.CategoryType == ECategoryType.ContentPage_Solution, x=>x.OrderBy(z=>z.Order));
            ViewData["SolutionSelectList"] = new SelectList(listService.Select(x=> new { Id = x.Id, Name = x.Name}), "Id", "Name");
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, bool? status, string startTime, string endTime, string ordertype = "asc", string orderby = "name")
        {
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iContactRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                    m =>
                        (m.FullName.Contains(key) || m.Email.Contains(key) || m.Phone.Contains(key) || m.Position.Contains(key) || m.ConpanyName.Contains(key) || key == null) &&
                        (m.Status == status || status == null) && m.Type == Contact.ContactType.Product
                       ,
                OrderByExtention(ordertype, orderby));
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
            if (!dl.Status)
            {
                dl.Status = true;
                _iContactRepository.Update(dl);
                await _iContactRepository.CommitAsync();
            }
            var model = MapModel<ContactModel>.Go(dl);
            return View(model);
        }
        #endregion
    }
}