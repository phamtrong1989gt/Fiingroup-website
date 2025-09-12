using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;

namespace PT.UI.Controllers
{
    public class EmployeeHomeController : Controller
    {
        private readonly IEmployeeRepository _iEmployeeRepository;
        public EmployeeHomeController(IEmployeeRepository iEmployeeRepository)
        {
            _iEmployeeRepository = iEmployeeRepository;
        }
 
        public async Task<IActionResult> Employees(string language, string linkData)
        {
            

            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            var list = await _iEmployeeRepository.SearchAsync(true, 0, 0, x => x.Status && !x.Delete && x.Language == language);
            return View(list);
        }

        //[HttpGet]
        //public async Task<IActionResult> Details(int id, string language, string linkData)
        //{
        //    

        //    ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);

        //    var dl = await _iEmployeeRepository.SingleOrDefaultAsync(true, x => x.Id == id && x.Status && !x.Delete);
        //    if (dl == null)
        //    {
        //        return View("_404");
        //    }

        //    var list = await _iEmployeeRepository.SearchAsync(true, 0, 0, x => x.Status && !x.Delete && x.Language == language && x.Id!=id, null, x => new Employee
        //    {
        //        Id = x.Id,
        //        Link = x.Link,
        //        Banner = x.Banner,
        //        Degrees = x.Degrees,
        //        Status = x.Status,
        //        Delete = x.Delete,
        //        Language = x.Language,
        //        Email = x.Email,
        //        Phone = x.Phone,
        //        Summary = x.Summary,
        //        EmployeeMappingId = x.EmployeeMappingId,
        //        FullName = x.FullName,
        //        Gender = x.Gender,
        //        Facebook = x.Facebook,
        //        Job = x.Job,
        //        Office = x.Office,
        //        Endodontics = x.Endodontics,
        //        OralMedicine = x.OralMedicine,
        //        GeneralDentistry = x.GeneralDentistry,
        //        OralSurgery = x.OralSurgery,
        //        Orthodontics = x.Orthodontics,
        //        Prosthodontics = x.Prosthodontics,
        //        Periodontics = x.Periodontics
        //    });

        //    ViewData["Title"] = "";
        //    ViewData["Description"] = "";
        //    ViewData["Keywords"] = "";
        //    ViewData["Img"] = dl.Banner;
        //    dl.Employees = list;
        //    return View(dl);
        //}

        //[HttpGet]
        //public IActionResult Schedule(string language, string linkData)
        //{
        //    

        //    ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
        //    return View();
        //}
    }
}
