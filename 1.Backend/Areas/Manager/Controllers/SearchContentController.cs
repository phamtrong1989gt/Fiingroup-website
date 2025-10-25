using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System.Linq;
using PT.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using PT.Base;

namespace PT.BE.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class SearchContentController : Base.Controllers.BaseController
    {
        private readonly ILogger _logger;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ILinkRepository _iLinkRepository;
        private readonly ITagRepository _iTagRepository;
        private readonly IContentPageTagRepository _iContentPageTagRepository;

        public SearchContentController(
            ILogger<SearchContentController> logger,
            IOptions<BaseSettings> baseSettings,
            IContentPageRepository iContentPageRepository,
            ILinkRepository iLinkRepository,
            ITagRepository iTagRepository,
            IContentPageTagRepository iContentPageTagRepository
        )
        {
            controllerName = "SearchContent";
            tableName = "Blog";
            _logger = logger;
            _baseSettings = baseSettings;
            _iContentPageRepository = iContentPageRepository;
            _iLinkRepository = iLinkRepository;
            _iTagRepository = iTagRepository;
            _iContentPageTagRepository = iContentPageTagRepository;
        }

        #region [Index]
        [AuthorizePermission]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, string language ="vi", string ordertype = "asc", string orderby = "name")
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iContentPageRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                    m => m.Content.Contains(key),
                OrderByExtention(ordertype, orderby), 
                x=> new ContentPage {
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
                data.ReturnUrl =  Url.Action("Index", 
                    new {
                        page,
                        limit,
                        key,
                        ordertype,
                        orderby
            });
            return View("IndexAjax", data);
        }
        private Func<IQueryable<ContentPage>, IOrderedQueryable<ContentPage>> OrderByExtention(string ordertype, string orderby)
        {
            Func<IQueryable<ContentPage>, IOrderedQueryable<ContentPage>> functionOrder = null;
            switch (orderby)
            {
                case "name":
                    functionOrder = ordertype == "asc" ? EntityExtention<ContentPage>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<ContentPage>.OrderBy(m => m.OrderByDescending(x => x.Name));
                    break;
                default:
                    functionOrder = ordertype == "asc" ? EntityExtention<ContentPage>.OrderBy(m => m.OrderBy(x => x.DatePosted)) : EntityExtention<ContentPage>.OrderBy(m => m.OrderByDescending(x => x.DatePosted));
                    break;
            }
            return functionOrder;
        }
        #endregion
    }
}