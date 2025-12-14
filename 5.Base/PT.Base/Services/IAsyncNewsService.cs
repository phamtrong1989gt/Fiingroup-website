using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PT.Base.Services
{
    public interface IAsyncNewsService
    {
        Task<string> GetAccessTokenAsync(bool clearCache = false);
        Task<NewsCreateResult> CreateAsync(ContentPage contentPage);
        Task<NewsCreateResult> UpdateAsync(ContentPage contentPage);
    }

    public class AsyncNewsService : IAsyncNewsService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<AsyncNewsSettings> _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string TOKEN_CACHE_KEY = "NewsAPI_AccessToken";
        private readonly IPortalRepository _iPortalRepository;
        private readonly IWebHostEnvironment _env;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly ILinkRepository _ilinkRepository;

        public AsyncNewsService(
            ISeoSettingRepository iSeoSettingRepository,
            IMemoryCache memoryCache,
            IBindContentSettingRepository iBindContentSettingRepository,
            IEmailSettingRepository iEmailSettingRepository,
            IOptions<AsyncNewsSettings> settings,
            IHttpClientFactory httpClientFactory, IPortalRepository iPortalRepository, IWebHostEnvironment env, ICategoryRepository iCategoryRepository, ILinkRepository iLinkRepository)
        {
            _memoryCache = memoryCache;
            _settings = settings;
            _httpClientFactory = httpClientFactory;
            _iPortalRepository = iPortalRepository;
            _iCategoryRepository = iCategoryRepository;
            _ilinkRepository = iLinkRepository;
            _env = env;
        }
  
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

            var tokenUrl = $"{_settings.Value.TokenEndpoint}";

            try
            {
                using var httpClient = _httpClientFactory.CreateClient();

                var formData = new Dictionary<string, string>
                {
                  { "grant_type", _settings.Value.GrantType },
                  { "client_id", _settings.Value.ClientId },
                  { "client_secret", _settings.Value.ClientSecret },
                  { "scope", _settings.Value.Scope },
                  { "username", _settings.Value.Username },
                  { "password", _settings.Value.Password }
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
                    Priority = CacheItemPriority.High
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

        public async Task<NewsCreateResult> CreateAsync(ContentPage contentPage)
        {
            var url = $"{_settings.Value.CreatedEndPointVI}";
            if(contentPage.Language == "en")
            {
                url = $"{_settings.Value.CreatedEndPointEN}";
            }
            if (string.IsNullOrWhiteSpace(url) || contentPage == null)
                return new NewsCreateResult { Success = false, ErrorMessage = "Invalid url or contentPage is null" };

            var portal = await _iPortalRepository.SingleOrDefaultAsync(true, x=> x.Id == contentPage.PortalId);
            if(portal == null)
                return new NewsCreateResult { Success = false, ErrorMessage = "Portal not found" };

            string domain = portal.Domain;
            if(_env.IsDevelopment())
            {
                domain = string.IsNullOrWhiteSpace(portal.DomainDev) ? portal.Domain : portal.DomainDev;
            }
            // remove trailing slash if present
            if (!string.IsNullOrWhiteSpace(domain))
            {
                domain = domain.TrimEnd('/');
            }
            try
            {
                int sourceId = 5; // Default source ID
                if(contentPage.PortalId == 1)
                {
                    sourceId = 299;
                }

                // Ánh xạ danh mục
                var category = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Id == contentPage.CategoryId);

                int? categoryId = category?.ReferentCategoryId;

                var cmd = new NewsCMD
                {
                    Title = contentPage.Name,
                    Content = contentPage.Content,
                    ShortContent = contentPage.Summary,
                    PublicDate = contentPage.DatePosted,
                    FriendlyTitle = contentPage.Link?.Slug,
                    ImageUrl = $"{contentPage.Banner}",
                    SourceUrl = contentPage.FullPath,
                    Author = contentPage.Author,
                    UpdateBy = null,
                    // 1 = Active, 4 = draft
                    StatusId = contentPage.Status ? 1 : 4,
                    Categories = (categoryId != null && categoryId > 0) ? [new() { Id = categoryId ?? 0, PriorityOrder = 1 }] : [],
                    TypeIds = [],
                    SourceIds = [sourceId],
                    Entities = [],
                    Tags = [],
                    ICBs = [],
                    VSICs = [],
                  //  CreateBy = "admin@fiingroup.vn"
                };

                async Task<HttpResponseMessage> PostWithTokenAsync(string bearerToken)
                {
                    using var client = _httpClientFactory.CreateClient();
                    if (!string.IsNullOrWhiteSpace(bearerToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                    }

                    var json = JsonConvert.SerializeObject(cmd);
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    return await client.PostAsync(url, content);
                }

                var token = await GetAccessTokenAsync();
                var response = await PostWithTokenAsync(token);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response.Dispose();
                    token = await GetAccessTokenAsync(clearCache: true);
                    response = await PostWithTokenAsync(token);
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var statusCode = (int)response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    response.Dispose();
                    var data = JsonConvert.DeserializeObject<NewsDTO>(responseContent);
                    return new NewsCreateResult { Success = true, Data = data, StatusCode = statusCode };
                }
                else
                {
                    response.Dispose();
                    string errorMsg = responseContent;
                    // Nếu là lỗi 400 hoặc 500, cố gắng parse lỗi dạng chuẩn
                    try
                    {
                        var errorObj = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        if (errorObj != null && errorObj.title != null && errorObj.detail != null)
                        {
                            errorMsg = $"{errorObj.title}: {errorObj.detail}";
                        }
                    }
                    catch { }
                    return new NewsCreateResult { Success = false, ErrorMessage = errorMsg, StatusCode = statusCode };
                }
            }
            catch (Exception ex)
            {
                return new NewsCreateResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<NewsCreateResult> UpdateAsync(ContentPage contentPage)
        {
            if (contentPage == null)
                return new NewsCreateResult { Success = false, ErrorMessage = "ContentPage is null" };

            var url = _settings.Value.EditEndPointVI;
            if (contentPage.Language == "en")
            {
                url = _settings.Value.EditEndPointEN;
            }
            if (string.IsNullOrWhiteSpace(url))
                return new NewsCreateResult { Success = false, ErrorMessage = "Update endpoint URL is not configured" };

            var portal = await _iPortalRepository.SingleOrDefaultAsync(true, x => x.Id == contentPage.PortalId);
            if (portal == null)
                return new NewsCreateResult { Success = false, ErrorMessage = "Portal not found" };

            string domain = portal.Domain;
            if (_env.IsDevelopment())
            {
                domain = string.IsNullOrWhiteSpace(portal.DomainDev) ? portal.Domain : portal.DomainDev;
            }
            // remove trailing slash if present
            if (!string.IsNullOrWhiteSpace(domain))
            {
                domain = domain.TrimEnd('/');
            }
            try
            {
                // Theo portal
                int sourceId = contentPage.PortalId == 1 ? 299 : 5;

                var link = await _ilinkRepository.SingleOrDefaultAsync(true, x => x.ObjectId == contentPage.Id && x.Type == contentPage.SlugType);
                if (link == null)
                {
                    return new NewsCreateResult { Success = false, ErrorMessage = "Link not found for content page" };
                }
                // Ánh xạ danh mục
                var category = await _iCategoryRepository.SingleOrDefaultAsync(true, x=>x.Id == contentPage.CategoryId);

                var categoryId = category?.ReferentCategoryId;

                var cmd = new NewsCMD
                {
                    // If NewsCMD has an Id for update, set it here:
                    // Id = contentPage.Id,
                    Title = contentPage.Name,
                    Content = contentPage.Content,
                    ShortContent = contentPage.Summary,
                    PublicDate = contentPage.DatePosted,
                    FriendlyTitle = contentPage.Link?.Slug,
                    ImageUrl = $"{contentPage.Banner}",
                    SourceUrl = contentPage.FullPath,
                    Author = contentPage.Author,
                    UpdateBy = null,
                    Categories = (categoryId != null && categoryId > 0) ? [new() { Id = categoryId ?? 0, PriorityOrder = 1 }] : [],
                    TypeIds = [],
                    SourceIds = [sourceId],
                    Entities = [],
                    Tags = [],
                    ICBs = [],
                    VSICs = [],
                    NewsId = contentPage.NewsId.HasValue ? contentPage.NewsId.Value : 0,
                    // 1 = Active, 4 = draft
                    StatusId = contentPage.Status ? 1 : 4,
                };

                async Task<HttpResponseMessage> PutWithTokenAsync(string bearerToken, string endpoint)
                {
                    using var client = _httpClientFactory.CreateClient();
                    if (!string.IsNullOrWhiteSpace(bearerToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                    }

                    var json = JsonConvert.SerializeObject(cmd);
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    return await client.PutAsync(endpoint, content);
                }

                var token = await GetAccessTokenAsync();
                var response = await PutWithTokenAsync(token, url);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response.Dispose();
                    token = await GetAccessTokenAsync(clearCache: true);
                    response = await PutWithTokenAsync(token, url);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var statusCode = (int)response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    response.Dispose();
                    var data = JsonConvert.DeserializeObject<NewsDTO>(responseContent);
                    return new NewsCreateResult { Success = true, Data = data, StatusCode = statusCode };
                }
                else
                {
                    response.Dispose();
                    string errorMsg = responseContent;
                    // Nếu là lỗi 400 hoặc 500, cố gắng parse lỗi dạng chuẩn
                    try
                    {
                        var errorObj = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        if (errorObj != null && errorObj.title != null && errorObj.detail != null)
                        {
                            errorMsg = $"{errorObj.title}: {errorObj.detail}";
                        }
                    }
                    catch { }
                    return new NewsCreateResult { Success = false, ErrorMessage = errorMsg, StatusCode = statusCode };
                }
            }
            catch (Exception ex)
            {
                return new NewsCreateResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<bool> DeleteAsync(int newsId, string deleteBy, string language = "en")
        {
            if (newsId <= 0)
                return false;

            var url = language == "en" ? _settings.Value.DeleteEndPointEN : _settings.Value.DeleteEndPointVI;
            if (string.IsNullOrWhiteSpace(url))
                return false;

            var request = new DeleteNewsRequest { NewsId = newsId, DeleteBy = deleteBy };

            try
            {
                async Task<HttpResponseMessage> PostWithTokenAsync(string bearerToken)
                {
                    using var client = _httpClientFactory.CreateClient();
                    if (!string.IsNullOrWhiteSpace(bearerToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                    }

                    var json = JsonConvert.SerializeObject(request);
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    return await client.PostAsync(url, content);
                }

                var token = await GetAccessTokenAsync();
                var response = await PostWithTokenAsync(token);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response.Dispose();
                    token = await GetAccessTokenAsync(clearCache: true);
                    response = await PostWithTokenAsync(token);
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    response.Dispose();
                    return false;
                }

                response.Dispose();

                // Optionally parse response to check result; assume success means deleted
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}