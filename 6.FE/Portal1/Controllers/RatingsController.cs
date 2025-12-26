using Microsoft.AspNetCore.Mvc;
using PT.Base.Services;
using PT.Domain.Model;
using System.Linq;
using System.Threading.Tasks;

namespace PT.UI.Controllers
{
    public class RatingsController : Controller
    {
        private readonly INewsAPIService _newsAPIService;
        public RatingsController(INewsAPIService newsAPIService)
        {
            _newsAPIService = newsAPIService;
        }

        public async Task<IActionResult> Index(string linkData, int portalId, string language)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);

            // Load dropdown data
            language = language ?? "vi";

            // Get industries
            var industries = await _newsAPIService.GetReportIndustriesAsync(language);
            ViewBag.Industries = industries?.Data?.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = x.IndustryTypeId.ToString(),
                Text = x.IndustryTypeName
            }).ToList();

            // Get scores
            var scores = await _newsAPIService.GetReportScoresAsync();
            ViewBag.Scores = scores?.Data?.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = x.ScoreId.ToString(),
                Text = x.ScoreValue
            }).ToList();

            // Get outlooks
            var outlooks = await _newsAPIService.GetReportOutlooksAsync(language);
            ViewBag.Outlooks = outlooks?.Data?.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = x.ProspectsId,
                Text = x.Prospects
            }).ToList();
            return View("Index");
        }

        [Route("{language}/Ratings/IndexAjax")]
        public async Task<IActionResult> IndexAjax([FromQuery] RatingResultsQueryParameters parameters)
        {
            var data = await _newsAPIService.GetRatingResultsAsync(parameters);
            return View("IndexAjax", data);
        }
    }
}
