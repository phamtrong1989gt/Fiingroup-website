using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using PT.Shared;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.UI.Areas.Base.Controllers;
using PT.Base;

namespace PT.UI.Areas.Log.Controllers
{
    [Area("User")]
    [AuthorizePermission]
    public class LogManagerController : BaseController
    {
        private readonly ILogRepository _ILogRepository;
        private readonly IRoleControllerRepository _IAspNetRoleControllerRepository;

        public LogManagerController(
            ILogRepository ILogRepository, IRoleControllerRepository IAspNetRoleControllerRepository
            )
        {
            controllerName = "LogManager";
            tableName = "User";
            _ILogRepository = ILogRepository;
            _IAspNetRoleControllerRepository = IAspNetRoleControllerRepository;
        }
        #region [Index]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["PageSelectListItem"] = new SelectList(await _IAspNetRoleControllerRepository.SearchAsync(true, 0, 0, m => m.Status == true), "Id", "Name");
            ViewData["TypeSelectListItem"] = new SelectList(
                new List<SelectListItem>() {
                    new SelectListItem {Text="Create",Value=((int)LogType.Create).ToString() },
                    new SelectListItem {Text="Edit",Value=((int)LogType.Edit).ToString() },
                    new SelectListItem {Text="Delete",Value=((int)LogType.Delete).ToString() }
                }
                , "Value", "Text");
            return View();
        }
        [HttpPost, ActionName("Index")]
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, int? type, string pageManager, string startTime, string endTime, string ordertype, string orderby)
        {
            ViewData["ListController"] = await _IAspNetRoleControllerRepository.SearchAsync(true, 0, 0, null);
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            DateTime? _startTime = Convert.ToDateTime(startTime).Date;
            DateTime? _endTime = Convert.ToDateTime(endTime).Date.AddDays(1);
            LogType _type = (LogType)(type ?? -1);
            var data = await _ILogRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                m => (m.Name.Contains(key) || key == null || m.AcctionUser.Contains(key))
                && (m.ObjectType == pageManager || pageManager == null)
                && (m.Type == _type || type == null)
                && (startTime == null || m.ActionTime >= _startTime.Value)
                && (endTime == null || m.ActionTime <= _endTime.Value),
                OrderByExtention(ordertype, orderby), m => new Domain.Model.Log { Id = m.Id, Name = m.Name, Object = m.Object, ObjectType = m.ObjectType, ActionTime = m.ActionTime, Type = m.Type,  AcctionUser = m.AcctionUser, ObjectId = m.ObjectId });
            // returnurl
            data.ReturnUrl = Url.Action("Index", new { page, limit, key, type, pageManager, startTime, endTime, ordertype, orderby });
            return View("IndexAjax", data);
        }
        private Func<IQueryable<PT.Domain.Model.Log>, IOrderedQueryable<PT.Domain.Model.Log>> OrderByExtention(string ordertype, string orderby)
        {
            Func<IQueryable<PT.Domain.Model.Log>, IOrderedQueryable<PT.Domain.Model.Log>> functionOrder = null;
            switch (orderby)
            {
                case "createdDate":
                    functionOrder = ordertype == "asc" ? EntityExtention<PT.Domain.Model.Log>.OrderBy(m => m.OrderBy(x => x.ActionTime)) : EntityExtention<PT.Domain.Model.Log>.OrderBy(m => m.OrderByDescending(x => x.ActionTime));
                    break;
                default:
                    functionOrder = ordertype == "asc" ? EntityExtention<PT.Domain.Model.Log>.OrderBy(m => m.OrderBy(x => x.ActionTime)) : EntityExtention<PT.Domain.Model.Log>.OrderBy(m => m.OrderByDescending(x => x.ActionTime));
                    break;
            }
            return functionOrder;
        }
        #endregion
       
    }
}