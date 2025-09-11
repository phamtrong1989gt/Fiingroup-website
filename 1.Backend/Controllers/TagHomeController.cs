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
    public class TagHomeController : Controller
    {
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ITagRepository _iTagRepository;
        private readonly IContentPageTagRepository _iContentPageTagRepository;

        public TagHomeController(IContentPageRepository iContentPageRepository, IContentPageTagRepository iContentPageTagRepository, ITagRepository iTagRepository)
        {
            _iContentPageRepository = iContentPageRepository;
            _iContentPageTagRepository = iContentPageTagRepository;
            _iTagRepository = iTagRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, string language, int? page, string linkData)
        {
            
            var objectLink = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            objectLink.Title = string.IsNullOrEmpty(objectLink.Title) ? objectLink.Name : objectLink.Title;
            objectLink.Title = $"{ objectLink.Title }{ ((page == null) ? "" : (language == "vi" ? $" - trang {page}" : $" - page {page}"))}";
            ViewData["linkData"] = objectLink;

            var dl = await _iTagRepository.SingleOrDefaultAsync(true, x => x.Id == id && x.Status && !x.Delete);
            if (dl == null)
            {
                return View("_Home404");
            }

            var data = await _iContentPageRepository.SearchPagedListAsync(
                   page ?? 1,
                   10,
                   null,
                   id,
                   m => m.Status && !m.Delete,
                   x => x.OrderByDescending(mbox => mbox.DatePosted), x => new ContentPage
                   {
                       Category = x.Category,
                       Id = x.Id,
                       Author = x.Author,
                       Banner = x.Banner,
                       DatePosted = x.DatePosted,
                       Delete = x.Delete,
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
                       IsHome = x.IsHome
                   });
            dl.ContentPages = data;

            int totalPage = (data.TotalRows % data.Limit > 0) ? (data.TotalRows / data.Limit + 1) : (data.TotalRows / data.Limit);
            if (totalPage >= 2)
            {
                page = page ?? 1;
                if (page < totalPage)
                {
                    ViewData["linkNext"] = $"{Request.Path}?page={page + 1}";
                }
                if (page >= totalPage)
                {
                    ViewData["linkPrev"] = $"{Request.Path}?page={page - 1}";
                }
            }

            return View(dl);
        }
    }
}