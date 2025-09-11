using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PT.Shared;

namespace PT.UI.Areas.Base.Controllers
{
    [Area("Base")]
    //[Authorize]
    public class DashboardController : Controller
    {
        [Route("Admin/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
        
    }
}