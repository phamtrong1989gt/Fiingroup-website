using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;

namespace PT.BE.Areas.Base.Controllers
{
    [Area("Base")]
    public class FunctionsController : Controller
    {
        private readonly ILinkRepository _iLinkRepository;
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly IContactRepository _iContactRepository;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ICategoryRepository _iCategoryRepository;

        public FunctionsController(
            IWebHostEnvironment iHostingEnvironment, 
            ILinkRepository iLinkRepository,
            IContactRepository iContactRepository,
            IContentPageRepository iContentPageRepository,
            ICategoryRepository iCategoryRepository
            )
        {
            _iHostingEnvironment = iHostingEnvironment;
            _iLinkRepository = iLinkRepository;
            _iContactRepository = iContactRepository;
            _iContentPageRepository = iContentPageRepository;
            _iCategoryRepository = iCategoryRepository;
        }
       
        [HttpGet, Authorize]
        public async Task<IActionResult> Download(string filePath)
        {
            if (filePath == null)
                return Content("filename not present");
            var path =  $"{_iHostingEnvironment.WebRootPath}{filePath}";
            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, Functions.GetContentType(path), Path.GetFileName(path));
        }
        [HttpGet, Authorize]
        public async Task<JsonResult> IsSlug(string slug, int id, string language, int portalId)
        {
            if(id > 0)
            {
                var ktLug = await _iLinkRepository.AnyAsync(x => x.Slug == slug && x.ObjectId != id && x.Language == language && x.PortalId == portalId);
                return Json(!ktLug);
            }
            else
            {
                var ktLug = await _iLinkRepository.AnyAsync(x => x.Slug == slug && x.Language== language && x.PortalId == portalId);
                return Json(!ktLug);
            }
        }
        [HttpGet, Authorize]
        public async Task<JsonResult> MessageCount()
        {
            return Json(new {
                Contact = await _iContactRepository.CountAsync(x=>!x.Delete && !x.Status),
                Car = 0
            });
        }
        [HttpPost, Authorize]
        public async Task<List<SelectListItem>> SearchLink(string q, string language, CategoryType type)
        {
            return (await _iLinkRepository.SearchAsync(true, 0, 20, x => x.Name.ToLower().Contains(q.ToLower()) && !x.Delete && x.Status && x.Type == type && x.Language == language, x => x.OrderBy(y => y.Name),
               x => new Link { Id = x.Id, Name = x.Name, Delete = x.Delete, Status = x.Status, Language = x.Language, Type = x.Type })).Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
        }
    }
}