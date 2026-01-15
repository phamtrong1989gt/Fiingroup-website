using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PT.Base;
using PT.Base.Services;
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
        private readonly INewsAPIService _iNewsAPIService;
        private readonly IOptions<BaseSettings> _baseSettings;
        public CategoryController(IContentPageRepository iContentPageRepository, ICategoryRepository iCategoryRepository, INewsAPIService iNewsAPIService, IOptions<BaseSettings> baseSettings)
        {
            _iContentPageRepository = iContentPageRepository;
            _iCategoryRepository = iCategoryRepository;
            _iNewsAPIService = iNewsAPIService;
            _baseSettings = baseSettings;
        }

        [HttpGet]
        [Route("{language}/Category/NewsAjax")]
        public async Task<ActionResult> NewsAjax([FromQuery] NewsQueryParameters prs)
        {
            prs.PageSize = 9;
            prs.Page = prs.Page <= 0 ? 1 : prs.Page;
            prs.StatusIds = "1";
            prs.CategoryIds = prs.CategoryIds ?? "0";
            var listNew = await _iNewsAPIService.GetNewsAsync(prs, prs.Language ?? "vi", _baseSettings.Value.PortalId);
            return View("NewsAjax", listNew);
        }

        [HttpGet]
        [Route("{language}/Category/EventAjax")]
        public async Task<ActionResult> EventAjax([FromQuery] NewsQueryParameters prs)
        {
            //await Task.Delay(1000);
            prs.PageSize = 9;
            prs.Page = prs.Page <= 0 ? 1 : prs.Page;
            prs.StatusIds = "1";
            prs.CategoryIds = prs.CategoryIds ?? "0";
            var listNew = await _iNewsAPIService.GetNewsAsync(prs, prs.Language ?? "vi", _baseSettings.Value.PortalId);
            var ids = listNew.Items.Select(x => x.Id).ToList();
            // Cau id t ừ bảng CMS lưu
            var pages = await _iContentPageRepository.SearchAsync(true, 0, 100, x => ids.Contains(x.NewsId ?? 0), null, x => new ContentPage
            {
                Id = x.Id,
                NewsId = x.NewsId,
                Status = x.Status,
                FilePath = x.FilePath,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                TimeFromTo = x.TimeFromTo,
                Address = x.Address,
            });

            foreach (var item in listNew.Items)
            {
                var dl = pages.FirstOrDefault(x => x.NewsId == item.Id);
                item.Address = dl?.Address;
                item.TimeFromTo = dl?.TimeFromTo;
                item.StartDate = dl?.StartDate;
                item.FilePath = dl?.FilePath;
            }
            return View("EventAjax", listNew);
        }

        [HttpGet]
        [Route("{language}/Category/PublicationsAjax")]
        public async Task<ActionResult> PublicationsAjax([FromQuery] NewsQueryParameters prs)
        {
            //await Task.Delay(1000);
            prs.PageSize = 9;
            prs.Page = prs.Page <= 0 ? 1 : prs.Page;
            prs.StatusIds = "1";
            prs.CategoryIds = prs.CategoryIds ?? "0";
            var listNew = await _iNewsAPIService.GetNewsAsync(prs, prs.Language ?? "vi", _baseSettings.Value.PortalId);
            return View("PublicationsAjax", listNew);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, string language, int? page, string key, string startDate, string endDate, string linkData)
        {
            var objectLink = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            ViewData["linkData"] = objectLink;
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
                if (string.IsNullOrEmpty(dl.ExCategoryIds))
                {
                    dl.DataAPI = new NewsListResponse() { Items = new List<NewsItem>() };
                }
                else
                {
                    var listNew = await _iNewsAPIService.GetNewsAsync(new NewsQueryParameters
                    {
                        Page = page ?? 1,
                        PageSize = 9,
                        CategoryIds = dl.ExCategoryIds,
                        StatusIds = "1"
                    }, language ?? "vi", _baseSettings.Value.PortalId);

                    dl.DataAPI = listNew ?? new NewsListResponse { Items = [] };
                    // lấy danh mục  
                    var eventCategory = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Id == dl.Id);
                    if (eventCategory != null)
                    {
                        var listEvent = await _iNewsAPIService.GetNewsAsync(new NewsQueryParameters
                        {
                            Page = page ?? 1,
                            PageSize = 1,
                            CategoryIds = eventCategory.ExCategoryIds,
                            StatusIds = "1"
                        }, language ?? "vi", _baseSettings.Value.PortalId);

                        dl.EventTop = (listEvent ?? new NewsListResponse { Items = [] });

                        var ids = dl.EventTop.Items.Select(x => x.Id).ToList();
                        // Cau id t ừ bảng CMS lưu
                        var pages = await _iContentPageRepository.SearchAsync(true, 0, 100, x => ids.Contains(x.NewsId ?? 0), null, x => new ContentPage
                        {
                            Id = x.Id,
                            NewsId = x.NewsId,
                            Status = x.Status,
                            FilePath = x.FilePath,
                            StartDate = x.StartDate,
                            EndDate = x.EndDate,
                            TimeFromTo = x.TimeFromTo,
                            Address = x.Address,
                        });

                        foreach (var item in dl.EventTop.Items)
                        {
                            var dlX = pages.FirstOrDefault(x => x.NewsId == item.Id);
                            item.Address = dlX?.Address;
                            item.TimeFromTo = dlX?.TimeFromTo;
                            item.StartDate = dlX?.StartDate;
                            item.FilePath = dlX?.FilePath;
                        }
                    }
                    ViewData["ExCategoryIds"] = dl.ExCategoryIds;
                }
                viewName = "News";
            }
            else if (dl.CategoryType == ECategoryType.ContentPage_Event)
            {
                Type = ECategoryType.ContentPage_Event;
                if (string.IsNullOrEmpty(dl.ExCategoryIds))
                {
                    dl.DataAPI = new NewsListResponse() { Items = new List<NewsItem>() };
                }
                else
                {
                    var listEvent = await _iNewsAPIService.GetNewsAsync(new NewsQueryParameters
                    {
                        Page = page ?? 1,
                        PageSize = 9,
                        CategoryIds = dl.ExCategoryIds,
                        StatusIds = "1"
                    }, language ?? "vi", _baseSettings.Value.PortalId);
                    dl.DataAPI = listEvent ?? new NewsListResponse { Items = [] };

                    var ids = dl.DataAPI.Items.Select(x => x.Id).ToList();
                    // Cau id t ừ bảng CMS lưu
                    var pages = await _iContentPageRepository.SearchAsync(true, 0, 100, x => ids.Contains(x.NewsId ?? 0), null, x => new ContentPage
                    {
                        Id = x.Id,
                        NewsId = x.NewsId,
                        Status = x.Status,
                        FilePath = x.FilePath,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        TimeFromTo = x.TimeFromTo,
                        Address = x.Address,
                    });
                    foreach (var item in dl.DataAPI.Items)
                    {
                        var dlX = pages.FirstOrDefault(x => x.NewsId == item.Id);
                        item.Address = dlX?.Address;
                        item.TimeFromTo = dlX?.TimeFromTo;
                        item.StartDate = dlX?.StartDate;
                        item.FilePath = dlX?.FilePath;
                    }
                    ViewData["ExCategoryIds"] = dl.ExCategoryIds;
                }
                viewName = "Event";
            }
            else if (dl.CategoryType == ECategoryType.ContentPage_Publications)
            {
                Type = ECategoryType.ContentPage_Publications;
                if (string.IsNullOrEmpty(dl.ExCategoryIds))
                {
                    dl.DataAPI = new NewsListResponse() { Items = new List<NewsItem>() };
                }
                else
                {
                    var listNew = await _iNewsAPIService.GetNewsAsync(new NewsQueryParameters
                    {
                        Page = page ?? 1,
                        PageSize = 9,
                        CategoryIds = dl.ExCategoryIds,
                        StatusIds = "1"
                    }, language ?? "vi", _baseSettings.Value.PortalId);
                    dl.DataAPI = listNew ?? new NewsListResponse { Items = [] };
                    ViewData["ExCategoryIds"] = dl.ExCategoryIds;
                }
                viewName = "Publications";
            }

            if (dl.CategoryType == ECategoryType.ContentPage_Publications)
            {
            }

            objectLink.Title = $"{objectLink.Title}{((page == null) ? "" : (language == "vi" ? $" - trang {page}" : $" - page {page}"))}";
            //ViewData["linkData"] = objectLink;
            return View(viewName, dl);
        }
      
        public IActionResult Methodology(string linkData, int portalId)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            return View("Methodology");
        }
       
    }
}