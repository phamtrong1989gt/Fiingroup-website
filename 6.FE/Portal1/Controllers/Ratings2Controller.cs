using Microsoft.AspNetCore.Mvc;
using PT.Base.Services;
using PT.Domain.Model;
using System.Linq;
using System.Threading.Tasks;

namespace PT.UI.Controllers
{
    public class Ratings2Controller : Controller
    {
        private readonly INewsAPIService _newsAPIService;
        public Ratings2Controller(INewsAPIService newsAPIService)
        {
            _newsAPIService = newsAPIService;
        }

        public async Task<IActionResult> Index(string linkData, int portalId, string language)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);

            // Load dropdown data
            language = language ?? "vi";

            // Get sustainable industries
            var industries = await _newsAPIService.GetSustainableIndustriesAsync(language);
            ViewBag.Industries = industries?.Data?.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = x.IndustryId.ToString(),
                Text = x.IndustryName
            }).ToList();
           
            // Get sustainable standards (applicable standards)
            var standards = await _newsAPIService.GetSustainableStandardsAsync(language);
            ViewBag.SustainableLevels = standards?.Data?.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = x.ApplicableStandardsId.ToString(),
                Text = x.ApplicableStandardsName
            }).ToList();
            
            return View("Index");
        }

        [Route("{language}/Ratings2/IndexAjax")]
        public async Task<IActionResult> IndexAjax([FromQuery] SustainableFinanceQueryParameters parameters, string language)
        {
            ViewBag.Language = language;
            var data = await _newsAPIService.GetSustainableFinanceReportsAsync(parameters, language);
            return View("IndexAjax", data);
        }
    }
}
