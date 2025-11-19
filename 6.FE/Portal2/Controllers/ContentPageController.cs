using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PT.UI.Controllers
{
    public class ContentPageController : Controller
    {
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly IContentPageTagRepository _iContentPageTagRepository;
        private readonly ICategoryRepository _iCategoryRepository;

        public ContentPageController(IContentPageRepository iContentPageRepository,
            IContentPageTagRepository iContentPageTagRepository,
            ICategoryRepository iCategoryRepository
            )
        {
            _iContentPageRepository = iContentPageRepository;
            _iContentPageTagRepository = iContentPageTagRepository;
            _iCategoryRepository = iCategoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, string language, string linkData, int portalId)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            string viewName = "Details";
            var dl = await _iContentPageRepository.SingleOrDefaultAsync(true, x => x.Id == id && x.Status && x.PortalId == portalId);
            if( dl == null)
            {
                return View("_Home404");
            }
            dl.Tags = await _iContentPageTagRepository.GetTag(0, 0, id, x => x.Status);
            switch(dl.CategoryType)
            {
                case ECategoryType.ContentPage_FlowItems:
                    viewName = "FlowItems";
                    break;
                case ECategoryType.ContentPage_Flow:
                    viewName = "Flow";
                    break;
                case ECategoryType.ContentPage_Solution:
                    viewName = "Solution";
                    break;
                case ECategoryType.ContentPage_Blog:
                    viewName = "Blog";
                    break;
                case ECategoryType.ContentPage_Event:
                    viewName = "Event";
                    break;
                case ECategoryType.ContentPage_Publications:
                    viewName = "Publications";
                    break;
            }
            return View(viewName, dl);
        }
    }
}