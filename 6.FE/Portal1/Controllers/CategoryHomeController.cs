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
    public class TourSearchModel
    {
        public int[] Categorys { get; set; }
        public string[] Days { get; set; }
        public int[] TourTypes { get; set; }
        public int Page { get; set; }
        public string Lang { get; set; }
        public TourStyle? TourStyle { get; set; }
    }

    public class CategoryHomeController : Controller
    {
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly ITourRepository _iTourRepository;
        private readonly IProductRepository _iProductRepository;

        public CategoryHomeController(IContentPageRepository iContentPageRepository, ICategoryRepository iCategoryRepository, ITourRepository iTourRepository, IProductRepository iProductRepository)
        {
            _iContentPageRepository = iContentPageRepository;
            _iCategoryRepository = iCategoryRepository;
            _iTourRepository = iTourRepository;
            _iProductRepository = iProductRepository;
        }

        [HttpPost, Route("Home/CategoryHome/TourSearch")]
        public async Task<IActionResult> TourSearch(TourSearchModel obj)
        {
            var data = new Category
            {
                Tours = await _iTourRepository.SearchPagedListAsync(
                     obj.Page,
                     6,
                     obj.Categorys,
                     obj.Days,
                     obj.TourTypes,
                     m => m.Language == obj.Lang && m.Status && (m.Style == obj.TourStyle || obj.TourStyle == null),
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
                     })
            };
            return View("TourSearchAjax", data);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, string language, int? page, string key, string linkData)
        {
            var objectLink = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            objectLink.Title = string.IsNullOrEmpty(objectLink.Title) ? objectLink.Name : objectLink.Title;

            string viewName = "_404";
            var dl = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Id == id && x.Status);
            if (dl == null)
            {
                return View("_Home404");
            }
           
            if(dl.Type == CategoryType.CategoryTour)
            {
                viewName = "Tours";
                dl.Tours = await _iTourRepository.SearchPagedListAsync(
                     page ?? 1,
                    6,
                     id,
                     m => m.Language == language && m.Status,
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
                     });

                objectLink.Title = $"{ objectLink.Title }{ ((page == null) ? "" : (language == "vi" ? $" - trang {page}" : $" - page {page}"))}";

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
            }
            else if (dl.Type == CategoryType.CategoryProduct)
            {
                viewName = "Products";

                dl.ChildrentCategorys = await _iCategoryRepository.SearchAsync(true, 0, 0, x => x.Status && x.Type == CategoryType.CategoryProduct);

                dl.Products = await _iProductRepository.SearchPagedListAsync(
                  page ?? 1,
                  12,
                  id,
                  m => (m.Name.Contains(key) || key == null || m.Content.Contains(key))
                      && (m.Language == language)
                      && m.Status
                      && !m.Delete, x => x.OrderBy(x => x.Name), x => new Product
                      {
                          Category = x.Category,
                          Id = x.Id,
                          Banner = x.Banner,
                          Delete = x.Delete,
                          Name = x.Name,
                          Language = x.Language,
                          Status = x.Status,
                          Type = x.Type,
                          Link = x.Link,
                          Specification = x.Specification
                 });

                objectLink.Title = $"{objectLink.Title}{((page == null) ? "" : (language == "vi" ? $" - trang {page}" : $" - page {page}"))}";

                ViewData["linkData"] = objectLink;

                int totalPage = (dl.Products.TotalRows % dl.Products.Limit > 0) ? (dl.Products.TotalRows / dl.Products.Limit + 1) : (dl.Products.TotalRows / dl.Products.Limit);
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

            }
            else if (dl.Type == CategoryType.CategoryBlog)
            {
                viewName = "Blogs";
                dl.PageBlog = await _iContentPageRepository.SearchPagedListAsync(
                  page ?? 1,
                  9,
                  id,
                  null,
                  m => (m.Name.Contains(key) || key == null || m.Content.Contains(key) || m.Summary.Contains(key))
                      && m.Type == CategoryType.Blog
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
                          Link = x.Link
                      });

                objectLink.Title = $"{ objectLink.Title }{ ((page == null) ? "" : (language == "vi" ? $" - trang {page}" : $" - page {page}"))}";
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

            }
            else if (dl.Type == CategoryType.CategoryService)
            {
                if (dl.ParentId == 0)
                {
                    ViewData["linkData"] = objectLink;
                    viewName = "Services";
                    dl.ChildrentCategorys = await _iCategoryRepository.SearchAsync(true, 0, 0, x => x.Status  && x.ParentId == id && x.Type == CategoryType.CategoryService);
                    foreach (var item in dl.ChildrentCategorys)
                    {
                        item.ContentPageCategory = await _iContentPageRepository.SearchAdvanceAsync(CategoryType.Service, 0, 0, item.Id, null, x => x.Status  && x.Type == CategoryType.Service, x => x.OrderByDescending(m => m.DatePosted), x => new ContentPage
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
                            IsHome = x.IsHome
                        });
                    }

                }
               
                else
                {
                    viewName = "ChildrentServices";
                    ViewData["linkData"] = objectLink;
                    dl.PageBlog = await _iContentPageRepository.SearchPagedListAsync(
                              page ?? 1,
                              99999,
                              id,
                              null,
                              m => (m.Name.Contains(key) || key == null || m.Content.Contains(key) || m.Summary.Contains(key))
                                  && m.Type == CategoryType.Service
                                  && (m.Language == language)
                                  && m.Status
                                  , x => x.OrderByDescending(m => m.DatePosted), x => new ContentPage
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
                                      IsHome = x.IsHome
                                  });
                }
            }    
            return View(viewName, dl);
        }
    }
}