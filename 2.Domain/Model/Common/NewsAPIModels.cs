using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PT.Domain.Model
{
    // ============ RATING API MODELS (NEW) ============

    // Report Scores Response
    public class ReportScoresResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public List<ScoreItem> Data { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class ScoreItem
    {
        [JsonProperty("scoreId")]
        public int ScoreId { get; set; }

        [JsonProperty("scoreValue")]
        public string ScoreValue { get; set; }
    }

    // Report Industries Response
    public class ReportIndustriesResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public List<IndustryItem> Data { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class IndustryItem
    {
        [JsonProperty("industryTypeId")]
        public int IndustryTypeId { get; set; }

        [JsonProperty("industryTypeName")]
        public string IndustryTypeName { get; set; }
    }

    // Report Outlooks Response
    public class ReportOutlooksResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public List<OutlookItem> Data { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class OutlookItem
    {
        [JsonProperty("prospectsId")]
        public string ProspectsId { get; set; }

        [JsonProperty("prospects")]
        public string Prospects { get; set; }
    }

    // Sustainable Finance Response
    public class SustainableFinanceResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public SustainableFinanceData Data { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class SustainableFinanceData
    {
        [JsonProperty("items")]
        public List<SustainableFinanceItem> Items { get; set; }

        [JsonProperty("totalRecords")]
        public int TotalRecords { get; set; }

        [JsonProperty("currentPage")]
        public int CurrentPage { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
    }

    public class SustainableFinanceItem
    {
        [JsonProperty("publicDate")]
        public string PublicDate { get; set; }

        [JsonProperty("organizationName")]
        public string OrganizationName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("industryName")]
        public string IndustryName { get; set; }

        [JsonProperty("applicableStandardsName")]
        public string ApplicableStandardsName { get; set; }

        [JsonProperty("files")]
        public FileLinks Files { get; set; }

        [JsonProperty("reportId")]
        public int ReportId { get; set; }

        [JsonProperty("organizationId")]
        public int OrganizationId { get; set; }

        [JsonProperty("industryId")]
        public int IndustryId { get; set; }
    }

    public class FileLinks
    {
        [JsonProperty("vi")]
        public string Vi { get; set; }

        [JsonProperty("en")]
        public string En { get; set; }
    }

    // Query Parameters for Sustainable Finance
    public class SustainableFinanceQueryParameters
    {
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public int? IndustryId { get; set; }
        public int? ApplicableStandardId { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string Lang { get; set; }

        public string ToQueryString()
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(Title))
            {
                queryParams.Add($"title={Uri.EscapeDataString(Title)}");
            }

            if (!string.IsNullOrWhiteSpace(CompanyName))
            {
                queryParams.Add($"companyName={Uri.EscapeDataString(CompanyName)}");
            }

            if (IndustryId.HasValue)
            {
                queryParams.Add($"industryId={IndustryId.Value}");
            }

            if (ApplicableStandardId.HasValue)
            {
                queryParams.Add($"applicableStandardId={ApplicableStandardId.Value}");
            }

            if (Page.HasValue)
            {
                queryParams.Add($"page={Page.Value}");
            }

            if (PageSize.HasValue)
            {
                queryParams.Add($"pageSize={PageSize.Value}");
            }

            if (!string.IsNullOrWhiteSpace(Lang))
            {
                queryParams.Add($"lang={Lang}");
            }

            return string.Join("&", queryParams);
        }
    }

    // ============ RATING RESULTS (for listing page) ============
    public class RatingResultsResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public RatingResultsData Data { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class RatingResultsData
    {
        [JsonProperty("items")]
        public List<RatingResultItem> Items { get; set; }

        [JsonProperty("totalRecords")]
        public int TotalRecords { get; set; }

        [JsonProperty("currentPage")]
        public int CurrentPage { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
    }

    public class RatingResultItem
    {
        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("issuer")]
        public string Issuer { get; set; }

        [JsonProperty("industry")]
        public string Industry { get; set; }

        [JsonProperty("ratingType")]
        public string RatingType { get; set; }

        [JsonProperty("rating")]
        public string Rating { get; set; }

        [JsonProperty("outlook")]
        public string Outlook { get; set; }

        [JsonProperty("announcement")]
        public FileLinks Announcement { get; set; }

        [JsonProperty("fullReport")]
        public FileLinks FullReport { get; set; }

        [JsonProperty("eventLink")]
        public string EventLink { get; set; }

        [JsonProperty("ratingId")]
        public int RatingId { get; set; }

        [JsonProperty("organizationId")]
        public int OrganizationId { get; set; }
    }

    // Query Parameters for Rating Results
    public class RatingResultsQueryParameters
    {
        public string CompanyName { get; set; }
        public int? IndustryTypeId { get; set; }
        public int? ScoreId { get; set; }
        public string ProspectsId { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string Lang { get; set; }

        public string ToQueryString()
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(CompanyName))
            {
                queryParams.Add($"companyName={Uri.EscapeDataString(CompanyName)}");
            }

            if (IndustryTypeId.HasValue)
            {
                queryParams.Add($"industryTypeId={IndustryTypeId.Value}");
            }

            if (ScoreId.HasValue)
            {
                queryParams.Add($"scoreId={ScoreId.Value}");
            }

            if (!string.IsNullOrWhiteSpace(ProspectsId))
            {
                queryParams.Add($"prospectsId={ProspectsId}");
            }

            if (Page.HasValue)
            {
                queryParams.Add($"page={Page.Value}");
            }

            if (PageSize.HasValue)
            {
                queryParams.Add($"pageSize={PageSize.Value}");
            }

            if (!string.IsNullOrWhiteSpace(Lang))
            {
                queryParams.Add($"lang={Lang}");
            }

            return string.Join("&", queryParams);
        }
    }
}
