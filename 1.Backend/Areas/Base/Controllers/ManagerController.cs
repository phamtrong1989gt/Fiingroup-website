using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PT.Base;
using PT.Shared;

namespace PT.UI.Areas.Base.Controllers
{
    [Area("Base")]
    public class ManagerController : BaseController
    {
        [AuthorizePermission]
        [Route("Admin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}