using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using Newtonsoft.Json;

namespace PT.Base.Services
{
    public interface INewsAPIService
    {
        Task<string> GetAccessTokenAsync(bool clearCache, int portalId, bool isRatingAPI);
        Task<NewsListResponse> GetNewsAsync(NewsQueryParameters parameters, string language = "vi", int portalId = 1);
        Task<NewsDetailResponse> GetNewsByIdAsync(int id, string language = "vi", int portalId = 1);
        
        // ============ NEW RATING API METHODS ============
        Task<ReportScoresResponse> GetReportScoresAsync(int portalId = 1);
        Task<ReportIndustriesResponse> GetReportIndustriesAsync(string language = "vi", int portalId = 1);
        Task<ReportOutlooksResponse> GetReportOutlooksAsync(string language = "vi", int portalId = 1);
        Task<SustainableFinanceResponse> GetSustainableFinanceReportsAsync(SustainableFinanceQueryParameters parameters, string language = "vi", int portalId = 1);
        Task<RatingResultsResponse> GetRatingResultsAsync(RatingResultsQueryParameters parameters, int portalId = 1);
        
        // ============ NEW SUSTAINABLE FINANCE API METHODS ============
        Task<SustainableIndustriesResponse> GetSustainableIndustriesAsync(string language = "vi", int portalId = 1);
        Task<SustainableStandardsResponse> GetSustainableStandardsAsync(string language = "vi", int portalId = 1);

        // ============ ADDED: Issuer types & Opinion types ==========
        Task<IssuerTypesResponse> GetIssuerTypesAsync(string language = "vi", int portalId = 1);
        Task<OpinionTypesResponse> GetOpinionTypesAsync(string language = "vi", int portalId = 1);
        Task<IssuerOrgansResponse> GetIssuerOrgansAsync(string keyword = "", string language = "vi", int portalId = 1);
    }

    public class NewsAPIService : INewsAPIService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAPILoggerService _apiLogger;
        private const string TOKEN_CACHE_KEY = "NewsAPI_AccessToken";

        public NewsAPIService(
            ISeoSettingRepository iSeoSettingRepository,
            IMemoryCache memoryCache,
            IBindContentSettingRepository iBindContentSettingRepository,
            IEmailSettingRepository iEmailSettingRepository,
            IOptions<BaseSettings> baseSettings,
            IHttpClientFactory httpClientFactory,
            IAPILoggerService apiLogger)
        {
            _memoryCache = memoryCache;
            _baseSettings = baseSettings;
            _httpClientFactory = httpClientFactory;
            _apiLogger = apiLogger;
        }

        /// <summary>
        /// Lấy Access Token từ API với cache
        /// </summary>
        /// <param name="clearCache">True: Xóa cache và lấy token mới. False: Dùng cache nếu có</param>
        /// <returns>Access Token</returns>
        public async Task<string> GetAccessTokenAsync(bool clearCache, int portalId, bool isRatingAPI)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            if (clearCache)
            {
                _memoryCache.Remove(TOKEN_CACHE_KEY);
            }

            if (_memoryCache.TryGetValue(TOKEN_CACHE_KEY, out string cachedToken))
            {
                return cachedToken;
            }

            var settings = _baseSettings.Value.NewAPI;
            if(isRatingAPI)
            {
                settings = _baseSettings.Value.RatingAPI;
            }

            var tokenUrl = $"{settings.TokenEndpoint}";

            try
            {
                using var httpClient = _httpClientFactory.CreateClient();

                var formData = new Dictionary<string, string>
                {
                    { "grant_type", settings.GrantType },
                    { "client_id", settings.ClientId },
                    { "client_secret", settings.ClientSecret },
                    { "scope", settings.Scope },
                    { "username", settings.Username },
                    { "password", settings.Password }
                };

                var content = new FormUrlEncodedContent(formData);
                var response = await httpClient.PostAsync(tokenUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    throw new Exception("Response content is null or empty");
                }

                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                {
                    throw new Exception("Access token is null or empty");
                }

                if (tokenResponse.ExpiresIn <= 0)
                {
                    throw new Exception("Token ExpiresIn is invalid (must be > 0)");
                }

                var cacheExpiration = TimeSpan.FromSeconds(tokenResponse.ExpiresIn > 60 ? tokenResponse.ExpiresIn - 60 : tokenResponse.ExpiresIn);

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheExpiration,
                    Priority = CacheItemPriority.High,
                    Size = 10
                };

                _memoryCache.Set(TOKEN_CACHE_KEY, tokenResponse.AccessToken, cacheOptions);

                stopwatch.Stop();
                
                // ✅ Log thành công với portalId
                _ = _apiLogger.LogAPICallAsync(new APILogModel
                {
                    Type = LogType.API_GetToken,
                    Endpoint = tokenUrl,
                    Method = "POST",
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    StatusCode = 200,
                    Success = true,
                    RequestParams = $"portal={portalId}, grant_type={settings.GrantType}, client_id={settings.ClientId}",
                    PortalId = portalId
                });

                return tokenResponse.AccessToken;
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                
                // ✅ Log lỗi HTTP với portalId
                _ = _apiLogger.LogAPICallAsync(new APILogModel
                {
                    Type = LogType.API_Error_Other,
                    Endpoint = tokenUrl,
                    Method = "POST",
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    StatusCode = (int)(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError),
                    Success = false,
                    ErrorMessage = ex.Message,
                    PortalId = portalId
                });
                
                throw new Exception($"Failed to get access token: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                stopwatch.Stop();
                
                _ = _apiLogger.LogAPICallAsync(new APILogModel
                {
                    Type = LogType.API_Error_Other,
                    Endpoint = tokenUrl,
                    Method = "POST",
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    StatusCode = 500,
                    Success = false,
                    ErrorMessage = $"JSON Error: {ex.Message}",
                    PortalId = portalId
                });
                
                throw new Exception($"Invalid JSON response: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                _ = _apiLogger.LogAPICallAsync(new APILogModel
                {
                    Type = LogType.API_Error_Other,
                    Endpoint = tokenUrl,
                    Method = "POST",
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    StatusCode = 500,
                    Success = false,
                    ErrorMessage = ex.Message,
                    PortalId = portalId
                });
                
                throw new Exception($"Error getting access token: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lấy danh sách tin tức từ API với retry 1 lần duy nhất khi gặp 401
        /// </summary>
        /// <param name="parameters">Query parameters</param>
        /// <param name="language">Ngôn ngữ: "vi" hoặc "en"</param>
        /// <returns>NewsListResponse hoặc null nếu thất bại</returns>
        public async Task<NewsListResponse> GetNewsAsync(NewsQueryParameters parameters, string language = "vi", int portalId = 1)
        {
            return await _apiLogger.TrackAPICallAsync(
                LogType.API_GetNews,
                $"{_baseSettings.Value.NewAPI.NewsEndpointVI}",
                "GET",
                async () =>
                {
                    var settings = _baseSettings.Value.NewAPI;
                    var endpoint = language.ToLower() == "vi" ? settings.NewsEndpointVI : settings.NewsEndpointEN;
                    var newsUrl = $"{endpoint}";

                    string token;
                    try
                    {
                        token = await GetAccessTokenAsync(false, portalId, false);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    try
                    {
                        return await CallNewsAPIAsync(newsUrl, token, parameters);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _ = _apiLogger.LogAPICallAsync(new APILogModel
                        {
                            Type = LogType.API_Error_Unauthorized,
                            Endpoint = newsUrl,
                            Method = "GET",
                            StatusCode = 401,
                            Success = false,
                            ErrorMessage = "Token expired, retrying with new token",
                            Language = language,
                            PortalId = portalId
                        });

                        try
                        {
                            token = await GetAccessTokenAsync(clearCache: true, portalId: portalId, false);
                            return await CallNewsAPIAsync(newsUrl, token, parameters);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                },
                language,
                parameters?.ToQueryString(),
                portalId
            );
        }

        /// <summary>
        /// Lấy chi tiết 1 bản tin theo ID với retry 1 lần duy nhất khi gặp 401
        /// </summary>
        /// <param name="id">News ID</param>
        /// <param name="language">Ngôn ngữ: "vi" hoặc "en"</param>
        /// <returns>NewsDetailResponse hoặc null nếu thất bại</returns>
        public async Task<NewsDetailResponse> GetNewsByIdAsync(int id, string language = "vi", int portalId = 1)
        {
            return await _apiLogger.TrackAPICallAsync(
                LogType.API_GetNewsDetail,
                $"{_baseSettings.Value.NewAPI.NewsEndpointVI}/Get?id={id}",
                "GET",
                async () =>
                {
                    var settings = _baseSettings.Value.NewAPI;
                    var endpoint = language.ToLower() == "vi" ? settings.NewsEndpointVI.Replace("/Gets", "/Get") : settings.NewsEndpointEN.Replace("/Gets", "/Get");
                    var newsUrl = $"{endpoint}?id={id}";

                    string token;
                    try
                    {
                        token = await GetAccessTokenAsync(false, portalId, false);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    try
                    {
                        return await CallNewsDetailAPIAsync(newsUrl, token);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _ = _apiLogger.LogAPICallAsync(new APILogModel
                        {
                            Type = LogType.API_Error_Unauthorized,
                            Endpoint = newsUrl,
                            Method = "GET",
                            StatusCode = 401,
                            Success = false,
                            ErrorMessage = "Token expired, retrying",
                            Language = language,
                            RequestParams = $"id={id}",
                            PortalId = portalId
                        });

                        try
                        {
                            token = await GetAccessTokenAsync(clearCache: true,  portalId, false);
                            return await CallNewsDetailAPIAsync(newsUrl, token);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                },
                language,
                $"id={id}",
                portalId
            );
        }

        /// <summary>
        /// Internal method để gọi News API (danh sách)
        /// </summary>
        private async Task<NewsListResponse> CallNewsAPIAsync(string url, string token, NewsQueryParameters parameters)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var queryString = parameters.ToQueryString();
            var requestUrl = string.IsNullOrEmpty(queryString) ? url : $"{url}?{queryString}";

            var response = await httpClient.GetAsync(requestUrl);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
            try
            {
                var newsResponse = JsonConvert.DeserializeObject<NewsListResponse>(responseContent);
                return newsResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Internal method để gọi News Detail API (1 bản tin)
        /// </summary>
        private async Task<NewsDetailResponse> CallNewsDetailAPIAsync(string url, string token)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);
            }

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var newsDetail = JsonConvert.DeserializeObject<NewsDetail>(responseContent);
            var outData = new NewsDetailResponse
            {
                Data = newsDetail,
                Success = newsDetail != null
            };
            return outData;
        }

        // ============ RATING API IMPLEMENTATIONS ============

        /// <summary>
        /// Lấy danh sách điểm xếp hạng (Scores) - không cache
        /// </summary>
        /// <returns>ReportScoresResponse hoặc null nếu thất bại</returns>
        public async Task<ReportScoresResponse> GetReportScoresAsync(int portalId = 1)
        {
            return await _apiLogger.TrackAPICallAsync(
                LogType.API_GetReportScores,
                _baseSettings.Value.RatingAPI.ReportScoresEndpoint ?? "N/A",
                "GET",
                async () =>
                {
                    var settings = _baseSettings.Value.RatingAPI;
                    var endpoint = settings.ReportScoresEndpoint;

                    if (string.IsNullOrWhiteSpace(endpoint))
                    {
                        return null;
                    }

                    string token;
                    try
                    {
                        token = await GetAccessTokenAsync(false, portalId, true);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    try
                    {
                        return await CallRatingAPIAsync<ReportScoresResponse>(endpoint, token);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        try
                        {
                            token = await GetAccessTokenAsync(clearCache: true, portalId, true);
                            return await CallRatingAPIAsync<ReportScoresResponse>(endpoint, token);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                },
                null,
                null,
                portalId
            );
        }

        /// <summary>
        /// Lấy danh sách ngành (Industries) - không cache
        /// </summary>
        /// <param name="language">Ngôn ngữ: "vi" hoặc "en"</param>
        /// <returns>ReportIndustriesResponse hoặc null nếu thất bại</returns>
        public async Task<ReportIndustriesResponse> GetReportIndustriesAsync(string language = "vi", int portalId = 1)
        {
            return await _apiLogger.TrackAPICallAsync(
                LogType.API_GetIndustries,
                _baseSettings.Value.RatingAPI.ReportIndustriesEndpoint ?? "N/A",
                "GET",
                async () =>
                {
                    var settings = _baseSettings.Value.RatingAPI;
                    var endpoint = settings.ReportIndustriesEndpoint;

                    if (string.IsNullOrWhiteSpace(endpoint))
                    {
                        return null;
                    }

                    var url = $"{endpoint}?lang={language}";

                    string token;
                    try
                    {
                        token = await GetAccessTokenAsync(false, portalId, true);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    try
                    {
                        return await CallRatingAPIAsync<ReportIndustriesResponse>(url, token);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        try
                        {
                            token = await GetAccessTokenAsync(clearCache: true, portalId, true);
                            return await CallRatingAPIAsync<ReportIndustriesResponse>(url, token);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                },
                language,
                null,
                portalId
            );
        }

        /// <summary>
        /// Lấy danh sách triển vọng (Outlooks) - không cache
        /// </summary>
        /// <param name="language">Ngôn ngữ: "vi" hoặc "en"</param>
        /// <returns>ReportOutlooksResponse hoặc null nếu thất bại</returns>
        public async Task<ReportOutlooksResponse> GetReportOutlooksAsync(string language = "vi", int portalId = 1)
        {
            return await _apiLogger.TrackAPICallAsync(
                LogType.API_GetOutlooks,
                _baseSettings.Value.RatingAPI.ReportOutlooksEndpoint ?? "N/A",
                "GET",
                async () =>
                {
                    var settings = _baseSettings.Value.RatingAPI;
                    var endpoint = settings.ReportOutlooksEndpoint;

                    if (string.IsNullOrWhiteSpace(endpoint))
                    {
                        return null;
                    }

                    var url = $"{endpoint}?lang={language}";

                    string token;
                    try
                    {
                        token = await GetAccessTokenAsync(false, portalId, true);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    try
                    {
                        return await CallRatingAPIAsync<ReportOutlooksResponse>(url, token);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        try
                        {
                            token = await GetAccessTokenAsync(clearCache: true, portalId: portalId, true);
                            return await CallRatingAPIAsync<ReportOutlooksResponse>(url, token);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                },
                language,
                null,
                portalId
            );
        }

        /// <summary>
        /// Lấy danh sách báo cáo tài chính bền vững - không cache
        /// </summary>
        /// <param name="parameters">Query parameters</param>
        /// <param name="language">Ngôn ngữ: "vi" hoặc "en"</param>
        /// <returns>SustainableFinanceResponse hoặc null nếu thất bại</returns>
        public async Task<SustainableFinanceResponse> GetSustainableFinanceReportsAsync(SustainableFinanceQueryParameters parameters, string language = "vi", int portalId = 1)
        {
            return await _apiLogger.TrackAPICallAsync(
                LogType.API_GetSustainableFinance,
                _baseSettings.Value.RatingAPI.SustainableFinanceEndpoint ?? "N/A",
                "GET",
                async () =>
                {
                    var settings = _baseSettings.Value.RatingAPI;
                    var endpoint = settings.SustainableFinanceEndpoint;

                    if (string.IsNullOrWhiteSpace(endpoint))
                    {
                        return null;
                    }

                    if (string.IsNullOrWhiteSpace(parameters.Lang))
                    {
                        parameters.Lang = language;
                    }

                    var queryString = parameters.ToQueryString();
                    var url = string.IsNullOrEmpty(queryString) ? endpoint : $"{endpoint}?{queryString}";

                    string token;
                    try
                    {
                        token = await GetAccessTokenAsync(false, portalId, true);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    try
                    {
                        return await CallRatingAPIAsync<SustainableFinanceResponse>(url, token);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        try
                        {
                            token = await GetAccessTokenAsync(clearCache: true, portalId: portalId, true);
                            return await CallRatingAPIAsync<SustainableFinanceResponse>(url, token);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                },
                language,
                parameters?.ToQueryString(),
                portalId
            );
        }

        /// <summary>
        /// Lấy kết quả xếp hạng tín nhiệm - không cache
        /// </summary>
        /// <param name="parameters">Query parameters</param>
        /// <returns>RatingResultsResponse hoặc null nếu thất bại</returns>
        public async Task<RatingResultsResponse> GetRatingResultsAsync(RatingResultsQueryParameters parameters, int portalId = 1)
        {
            return await _apiLogger.TrackAPICallAsync(
                LogType.API_GetRatingResults,
                _baseSettings.Value.RatingAPI.RatingResultsEndpoint ?? "N/A",
                "GET",
                async () =>
                {
                    var settings = _baseSettings.Value.RatingAPI;
                    var endpoint = settings.RatingResultsEndpoint;

                    if (string.IsNullOrWhiteSpace(endpoint))
                    {
                        return null;
                    }

                    var queryString = parameters.ToQueryString();
                    var url = string.IsNullOrEmpty(queryString) ? endpoint : $"{endpoint}?{queryString}";

                    string token;
                    try
                    {
                        token = await GetAccessTokenAsync(false, portalId, true);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    try
                    {
                        return await CallRatingAPIAsync<RatingResultsResponse>(url, token);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        try
                        {
                            token = await GetAccessTokenAsync(clearCache: true, portalId: portalId, true);
                            return await CallRatingAPIAsync<RatingResultsResponse>(url, token);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                },
                parameters?.Lang,
                parameters?.ToQueryString(),
                portalId
            );
        }

        /// <summary>
        /// Lấy danh sách ngành cho Sustainable Finance (Industries) - không cache
        /// </summary>
        /// <param name="language">Ngôn ngữ: "vi" hoặc "en"</param>
        /// <returns>SustainableIndustriesResponse hoặc null nếu thất bại</returns>
        public async Task<SustainableIndustriesResponse> GetSustainableIndustriesAsync(string language = "vi", int portalId = 1)
        {
            return await _apiLogger.TrackAPICallAsync(
                LogType.API_GetIndustries,
                _baseSettings.Value.RatingAPI.SustainableIndustriesEndpoint ?? "N/A",
                "GET",
                async () =>
                {
                    var settings = _baseSettings.Value.RatingAPI;
                    var endpoint = settings.SustainableIndustriesEndpoint;

                    if (string.IsNullOrWhiteSpace(endpoint))
                    {
                        return null;
                    }

                    var url = $"{endpoint}?lang={language}";

                    string token;
                    try
                    {
                        token = await GetAccessTokenAsync(false, portalId, true);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    try
                    {
                        return await CallRatingAPIAsync<SustainableIndustriesResponse>(url, token);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        try
                        {
                            token = await GetAccessTokenAsync(clearCache: true, portalId: portalId, true);
                            return await CallRatingAPIAsync<SustainableIndustriesResponse>(url, token);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                },
                language,
                null,
                portalId
            );
        }

        /// <summary>
        /// Lấy danh sách tiêu chuẩn áp dụng cho Sustainable Finance (Standards) - không cache
        /// </summary>
        /// <param name="language">Ngôn ngữ: "vi" hoặc "en"</param>
        /// <returns>SustainableStandardsResponse hoặc null nếu thất bại</returns>
        public async Task<SustainableStandardsResponse> GetSustainableStandardsAsync(string language = "vi", int portalId = 1)
        {
            return await _apiLogger.TrackAPICallAsync(
                LogType.API_GetIndustries,
                _baseSettings.Value.RatingAPI.SustainableStandardsEndpoint ?? "N/A",
                "GET",
                async () =>
                {
                    var settings = _baseSettings.Value.RatingAPI;
                    var endpoint = settings.SustainableStandardsEndpoint;

                    if (string.IsNullOrWhiteSpace(endpoint))
                    {
                        return null;
                    }

                    var url = $"{endpoint}?lang={language}";

                    string token;
                    try
                    {
                        token = await GetAccessTokenAsync(false, portalId, true);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    try
                    {
                        return await CallRatingAPIAsync<SustainableStandardsResponse>(url, token);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        try
                        {
                            token = await GetAccessTokenAsync(clearCache: true, portalId: portalId, true);
                            return await CallRatingAPIAsync<SustainableStandardsResponse>(url, token);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                },
                language,
                null,
                portalId
            );
        }

        /// <summary>
        /// ADDED: Lấy danh sách Issuer Types
        /// </summary>
        public async Task<IssuerTypesResponse> GetIssuerTypesAsync(string language = "vi", int portalId = 1)
        {
            return await _apiLogger.TrackAPICallAsync(
                LogType.API_GetIssuerTypes,
                _baseSettings.Value.RatingAPI.IssuerTypeEndpoint ?? "N/A",
                "GET",
                async () =>
                {
                    var endpoint = _baseSettings.Value.RatingAPI.IssuerTypeEndpoint;
                    if (string.IsNullOrWhiteSpace(endpoint)) return null;

                    var url = $"{endpoint}?lang={language}";

                    string token;
                    try { token = await GetAccessTokenAsync(false, portalId, true); }
                    catch { return null; }

                    try
                    {
                        return await CallRatingAPIAsync<IssuerTypesResponse>(url, token);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        try { token = await GetAccessTokenAsync(true, portalId, true); return await CallRatingAPIAsync<IssuerTypesResponse>(url, token); }
                        catch { return null; }
                    }
                    catch { return null; }
                },
                language,
                null,
                portalId
            );
        }

        /// <summary>
        /// ADDED: Lấy danh sách Opinion Types
        /// </summary>
        public async Task<OpinionTypesResponse> GetOpinionTypesAsync(string language = "vi", int portalId = 1)
        {
            return await _apiLogger.TrackAPICallAsync(
                LogType.API_GetOpinionTypes,
                _baseSettings.Value.RatingAPI.OpinionTypesEndpoint ?? "N/A",
                "GET",
                async () =>
                {
                    var endpoint = _baseSettings.Value.RatingAPI.OpinionTypesEndpoint;
                    if (string.IsNullOrWhiteSpace(endpoint)) return null;

                    var url = $"{endpoint}?lang={language}";

                    string token;
                    try { token = await GetAccessTokenAsync(false, portalId, true); }
                    catch { return null; }

                    try
                    {
                        return await CallRatingAPIAsync<OpinionTypesResponse>(url, token);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        try { token = await GetAccessTokenAsync(true, portalId, true); return await CallRatingAPIAsync<OpinionTypesResponse>(url, token); }
                        catch { return null; }
                    }
                    catch { return null; }
                },
                language,
                null,
                portalId
            );
        }

        /// <summary>
        /// ADDED: Lấy danh sách Issuer Organs (tổ chức/phát hành) từ Rating API theo keyword
        /// </summary>
        public async Task<IssuerOrgansResponse> GetIssuerOrgansAsync(string keyword = "", string language = "vi", int portalId = 1)
        {
            return await _apiLogger.TrackAPICallAsync(
                LogType.API_GetIssuerTypes,
                _baseSettings.Value.RatingAPI.IssuerOrgansEndpoint ?? "N/A",
                "GET",
                async () =>
                {
                    var endpoint = _baseSettings.Value.RatingAPI.IssuerOrgansEndpoint;
                    if (string.IsNullOrWhiteSpace(endpoint)) return null;

                    var url = string.IsNullOrEmpty(keyword) ? $"{endpoint}?lang={language}" : $"{endpoint}?lang={language}&keyword={Uri.EscapeDataString(keyword)}";

                    string token;
                    try { token = await GetAccessTokenAsync(false, portalId, true); }
                    catch { return null; }

                    try
                    {
                        return await CallRatingAPIAsync<IssuerOrgansResponse>(url, token);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        try { token = await GetAccessTokenAsync(true, portalId, true); return await CallRatingAPIAsync<IssuerOrgansResponse>(url, token); }
                        catch { return null; }
                    }
                    catch { return null; }
                },
                language,
                $"keyword={keyword}",
                portalId
            );
        }

        /// <summary>
        /// Internal method để gọi Rating API (generic)
        /// </summary>
        private async Task<T> CallRatingAPIAsync<T>(string url, string token) where T : class
        {
            using var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);
            }

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }

}