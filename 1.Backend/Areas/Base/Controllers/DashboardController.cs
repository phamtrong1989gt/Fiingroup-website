using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PT.Base;
using PT.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PT.BE.Areas.Base.Controllers
{
    [Area("Base")]
    //[Authorize]
    public class DashboardController : Controller
    {
        [AuthorizePermission]
        [Route("Admin")]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("Admin/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
        
    }
}