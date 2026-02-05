using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PT.Base.Services;
using PT.Domain.Model;
using System;
using System.Collections.Generic;
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
            parameters.FromDate = "2000-01-01";
            parameters.ToDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            var data = await _newsAPIService.GetRatingResultsAsync(parameters, _baseSettings.Value.PortalId);
            ViewBag.Language = parameters.Lang;
            return View("IndexAjax", data);
        }

        [Route("{language}/Ratings/RatingsAjax")]
        public async Task<IActionResult> RatingsAjax([FromQuery] RatingResultsQueryParameters parameters)
        {
            parameters.FromDate = "2000-01-01";
            parameters.ToDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            var data = await _newsAPIService.GetRatingResultsAsync(parameters, _baseSettings.Value.PortalId);
            ViewBag.Language = parameters.Lang;
            return View("RatingsAjax", data);
        }

        [Route("{language}/Ratings/SearchAjax")]
        public async Task<IActionResult> SearchAjax([FromQuery] RatingResultsQueryParameters parameters)
        {
            parameters.FromDate = "2000-01-01";
            parameters.ToDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            var data = await _newsAPIService.GetRatingResultsAsync(parameters, _baseSettings.Value.PortalId);
            ViewBag.Language = parameters.Lang;
            return View("SearchAjax", data);
        }

        [Route("{language}/Ratings/DebitAjax")]
        public async Task<IActionResult> DebitAjax([FromQuery] RatingResultsQueryParameters parameters)
        {
            parameters.FromDate = "2000-01-01";
            parameters.ToDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            parameters.IssuerTypesId = 2; // Set a default industry type ID
            var data = await _newsAPIService.GetRatingResultsAsync(parameters, _baseSettings.Value.PortalId);
            ViewBag.Language = parameters.Lang;
            return View("DebitAjax", data);
        }

        [Route("{language}/Ratings/IssuerOrgansAsync")]
        public async Task<IActionResult> IssuerOrgansAsync([FromQuery] IssuerOrgansQueryParameters parameters)
        {
            var data = await _newsAPIService.GetIssuerOrgansAsync(parameters.Keyword, parameters.Lang, _baseSettings.Value.PortalId);
            ViewBag.Language = parameters.Lang;
            return View("IssuerOrgansAsync", data);
        }

        [Route("{language}/Ratings/ChartData")]
        public async Task<IActionResult> ChartData([FromQuery] ChartDataQueryParameters parameters, string language)
        {
            var outData = new ChartDataResult();
            var data = new RatingResultsResponse();
            var scores = await _newsAPIService.GetReportScoresAsync(_baseSettings.Value.PortalId);
            if(parameters.Type == 2)
            {
                data = await _newsAPIService.GetRatingResultsAsync(new RatingResultsQueryParameters { FromDate = parameters.FromDate, ToDate = parameters.ToDate, OrganizationId = parameters.OrganizationId, IssuerTypesId = 2 }, _baseSettings.Value.PortalId);
            }
            else
            {
                data = await _newsAPIService.GetRatingResultsAsync(new RatingResultsQueryParameters { FromDate = parameters.FromDate, ToDate = parameters.ToDate, OrganizationId = parameters.OrganizationId, IssuerTypesId = 1 }, _baseSettings.Value.PortalId);
               
                var data2 = await _newsAPIService.GetRatingResultsAsync(new RatingResultsQueryParameters { FromDate = parameters.FromDate, ToDate = parameters.ToDate, OrganizationId = parameters.OrganizationId, IssuerTypesId = 3 }, _baseSettings.Value.PortalId);
                if(data2 != null && data2.Data != null && data2.Data.Items != null && data2.Data.Items.Any())
                {
                    if(data != null && (data.Data == null || data.Data.Items == null))
                    {
                        data.Data = new RatingResultsData
                        {
                            Items = new List<RatingResultItem>()
                        };
                    }
                    data.Data.Items.AddRange(data2.Data.Items);
                }
            }

            outData.Data = data.Data.Items.OrderBy(x=>x.Date)
                .Select(g => new ChartDataDataItem
                {
                    Label = language == "vi" ? $"{Convert.ToDateTime(g.Date):dd/MM/yyyy}" : $"{Convert.ToDateTime(g.Date):MM/dd/yyyy}",
                    Value = g.Rating
                })
                .ToList();

            outData.Scores = scores.Data.Select(s => new ChartDataScoreItem
                {
                    ScoreValue = s.ScoreValue,
                    Value = s.ScoreId
                }).ToList();

            return Json(outData);
        }

        public class ChartDataResult
        {
            public List<ChartDataScoreItem> Scores { get; set; } = new List<ChartDataScoreItem>();
            public List<ChartDataDataItem> Data { get; set; } = new List<ChartDataDataItem>();
        }

        public class ChartDataScoreItem
        {
            public string ScoreValue { get; set; }
            public int Value { get; set; }
        }

        public class ChartDataDataItem
        {
            public string Label { get; set; }
            public string Value { get; set; }
        }
    }
}
