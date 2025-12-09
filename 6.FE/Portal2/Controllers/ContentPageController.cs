using Microsoft.AspNetCore.Mvc;
using PT.Base.Services;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Threading.Tasks;

namespace PT.UI.Controllers
{
    public class ContentPageController : Controller
    {
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly IContentPageTagRepository _iContentPageTagRepository;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly INewsAPIService _iNewsAPIService;
        private readonly ILinkRepository _iLinkRepository;
        public ContentPageController(IContentPageRepository iContentPageRepository,
            IContentPageTagRepository iContentPageTagRepository,
            ICategoryRepository iCategoryRepository,
            INewsAPIService iNewsAPIService,
            ILinkRepository iLinkRepository
            )
        {
            _iContentPageRepository = iContentPageRepository;
            _iContentPageTagRepository = iContentPageTagRepository;
            _iCategoryRepository = iCategoryRepository;
            _iNewsAPIService = iNewsAPIService;
            _iLinkRepository = iLinkRepository;
        }

        [HttpGet]
        public async Task<ActionResult> TopNewsAjax([FromQuery] NewsQueryParameters prs)
        {
            var cmsCategory = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Id == prs.CategoryId);
            if (cmsCategory != null && !string.IsNullOrEmpty(cmsCategory.ExCategoryIds))
            {
                prs.ExCategoryIds = cmsCategory.ExCategoryIds;
            }
            prs.PageSize = 3;
            prs.Page = prs.Page <= 0 ? 1 : prs.Page;
            prs.FromDate = prs.FromDate ?? Convert.ToDateTime($"{DateTime.Now.Year}/01/01").ToString("yyyy-MM-dd");
            prs.Status = "Active";
            prs.CategoryIds = prs.CategoryIds ?? "0";
            var listNew = await _iNewsAPIService.GetNewsAsync(prs, prs.Language ?? "vi");
            return View("TopNewsAjax", listNew);
        }

        [HttpGet]
        public async Task<IActionResult> FGNews(int id, string language, string linkData, int portalId)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            var dl = await _iNewsAPIService.GetNewsByIdAsync(id);
            if(dl.Success == false)
            {
                return View("_Home404");
            }
            string viewName = "FGNews";
            return View(viewName, dl.Data);
        }

        [HttpGet]
        public async Task<IActionResult> FGEvent(int id, string language, string linkData, int portalId)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            var dl = await _iNewsAPIService.GetNewsByIdAsync(id);
            if (dl.Success == false)
            {
                return View("_Home404");
            }
            string viewName = "FGEvent";
            return View(viewName, dl.Data);
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