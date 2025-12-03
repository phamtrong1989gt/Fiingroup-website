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
        Task<string> GetAccessTokenAsync(bool clearCache = false);
        Task<NewsListResponse> GetNewsAsync(NewsQueryParameters parameters, string language = "vi");
        Task<NewsDetailResponse> GetNewsByIdAsync(int id, string language = "vi");
    }

    public class NewsAPIService : INewsAPIService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string TOKEN_CACHE_KEY = "NewsAPI_AccessToken";

        public NewsAPIService(
    ISeoSettingRepository iSeoSettingRepository,
     IMemoryCache memoryCache,
            IBindContentSettingRepository iBindContentSettingRepository,
            IEmailSettingRepository iEmailSettingRepository,
            IOptions<BaseSettings> baseSettings,
            IHttpClientFactory httpClientFactory)
        {
            _memoryCache = memoryCache;
            _baseSettings = baseSettings;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Lấy Access Token từ API với cache
        /// </summary>
        /// <param name="clearCache">True: Xóa cache và lấy token mới. False: Dùng cache nếu có</param>
        /// <returns>Access Token</returns>
        public async Task<string> GetAccessTokenAsync(bool clearCache = false)
        {
            if (clearCache)
            {
                _memoryCache.Remove(TOKEN_CACHE_KEY);
            }

            if (_memoryCache.TryGetValue(TOKEN_CACHE_KEY, out string cachedToken))
            {
                return cachedToken;
            }

            var settings = _baseSettings.Value.NewAPI;
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
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                if (string.IsNullOrEmpty(tokenResponse?.AccessToken))
                {
                    throw new Exception("Access token is null or empty");
                }

                var cacheExpiration = TimeSpan.FromSeconds(tokenResponse.ExpiresIn > 60 ? tokenResponse.ExpiresIn - 60 : tokenResponse.ExpiresIn);

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheExpiration,
                    Priority = CacheItemPriority.High,
                    Size = 10
                };

                _memoryCache.Set(TOKEN_CACHE_KEY, tokenResponse.AccessToken, cacheOptions);

                return tokenResponse.AccessToken;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to get access token: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting access token: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lấy danh sách tin tức từ API với retry 1 lần duy nhất khi gặp 401
        /// </summary>
        /// <param name="parameters">Query parameters</param>
        /// <param name="language">Ngôn ngữ: "vi" hoặc "en"</param>
        /// <returns>NewsListResponse hoặc null nếu thất bại</returns>
        public async Task<NewsListResponse> GetNewsAsync(NewsQueryParameters parameters, string language = "vi")
        {
            var settings = _baseSettings.Value.NewAPI;
            var endpoint = language.ToLower() == "vi" ? settings.NewsEndpointVI : settings.NewsEndpointEN;
            var newsUrl = $"{endpoint}";

            // Lấy token
            string token;
            try
            {
                token = await GetAccessTokenAsync();
            }
            catch (Exception)
            {
                // Không thể lấy token, trả về null
                return null;
            }

            // ✅ Lần 1: Gọi API với token hiện tại
            try
            {
                return await CallNewsAPIAsync(newsUrl, token, parameters);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                // ✅ Gặp 401: Thử lấy token mới và retry 1 LẦN DUY NHẤT
                try
                {
                    token = await GetAccessTokenAsync(clearCache: true);
                    return await CallNewsAPIAsync(newsUrl, token, parameters);
                }
                catch (Exception)
                {
                    // ✅ Retry thất bại, trả về null
                    return null;
                }
            }
            catch (Exception)
            {
                // ✅ Lỗi khác, trả về null
                return null;
            }
        }

        /// <summary>
        /// Lấy chi tiết 1 bản tin theo ID với retry 1 lần duy nhất khi gặp 401
        /// </summary>
        /// <param name="id">News ID</param>
        /// <param name="language">Ngôn ngữ: "vi" hoặc "en"</param>
        /// <returns>NewsDetailResponse hoặc null nếu thất bại</returns>
        public async Task<NewsDetailResponse> GetNewsByIdAsync(int id, string language = "vi")
        {
            var settings = _baseSettings.Value.NewAPI;
            var endpoint = language.ToLower() == "vi"
           ? settings.NewsEndpointVI.Replace("/Gets", "/Get")
            : settings.NewsEndpointEN.Replace("/Gets", "/Get");
            var newsUrl = $"{endpoint}?id={id}";

            // Lấy token
            string token;
            try
            {
                token = await GetAccessTokenAsync();
            }
            catch (Exception)
            {
                // Không thể lấy token, trả về null
                return null;
            }

            // ✅ Lần 1: Gọi API với token hiện tại
            try
            {
              //  newsUrl = "http://113.160.94.133:5050/FGFN/api/NewsEN/Get?id=6410206";
                return await CallNewsDetailAPIAsync(newsUrl, token);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                // ✅ Gặp 401: Thử lấy token mới và retry 1 LẦN DUY NHẤT
                try
                {
                    token = await GetAccessTokenAsync(clearCache: true);
                    return await CallNewsDetailAPIAsync(newsUrl, token);
                }
                catch (Exception)
                {
                    // ✅ Retry thất bại, trả về null
                    return null;
                }
            }
            catch (Exception)
            {
                // ✅ Lỗi khác, trả về null
                return null;
            }
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

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var newsResponse = JsonConvert.DeserializeObject<NewsListResponse>(responseContent);

            return newsResponse;
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
    }

}