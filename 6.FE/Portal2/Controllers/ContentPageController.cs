using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PT.Base.Services;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PT.UI.Controllers
{
    public class ContentPageController : Controller
    {
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly IContentPageTagRepository _iContentPageTagRepository;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly INewsAPIService _iNewsAPIService;
        private readonly ILinkRepository _iLinkRepository;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly Dictionary<string, string> customRoutes = new()
        {
                    { "tin-tuc-fg", "FGNews" },
                    { "news-fg", "FGNews" },
                    { "su-kien-fg", "FGEvent" },
                    { "event-fg", "FGEvent" },
                    { "ratings-fr", "FRatings" },
         };

        public ContentPageController(IContentPageRepository iContentPageRepository,
            IContentPageTagRepository iContentPageTagRepository,
            ICategoryRepository iCategoryRepository,
            INewsAPIService iNewsAPIService,
            ILinkRepository iLinkRepository,
            IOptions<BaseSettings> baseSettings
            )
        {
            _iContentPageRepository = iContentPageRepository;
            _iContentPageTagRepository = iContentPageTagRepository;
            _iCategoryRepository = iCategoryRepository;
            _iNewsAPIService = iNewsAPIService;
            _iLinkRepository = iLinkRepository;
            _baseSettings = baseSettings;
        }


        [HttpGet]
        [Route("{language}/ContentPage/TopNewsAjax")]
        public async Task<ActionResult> TopNewsAjax([FromQuery] NewsQueryParameters prs)
        {
            ViewData["language"] = prs.Language;
            var cmsCategory = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Language == prs.Language && x.PortalId == _baseSettings.Value.PortalId && x.CategoryType == ECategoryType.ContentPage_Blog && x.ParentId == 0 );
            if (cmsCategory != null && !string.IsNullOrEmpty(cmsCategory.ExCategoryIds))
            {
                prs.ExCategoryIds = cmsCategory.ExCategoryIds;
            }
            else
            {
                return View("TopNewsAjax", null);
            }
            prs.PageSize = 3;
            prs.Page = 1;
            prs.StatusIds = "1";
            prs.CategoryIds = prs.ExCategoryIds ?? "0";
            var listNew = await _iNewsAPIService.GetNewsAsync(prs, prs.Language ?? "vi", _baseSettings.Value.PortalId);
            return View("TopNewsAjax", listNew);
        }

        [HttpGet]
        public async Task<IActionResult> FGNews(int id, string language, string linkData, int portalId)
        {
           

            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            var dl = await _iNewsAPIService.GetNewsByIdAsync(id, language, portalId);
            if(dl == null || dl.Success == false)
            {
                return View("_Home404");
            }
            string viewName = "FGNews";
            ViewBag.newId = id;
            ViewBag.action = language == "en" ? "tin-tuc-fg": "news-fg";
            return View(viewName, dl.Data);
        }

        [HttpGet]
        public async Task<IActionResult> FGEvent(int id, string language, string linkData, int portalId)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            var dl = await _iNewsAPIService.GetNewsByIdAsync(id, language, portalId);
            if (dl.Success == false)
            {
                return View("_Home404");
            }
            string viewName = "FGEvent";
            ViewBag.newId = id;
            ViewBag.action = language == "en" ? "su-kien-fg" : "event-fg";
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
            //dl.Tags = await _iContentPageTagRepository.GetTag(0, 0, id, x => x.Status);
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
                case ECategoryType.ContentPage_Page:
                    viewName = "Page";
                    break;
            }
            return View(viewName, dl);
        }
    }
}