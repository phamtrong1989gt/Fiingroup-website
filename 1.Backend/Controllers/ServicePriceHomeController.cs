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
    public class ServicePriceHomeController : Controller
    {
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ITagRepository _iTagRepository;
        private readonly IContentPageTagRepository _iContentPageTagRepository;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly IServicePriceRepository _iServicePriceRepository;

        public ServicePriceHomeController(IContentPageRepository iContentPageRepository,
            ITagRepository iTagRepository, 
            IContentPageTagRepository iContentPageTagRepository,
            ICategoryRepository iCategoryRepository,
            IServicePriceRepository iServicePriceRepository
            )
        {
            _iContentPageRepository = iContentPageRepository;
            _iTagRepository = iTagRepository;
            _iContentPageTagRepository = iContentPageTagRepository;
            _iCategoryRepository = iCategoryRepository;
            _iServicePriceRepository = iServicePriceRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Details(string language, string linkData)
        {
            

            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            var listPrice = await _iServicePriceRepository.SearchAsync(true, 0, 0, x => x.Status && !x.Delete && x.Language == language);
            return View(listPrice);
        }
    }
}