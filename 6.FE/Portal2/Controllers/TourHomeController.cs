using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;

namespace PT.UI.Controllers
{
    public class TourHomeController : Controller
    {
        
        private readonly ITourRepository _iTourRepository;
        private readonly ITourDayRepository _iTourDayRepository;

        public TourHomeController(ITourRepository iTourRepository, ITourDayRepository iTourDayRepository)
        {
            _iTourRepository = iTourRepository;
            _iTourDayRepository = iTourDayRepository;
        }

        public async Task<IActionResult> Details(int id, string language, string linkData)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            var data = await _iTourRepository.SingleOrDefaultAsync(true, x => x.Id == id);
            if (data == null)
            {
                return View("_Home404");
            }
            data.TourDays = await _iTourDayRepository.SearchAsync(true, 0, 100, x => x.TourId == data.Id);
            return View($"Details", data);
        }

        public async Task<IActionResult> StyleTours(TourStyle id, string language, int? page, string linkData)
        {
            var objectLink = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            objectLink.Title = string.IsNullOrEmpty(objectLink.Title) ? objectLink.Name : objectLink.Title;
            var dl = new Category
            {
                Tours = await _iTourRepository.SearchPagedListAsync(
                     page ?? 1,
                     6,
                     null,
                     m => 
                         m.Language == language 
                         && m.Status
                         && m.Style == id,
                     x => x.OrderByDescending(mbox => mbox.CreatedDate),
                     x => new Tour
                     {
                         Id = x.Id,
                         Status = x.Status,
                         Style = x.Style,
                         CreatedDate = x.CreatedDate,
                         Name = x.Name,
                         Trips = x.Trips,
                         Language = x.Language,
                         Link = x.Link,
                         Banner = x.Banner,
                         Address = x.Address,
                     }),
                Name = objectLink.Name,
                Id = (int)id
            };

            objectLink.Title = $"{ objectLink.Title }{((page == null) ? "" : (language == "vi" ? $" - trang {page}" : $" - page {page}"))}";

            ViewData["linkData"] = objectLink;

            int totalPage = (dl.Tours.TotalRows % dl.Tours.Limit > 0) ? (dl.Tours.TotalRows / dl.Tours.Limit + 1) : (dl.Tours.TotalRows / dl.Tours.Limit);
            if (totalPage >= 2)
            {
                page ??= 1;
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

