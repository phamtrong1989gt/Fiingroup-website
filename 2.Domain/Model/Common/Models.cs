using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PT.Domain.Model
{

    #region Models

    /// <summary>
    /// Query parameters cho News API
    /// </summary>
    public class NewsQueryParameters
    {
        public string ExCategoryIds { get; set; }
        /// <summary>
        /// Số trang (default: 1)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Số item trên 1 trang (default: 10)
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Từ khóa tìm kiếm (theo title/short content)
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// Danh sách Category IDs (comma-separated), e.g. "1,2,3"
        /// </summary>
        public string CategoryIds { get; set; }

        /// <summary>
        /// Type ID để filter
        /// </summary>
        public int? TypeId { get; set; }

        /// <summary>
        /// Source ID để filter
        /// </summary>
        public int? SourceId { get; set; }

        /// <summary>
        /// Từ ngày (format: yyyy-MM-dd)
        /// </summary>
        public string FromDate { get; set; }

        /// <summary>
        /// Đến ngày (format: yyyy-MM-dd)
        /// </summary>
        public string ToDate { get; set; }

        /// <summary>
        /// Status (default: "Active")
        /// </summary>
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Tags (comma-separated)
        /// </summary>
        public string Tags { get; set; }
        public string Language { get; set; }

        public int? CategoryId { get; set; }

        /// <summary>
        /// Convert to query string
        /// </summary>
        public string ToQueryString()
        {
            var queryParams = new List<string>();

            queryParams.Add($"page={Page}");
            queryParams.Add($"pageSize={PageSize}");

            if (!string.IsNullOrEmpty(Keyword))
                queryParams.Add($"keyword={Uri.EscapeDataString(Keyword)}");

            if (!string.IsNullOrEmpty(CategoryIds))
                queryParams.Add($"categoryIds={Uri.EscapeDataString(CategoryIds)}");

            if (TypeId.HasValue)
                queryParams.Add($"typeId={TypeId.Value}");

            if (SourceId.HasValue)
                queryParams.Add($"sourceId={SourceId.Value}");

            if (!string.IsNullOrEmpty(FromDate))
                queryParams.Add($"fromDate={Uri.EscapeDataString(FromDate)}");

            if (!string.IsNullOrEmpty(ToDate))
                queryParams.Add($"toDate={Uri.EscapeDataString(ToDate)}");

            if (!string.IsNullOrEmpty(Status))
                queryParams.Add($"status={Uri.EscapeDataString(Status)}");

            if (!string.IsNullOrEmpty(Tags))
                queryParams.Add($"tags={Uri.EscapeDataString(Tags)}");

            return string.Join("&", queryParams);
        }
    }

    /// <summary>
    /// Response model cho danh sách tin tức (theo cấu trúc API thực tế)
    /// </summary>
    public class NewsListResponse
    {
        /// <summary>
        /// Tổng số bản ghi
        /// </summary>
        [JsonProperty("total")]
        public int Total { get; set; }

        /// <summary>
        /// Danh sách tin tức
        /// </summary>
        [JsonProperty("items")]
        public List<NewsItem> Items { get; set; }

        // Helper properties để tương thích với code cũ
        [Newtonsoft.Json.JsonIgnore]
        public int TotalRecords => Total;

        [Newtonsoft.Json.JsonIgnore]
        public List<NewsItem> Data => Items;

  
    }

    /// <summary>
    /// Response model cho chi tiết 1 bản tin
    /// </summary>
    public class NewsDetailResponse
    {
        [JsonProperty("data")]
        public NewsDetail Data { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Model cho 1 tin tức trong danh sách (theo cấu trúc API thực tế)
    /// </summary>
    public class NewsItem
    {
        /// <summary>
        /// Record ID
        /// </summary>
        [JsonProperty("recordId")]
        public long RecordId { get; set; }

        /// <summary>
        /// News ID
        /// </summary>
        [JsonProperty("newsId")]
        public long NewsId { get; set; }

        /// <summary>
        /// Tiêu đề tin tức
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Friendly title (slug)
        /// </summary>
        [JsonProperty("friendlyTitle")]
        public string FriendlyTitle { get; set; }

        /// <summary>
        /// Nội dung ngắn
        /// </summary>
        [JsonProperty("shortContent")]
        public string ShortContent { get; set; }

        /// <summary>
        /// URL hình ảnh
        /// </summary>
        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("sourceUrl")]
        public string SourceUrl { get; set; }

        /// <summary>
        /// Tác giả
        /// </summary>
        [JsonProperty("author")]
        public string Author { get; set; }

        /// <summary>
        /// Ngày công khai
        /// </summary>
        [JsonProperty("publicDate")]
        public DateTime? PublicDate { get; set; }

        /// <summary>
        /// Danh sách Category IDs
        /// </summary>
        [JsonProperty("categoryIds")]
        public List<int> CategoryIds { get; set; }

        /// <summary>
        /// Danh sách Type IDs
        /// </summary>
        [JsonProperty("typeIds")]
        public List<int> TypeIds { get; set; }

        [JsonProperty("sourceIds")]
        public List<int> SourceIds { get; set; }

        /// <summary>
        /// Trạng thái (1: Active, 0: Inactive)
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }


        [JsonProperty("categories")]
        public List<CategoryInfo> Categories { get; set; }

        [JsonProperty("types")]
        public List<TypeInfo> Types { get; set; }

        [JsonProperty("sources")]
        public List<SourceInfo> Sources { get; set; }

        [JsonProperty("tags")]
        public List<TagInfo> Tags { get; set; }

        [JsonProperty("entities")]
        public List<EntityInfo> Entities { get; set; }

        // Helper properties để tương thích
        [Newtonsoft.Json.JsonIgnore]
        public long Id => NewsId;

        [Newtonsoft.Json.JsonIgnore]
        public DateTime? PublishedDate => PublicDate;
        public DateTime? StartDate { get; set; }

        public string TimeFromTo { get; set; }
        public string Address { get; set; }

    }

    /// <summary>
    /// Model cho chi tiết tin tức (có thêm Content đầy đủ)
    /// </summary>
    public class NewsDetail
    {
        [JsonProperty("recordId")]
        public long RecordId { get; set; }

        [JsonProperty("newsId")]
        public long NewsId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("friendlyTitle")]
        public string FriendlyTitle { get; set; }

        [JsonProperty("shortContent")]
        public string ShortContent { get; set; }

        /// <summary>
        /// Nội dung đầy đủ (chỉ có trong detail)
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("sourceUrl")]
        public string SourceUrl { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("publicDate")]
        public DateTime? PublicDate { get; set; }

        [JsonProperty("categoryIds")]
        public List<int> CategoryIds { get; set; }

        [JsonProperty("typeIds")]
        public List<int> TypeIds { get; set; }

        [JsonProperty("sourceIds")]
        public List<int> SourceIds { get; set; }

        /// <summary>
        /// Trạng thái (1: Active, 0: Inactive)
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("entities")]
        public List<object> Entities { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("viewCount")]
        public int? ViewCount { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public long Id => NewsId;

        [Newtonsoft.Json.JsonIgnore]
        public DateTime? PublishedDate => PublicDate;
    }

    /// <summary>
    /// Model cho response token
    /// </summary>
    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }
    }

    #endregion
    public enum ModuleType
    {
        StaticInformation,
        Menu,
        AdvertisingBanner,
        PhotoSlide
    }

    public class PluginFileUploadModel
    {
        public string ParentTagName { get; set; }
        public string Extention { get; set; }
        public double MaxSize { get; set; }
        public string UrlUpload { get; set; }
        public string UrlDelete { get; set; }
        public string UrlFile { get; set; }
        public string InputName { get; set; }
        public List<FileDataModel> FileDatas { get; set; } = new List<FileDataModel>();
        public string FileDataString { get; set; }
        public bool? IsS { get; set; }
        public bool IsSingle { get; set; }
    }
    public class FileDataModel
    {
        public string Path { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedUser { get; set; }
    }
    public class FileDataCKEditerModel
    {
        public int Uploaded { get; set; }
        public string FileName { get; set; }
        public int Number { get; set; }
        public string Url { get; set; }
    }

    public class CategoryInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("priorityOrder")]
        public int PriorityOrder { get; set; }
    }

    public class TypeInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class SourceInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class TagInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class EntityInfo
    {
        [JsonProperty("organizationId")]
        public int OrganizationId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("priorityOrder")]
        public int PriorityOrder { get; set; }
        [JsonProperty("entityAssetTypeId")]
        public int EntityAssetTypeId { get; set; }
    }
}
