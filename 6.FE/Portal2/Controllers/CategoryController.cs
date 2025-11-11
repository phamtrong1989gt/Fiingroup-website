using Microsoft.AspNetCore.Mvc;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace PT.UI.Controllers
{
    public class TourSearchModel
    {
        public int[] Categorys { get; set; }
        public string[] Days { get; set; }
        public int[] TourTypes { get; set; }
        public int Page { get; set; }
        public string Lang { get; set; }
        public TourStyle? TourStyle { get; set; }
    }

    public class CategoryController : Controller
    {
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly ITourRepository _iTourRepository;
        public CategoryController(IContentPageRepository iContentPageRepository, ICategoryRepository iCategoryRepository, ITourRepository iTourRepository)
        {
            _iContentPageRepository = iContentPageRepository;
            _iCategoryRepository = iCategoryRepository;
            _iTourRepository = iTourRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, string language, int? page, string key, string startDate, string endDate, string linkData)
        {
            var objectLink = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            objectLink.Title = string.IsNullOrEmpty(objectLink.Title) ? objectLink.Name : objectLink.Title;

            string viewName = "_404";
            var Type = ECategoryType.ContentPage_Blog;
            var dl = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Id == id && x.Status);
            if (dl == null)
            {
                return View("_Home404");
            }
            else if (dl.CategoryType == ECategoryType.ContentPage_Blog)
            {
                Type = ECategoryType.ContentPage_Blog;
                viewName = "News";
            }
            else if (dl.CategoryType == ECategoryType.ContentPage_Event)
            {
                Type = ECategoryType.ContentPage_Event;
                viewName = "Event";
            }

            if (dl.CategoryType == ECategoryType.ContentPage_Event)
            {
                dl.PageBlog = await _iContentPageRepository.SearchPagedListAsync(
                    page ?? 1,
                    5,
                    id,
                    null,
                    m => (m.Name.Contains(key) || key == null || m.Content.Contains(key) || m.Summary.Contains(key))
                        && m.CategoryType == Type
                        && (m.Language == language)
                        && m.Status
                        , x => x.OrderByDescending(mbox => mbox.StartDate), x => new ContentPage
                        {
                            Category = x.Category,
                            Id = x.Id,
                            Author = x.Author,
                            Banner = x.Banner,
                            DatePosted = x.DatePosted,
                            Name = x.Name,
                            Language = x.Language,
                            Status = x.Status,
                            Summary = x.Summary,
                            Tags = x.Tags,
                            Type = x.Type,
                            Link = x.Link,
                            StartDate = x.StartDate,
                            TimeFromTo = x.TimeFromTo,
                            Address = x.Address,
                            Input1 = x.Input1
                        });
            }
            else
            {


                DateTime? start = null, end = null;
                if (!string.IsNullOrWhiteSpace(startDate)
                    && DateTime.TryParseExact(startDate, "dd/MM/yyyy",
                        CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.None, out var d))
                {
                    start = d.Date;
                    end = d.Date.AddDays(1);
                }
                if (!string.IsNullOrWhiteSpace(endDate)
                    && DateTime.TryParseExact(endDate, "dd/MM/yyyy",
                        CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.None, out var dE))
                {
                    end = dE.Date;
                }
                dl.PageBlog = await _iContentPageRepository.SearchPagedListAsync(
                     page ?? 1,
                     9,
                     id,
                     null,
                     m => (m.Name.Contains(key) || key == null || m.Content.Contains(key) || m.Summary.Contains(key))
                         && (!start.HasValue || m.DatePosted >= start.Value)
                         && (!end.HasValue || m.DatePosted <= end.Value)
                         && m.CategoryType == Type
                         && (m.Language == language)
                         && m.Status
                         , x => x.OrderByDescending(mbox => mbox.DatePosted), x => new ContentPage
                         {
                             Category = x.Category,
                             Id = x.Id,
                             Author = x.Author,
                             Banner = x.Banner,
                             DatePosted = x.DatePosted,
                             Name = x.Name,
                             Language = x.Language,
                             Status = x.Status,
                             Summary = x.Summary,
                             Tags = x.Tags,
                             Type = x.Type,
                             Link = x.Link,
                             Input1 = x.Input1
                         });
            }

            objectLink.Title = $"{objectLink.Title}{((page == null) ? "" : (language == "vi" ? $" - trang {page}" : $" - page {page}"))}";
            ViewData["linkData"] = objectLink;
            int totalPage = (dl.PageBlog.TotalRows % dl.PageBlog.Limit > 0) ? (dl.PageBlog.TotalRows / dl.PageBlog.Limit + 1) : (dl.PageBlog.TotalRows / dl.PageBlog.Limit);
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

            return View(viewName, dl);
        }
    }
}