using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PT.Base.Services;
using PT.Domain.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PT.UI.Controllers
{
    public class RatingsController : Controller
    {
        private readonly INewsAPIService _newsAPIService;
        private readonly IOptions<BaseSettings> _baseSettings;
        public RatingsController(INewsAPIService newsAPIService, IOptions<BaseSettings> baseSettings)
        {
            _newsAPIService = newsAPIService;
            _baseSettings = baseSettings;
        }

        public async Task<IActionResult> Index(string linkData, int portalId, string language)
        {
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);

            // Load dropdown data
            language = language ?? "vi";

            // Get industries
            var industries = await _newsAPIService.GetReportIndustriesAsync(language, _baseSettings.Value.PortalId);
            ViewBag.Industries = industries?.Data?.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = x.IndustryTypeId.ToString(),
                Text = x.IndustryTypeName
            }).ToList();

            // Get scores
            var scores = await _newsAPIService.GetReportScoresAsync(_baseSettings.Value.PortalId);
            ViewBag.Scores = scores?.Data?.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = x.ScoreId.ToString(),
                Text = x.ScoreValue
            }).ToList();

            // Get outlooks
            var outlooks = await _newsAPIService.GetReportOutlooksAsync(language, _baseSettings.Value.PortalId);
            ViewBag.Outlooks = outlooks?.Data?.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = x.ProspectsId,
                Text = x.Prospects
            }).ToList();

            // Get opinion types
            var opinionTypes = await _newsAPIService.GetOpinionTypesAsync(language, _baseSettings.Value.PortalId);
            ViewBag.OpinionTypes = opinionTypes?.data?.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = x.opinionTypeId.ToString(),
                Text = x.opinionTypeName
            }).ToList();

            var issuerTypes = await _newsAPIService.GetIssuerTypesAsync(language, _baseSettings.Value.PortalId);
            ViewBag.IssuerTypes = issuerTypes?.data?.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = x.issuerTypeId.ToString(),
                Text = x.issuerTypeName
            }).ToList();

            return View("Index");
        }

        [Route("{language}/Ratings/IndexAjax")]
        public async Task<IActionResult> IndexAjax([FromQuery] RatingResultsQueryParameters parameters)
        {
            parameters.FromDate = "2020-01-01";
            parameters.ToDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            var data = await _newsAPIService.GetRatingResultsAsync(parameters, _baseSettings.Value.PortalId);
            ViewBag.Language = parameters.Lang;
            return View("IndexAjax", data);
        }

        [Route("{language}/Ratings/RatingsAjax")]
        public async Task<IActionResult> RatingsAjax([FromQuery] RatingResultsQueryParameters parameters)
        {
            parameters.FromDate = "2020-01-01";
            parameters.ToDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            var data = await _newsAPIService.GetRatingResultsAsync(parameters, _baseSettings.Value.PortalId);
            ViewBag.Language = parameters.Lang;
            return View("RatingsAjax", data);
        }

        [Route("{language}/Ratings/RatingsDebtAjax")]
        public async Task<IActionResult> RatingsDebtAjax([FromQuery] RatingResultsQueryParameters parameters)
        {
            parameters.FromDate = "2020-01-01";
            parameters.ToDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            parameters.IndustryTypeId = 2; // Set a default industry type ID
            var data = await _newsAPIService.GetRatingResultsAsync(parameters, _baseSettings.Value.PortalId);
            ViewBag.Language = parameters.Lang;
            return View("RatingsDebtAjax", data);
        }
    }
}
