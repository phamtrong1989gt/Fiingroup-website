using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PT.Base.Services;
using PT.Domain.Model;
using System.Linq;
using System.Threading.Tasks;

namespace PT.UI.Controllers
{
    public class Ratings2Controller : Controller
    {
        private readonly INewsAPIService _newsAPIService;
        private readonly IOptions<BaseSettings> _baseSettings;
        public Ratings2Controller(INewsAPIService newsAPIService, IOptions<BaseSettings> baseSettings)
        {
            _newsAPIService = newsAPIService;
            _baseSettings = baseSettings;
        }

        public async Task<IActionResult> Index(string linkData, int portalId, string language)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);

            // Load dropdown data
            language = language ?? "vi";

            // Get sustainable industries
            var industries = await _newsAPIService.GetSustainableIndustriesAsync(language, portalId);
            ViewBag.Industries = industries?.Data?.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = x.IndustryId.ToString(),
                Text = x.IndustryName
            }).ToList();
           
            // Get sustainable standards (applicable standards)
            var standards = await _newsAPIService.GetSustainableStandardsAsync(language, portalId);
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
            var data = await _newsAPIService.GetSustainableFinanceReportsAsync(parameters, language, _baseSettings.Value.PortalId);
            return View("IndexAjax", data);
        }
    }
}
