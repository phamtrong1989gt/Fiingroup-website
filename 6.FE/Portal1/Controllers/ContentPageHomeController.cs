using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;

namespace PT.UI.Controllers
{
    public class ContentPageHomeController : Controller
    {
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly IContentPageTagRepository _iContentPageTagRepository;
        private readonly ICategoryRepository _iCategoryRepository;

        public ContentPageHomeController(IContentPageRepository iContentPageRepository,
            IContentPageTagRepository iContentPageTagRepository,
            ICategoryRepository iCategoryRepository
            )
        {
            _iContentPageRepository = iContentPageRepository;
            _iContentPageTagRepository = iContentPageTagRepository;
            _iCategoryRepository = iCategoryRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id, string language, string linkData)
        {
            
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);

            string viewName = "Details";

            var dl = await _iContentPageRepository.SingleOrDefaultAsync(true, x => x.Id == id && x.Status);
            if( dl == null)
            {
                return View("_Home404");
            }
            dl.Tags = await _iContentPageTagRepository.GetTag(0, 0, id, x => x.Status);
            switch(dl.CategoryType)
            {
                case ECategoryType.ContentPage_Blog:
                    viewName = "Blog";
                    break;
                case ECategoryType.ContentPage_Page:
                    viewName = "Page";
                    break;
            }

            return View(viewName, dl);
        }
        public async Task<IActionResult> FAQ(string language, int? c, int? page,string k, string linkData)
        {
            var objectLink = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            objectLink.Title = string.IsNullOrEmpty(objectLink.Title) ? objectLink.Name : objectLink.Title;
            objectLink.Title = $"{ objectLink.Title }{ ((page == null) ? "" : (language == "vi" ? $" - trang {page}" : $" - page {page}"))}";
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);

            page = page < 0 ? 1 : page;
            int limit = 10;
            var data = await _iContentPageRepository.FAQSearchPagedListAsync(
                page ?? 1,
                limit,
                c,
                null,
                m => (m.Name.Contains(k) || k == null || m.Content.Contains(k) || m.Summary.Contains(k)) 
                && m.CategoryType == ECategoryType.ContentPage_FAQ
                && (m.Language == language) && m.Status, x=>x.OrderByDescending(m=>m.DatePosted), x => new ContentPage
                {
                    Category = x.Category,
                    Id = x.Id,
                    Author = x.Author,
                    Banner = x.Banner,
                    DatePosted = x.DatePosted,
                    Name = x.Name,
                    Language = x.Language,
                    Price = x.Price,
                    Serice = x.Serice,
                    ServiceId = x.ServiceId,
                    Status = x.Status,
                    Summary = x.Summary,
                    Tags = x.Tags,
                    Type = x.Type,
                    Link = x.Link,
                    IsHome = x.IsHome,
                    CategoryType = x.CategoryType,
                    SlugType = x.SlugType
                });

            int totalPage = (data.TotalRows % data.Limit > 0) ? (data.TotalRows / data.Limit + 1) : (data.TotalRows / data.Limit);
            if (totalPage >= 2)
            {
                page = page ?? 1;
                if (page < totalPage)
                {
                    ViewData["linkNext"] = $"{Request.Path}?page={page + 1}&c={c}&k={k}";
                }
                if (page >= totalPage)
                {
                    ViewData["linkPrev"] = $"{Request.Path}?page={page - 1}&c={c}&k={k}";
                }
            }

            return View(data);
        }
    }
}