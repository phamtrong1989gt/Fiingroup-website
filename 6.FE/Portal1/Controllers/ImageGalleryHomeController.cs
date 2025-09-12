using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;

namespace PT.UI.Controllers
{
    public class ImageGalleryHomeController : Controller
    {
        private readonly IImageGalleryRepository _iImageGalleryRepository;
        private readonly IImageRepository _iImageRepository;
        private readonly ICategoryRepository _iCategoryRepository;

        public ImageGalleryHomeController(IImageGalleryRepository iImageGalleryRepository, IImageRepository iImageRepository, ICategoryRepository iCategoryRepository)
        {
            _iImageGalleryRepository = iImageGalleryRepository;
            _iImageRepository = iImageRepository;
            _iCategoryRepository = iCategoryRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id, string language,int? page, string linkData, int? c)
        {
            

            var objectLink = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            objectLink.Title = string.IsNullOrEmpty(objectLink.Title) ? objectLink.Name : objectLink.Title;
            objectLink.Title = $"{ objectLink.Title }{ ((page == null) ? "" : (language == "vi" ? $" - trang {page}" : $" - page {page}"))}";
            ViewData["linkData"] = objectLink;

            var dl = await _iImageGalleryRepository.SingleOrDefaultAsync(true, x => x.Id == id && x.Status && !x.Delete);
            if (dl == null)
            {
                return View("_Home404");
            }
            ViewData["Title"] = dl.Title;
            ViewData["Description"] = dl.Description;
            ViewData["Keywords"] = dl.Keywords;
            dl.ListImage = await _iImageRepository.SearchPagedListAsync(page??1, 20, x => x.ImageGalleryId == id && (x.CategoryId== c || c==null),x=>x.OrderByDescending(m=>m.Id));
            dl.Categorys = await _iCategoryRepository.SearchAsync(true, 0, 0, x => x.Status && !x.Delete && x.Language == language && x.Type == CategoryType.CategoryService, x => x.OrderBy(m => m.Order));

            int totalPage = (dl.ListImage.TotalRows % dl.ListImage.Limit > 0) ? (dl.ListImage.TotalRows / dl.ListImage.Limit + 1) : (dl.ListImage.TotalRows / dl.ListImage.Limit);
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

