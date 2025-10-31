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
    public class TourTypeHomeController : Controller
    {
        
        private readonly ITourRepository _iTourRepository;
        private readonly ITourDayRepository _iTourDayRepository;
        private readonly ITourTypeRepository _iTourTypeRepository;
        public TourTypeHomeController(ITourRepository iTourRepository, ITourDayRepository iTourDayRepository, ITourTypeRepository iTourTypeRepository)
        {
            _iTourRepository = iTourRepository;
            _iTourDayRepository = iTourDayRepository;
            _iTourTypeRepository = iTourTypeRepository;
        }

        public async Task<IActionResult> Tours(int id, string language, int? page, string linkData)
        {
            var dlType = await _iTourTypeRepository.SingleOrDefaultAsync(true, x => x.Id == id && x.Status && !x.Delete);
            if (dlType == null)
            {
                return View("_Home404");
            }
            var objectLink = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            objectLink.Title = string.IsNullOrEmpty(objectLink.Title) ? objectLink.Name : objectLink.Title;
            var dl = new Category
            {
                Tours = await _iTourRepository.SearchPagedListAsync(
                     page ?? 1,
                     6,
                     null,
                     m => m.Language == language && m.Status && m.TourTypeId == id,
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
                         Address = x.Address
                     }),
                Name = dlType.Name,
                Id = dlType.Id,
                Banner = dlType.Banner
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

