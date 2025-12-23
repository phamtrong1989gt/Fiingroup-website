using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        Task<NewsCreateResult> CreateAsync(ContentPage contentPage, List<int> tags, List<int> categories, string updateBy);
        Task<NewsCreateResult> UpdateAsync(ContentPage contentPage, List<int> tags, List<int> categories, string updateBy);
        Task<NewsCreateResult> DeleteAsync(int newsId, string deleteBy, string language = "en");
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
        private readonly ITagRepository _iTagRepository;
        private readonly ILogger _logger;

        public AsyncNewsService(
            ISeoSettingRepository iSeoSettingRepository,
            ILogger<AsyncNewsService> logger,
            IMemoryCache memoryCache,
            IBindContentSettingRepository iBindContentSettingRepository,
            IEmailSettingRepository iEmailSettingRepository,
            IOptions<AsyncNewsSettings> settings,
            IHttpClientFactory httpClientFactory, IPortalRepository iPortalRepository, IWebHostEnvironment env, ICategoryRepository iCategoryRepository, ILinkRepository iLinkRepository, ITagRepository iTagRepository)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _settings = settings;
            _httpClientFactory = httpClientFactory;
            _iPortalRepository = iPortalRepository;
            _iCategoryRepository = iCategoryRepository;
            _ilinkRepository = iLinkRepository;
            _env = env;
            _iTagRepository = iTagRepository;
        }
  
        public async Task<string> GetAccessTokenAsync(bool clearCache = false)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("[GetAccessTokenAsync] Start - ClearCache: {ClearCache}", clearCache);

            if (clearCache)
            {
                _memoryCache.Remove(TOKEN_CACHE_KEY);
                _logger.LogDebug("[GetAccessTokenAsync] Cache cleared");
            }

            if (_memoryCache.TryGetValue(TOKEN_CACHE_KEY, out string cachedToken))
            {
                stopwatch.Stop();
                _logger.LogDebug("[GetAccessTokenAsync] Token retrieved from cache - Duration: {Duration}ms", stopwatch.ElapsedMilliseconds);
                return cachedToken;
            }

            var tokenUrl = $"{_settings.Value.TokenEndpoint}";
            _logger.LogDebug("[GetAccessTokenAsync] Requesting new token from: {TokenUrl}", tokenUrl);

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

                var apiCallStopwatch = Stopwatch.StartNew();
                var content = new FormUrlEncodedContent(formData);
                var response = await httpClient.PostAsync(tokenUrl, content);
                apiCallStopwatch.Stop();
                
                _logger.LogDebug("[GetAccessTokenAsync] API call completed - Status: {StatusCode}, Duration: {Duration}ms", 
                    response.StatusCode, apiCallStopwatch.ElapsedMilliseconds);

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                if (string.IsNullOrEmpty(tokenResponse?.AccessToken))
                {
                    _logger.LogError("[GetAccessTokenAsync] Access token is null or empty");
                    throw new Exception("Access token is null or empty");
                }

                var cacheExpiration = TimeSpan.FromSeconds(tokenResponse.ExpiresIn > 60 ? tokenResponse.ExpiresIn - 60 : tokenResponse.ExpiresIn);

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheExpiration,
                    Priority = CacheItemPriority.High
                };

                _memoryCache.Set(TOKEN_CACHE_KEY, tokenResponse.AccessToken, cacheOptions);
                
                stopwatch.Stop();
                _logger.LogDebug("[GetAccessTokenAsync] Token cached successfully - Total Duration: {Duration}ms, Expires In: {ExpiresIn}s", 
                    stopwatch.ElapsedMilliseconds, tokenResponse.ExpiresIn);

                return tokenResponse.AccessToken;
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "[GetAccessTokenAsync] Failed to get access token - Duration: {Duration}ms", stopwatch.ElapsedMilliseconds);
                throw new Exception($"Failed to get access token: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "[GetAccessTokenAsync] Error getting access token - Duration: {Duration}ms", stopwatch.ElapsedMilliseconds);
                throw new Exception($"Error getting access token: {ex.Message}", ex);
            }
        }

        public async Task<NewsCreateResult> CreateAsync(ContentPage contentPage, List<int> tags, List<int> categories, string createBy)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("[CreateAsync] Start - ContentPageId: {ContentPageId}, Language: {Language}, PortalId: {PortalId}", 
                contentPage?.Id, contentPage?.Language, contentPage?.PortalId);

            var url = $"{_settings.Value.CreatedEndPointVI}";
            if(contentPage.Language == "en")
            {
                url = $"{_settings.Value.CreatedEndPointEN}";
            }
            
            _logger.LogDebug("[CreateAsync] API Endpoint: {Url}", url);

            if (string.IsNullOrWhiteSpace(url) || contentPage == null)
            {
                _logger.LogWarning("[CreateAsync] Invalid parameters - Url IsEmpty: {UrlEmpty}, ContentPage IsNull: {IsNull}", 
                    string.IsNullOrWhiteSpace(url), contentPage == null);
                return new NewsCreateResult { Success = false, ErrorMessage = "Invalid url or contentPage is null" };
            }

            var portal = await _iPortalRepository.SingleOrDefaultAsync(true, x=> x.Id == contentPage.PortalId);
            if(portal == null)
            {
                _logger.LogWarning("[CreateAsync] Portal not found - PortalId: {PortalId}", contentPage.PortalId);
                return new NewsCreateResult { Success = false, ErrorMessage = "Portal not found" };
            }

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
            
            _logger.LogDebug("[CreateAsync] Domain: {Domain}", domain);

            try
            {
                int sourceId = 5; // Default source ID
                if(contentPage.PortalId == 1)
                {
                    sourceId = 299;
                }

                // Ánh xạ danh mục
                var categoryConvertStopwatch = Stopwatch.StartNew();
                var categoryIds = await ConvertCategories(categories, contentPage.CategoryId);
                categoryConvertStopwatch.Stop();
                _logger.LogDebug("[CreateAsync] Categories converted - Count: {Count}, Duration: {Duration}ms", 
                    categoryIds.Count, categoryConvertStopwatch.ElapsedMilliseconds);

                var tagsConvertStopwatch = Stopwatch.StartNew();
                var convertTags = await ConvertTags(tags);
                tagsConvertStopwatch.Stop();
                _logger.LogDebug("[CreateAsync] Tags converted - Count: {Count}, Duration: {Duration}ms", 
                    convertTags.Count, tagsConvertStopwatch.ElapsedMilliseconds);

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
                    CreateBy = createBy,
                    StatusId = contentPage.Status ? 1 : 4,
                    Categories = categoryIds,
                    TypeIds = [],
                    SourceIds = [sourceId],
                    Entities = [],
                    Tags = convertTags,
                    ICBs = [],
                    VSICs = [],

                };

                _logger.LogDebug("[CreateAsync] Request payload prepared - Title: {Title}, StatusId: {StatusId}, SourceId: {SourceId}", 
                    cmd.Title, cmd.StatusId, sourceId);

                async Task<HttpResponseMessage> PostWithTokenAsync(string bearerToken)
                {
                    using var client = _httpClientFactory.CreateClient();
                    if (!string.IsNullOrWhiteSpace(bearerToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                    }

                    var json = JsonConvert.SerializeObject(cmd);
                    _logger.LogDebug("[CreateAsync] Request JSON length: {Length} characters", json.Length);
                    
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    var apiStopwatch = Stopwatch.StartNew();
                    var result = await client.PostAsync(url, content);
                    apiStopwatch.Stop();
                    
                    _logger.LogDebug("[CreateAsync] API call completed - Status: {StatusCode}, Duration: {Duration}ms", 
                        result.StatusCode, apiStopwatch.ElapsedMilliseconds);
                    
                    return result;
                }

                var tokenStopwatch = Stopwatch.StartNew();
                var token = await GetAccessTokenAsync();
                tokenStopwatch.Stop();
                _logger.LogDebug("[CreateAsync] Token retrieved - Duration: {Duration}ms", tokenStopwatch.ElapsedMilliseconds);

                var response = await PostWithTokenAsync(token);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("[CreateAsync] Unauthorized response, retrying with new token");
                    response.Dispose();
                    
                    var retryTokenStopwatch = Stopwatch.StartNew();
                    token = await GetAccessTokenAsync(clearCache: true);
                    retryTokenStopwatch.Stop();
                    _logger.LogDebug("[CreateAsync] New token retrieved - Duration: {Duration}ms", retryTokenStopwatch.ElapsedMilliseconds);
                    
                    response = await PostWithTokenAsync(token);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var statusCode = (int)response.StatusCode;
                
                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    response.Dispose();
                    var data = JsonConvert.DeserializeObject<NewsDTO>(responseContent);
                    _logger.LogDebug("[CreateAsync] Success - NewsId: {NewsId}, Total Duration: {Duration}ms", 
                        data.NewsId, stopwatch.ElapsedMilliseconds);
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
                    
                    _logger.LogError("[CreateAsync] Failed - StatusCode: {StatusCode}, Error: {Error}, Total Duration: {Duration}ms", 
                        statusCode, errorMsg, stopwatch.ElapsedMilliseconds);
                    return new NewsCreateResult { Success = false, ErrorMessage = errorMsg, StatusCode = statusCode };
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "[CreateAsync] Exception - Total Duration: {Duration}ms", stopwatch.ElapsedMilliseconds);
                return new NewsCreateResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        private async Task<List<string>> ConvertTags(List<int> tags)
        {
            var tagObjects = new List<string>();
            if(tags == null || tags.Count == 0)
                return tagObjects;

            var lists = await _iTagRepository.SearchAsync(true,0, 0,x => tags.Contains(x.Id));

            foreach (var tagId in tags)
            {
                var tag = lists.FirstOrDefault(x=>x.Id == tagId);
                if (tag != null)
                {
                    tagObjects.Add(tag.Name);
                }
            }
            return tagObjects;
        }
    

        private async Task<List<RefItem>> ConvertCategories(List<int> categories, int mainCategoryId)
        {
            var categoryObjects = new List<RefItem>();
            if (categories == null || categories.Count == 0)
                return categoryObjects;

            // Lấy tất cả categories trong danh sách
            var lists = await _iCategoryRepository.SearchAsync(true, 0, 0, x => categories.Contains(x.Id));

            // Lấy tất cả category IDs để kiểm tra xem category nào là parent
            var allCategoryIds = lists.Select(x => x.Id).ToList();

            // Lấy các category có ParentId trong danh sách categories (tức là có cha trong danh sách)
            var childCategoryIds = lists.Where(x => allCategoryIds.Contains(x.ParentId)).Select(x => x.ParentId).Distinct().ToList();

            // Chỉ lấy những category KHÔNG phải là parent (leaf nodes - level thấp nhất)
            var leafCategories = lists.Where(x => !childCategoryIds.Contains(x.Id)).ToList();

            foreach (var category in leafCategories)
            {
                if (category.ReferentCategoryId.HasValue && category.ReferentCategoryId.Value > 0)
                {
                    categoryObjects.Add(new RefItem
                    {
                        Id = category.ReferentCategoryId.Value,
                        PriorityOrder = 0 // Tạm thời set = 0, sẽ đánh lại sau
                    });
                }
            }

            // Kiểm tra mainCategoryId
            if (mainCategoryId > 0)
            {
                // Lấy thông tin mainCategory từ database
                var mainCategory = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Id == mainCategoryId);

                if (mainCategory?.ReferentCategoryId.HasValue == true && mainCategory.ReferentCategoryId.Value > 0)
                {
                    // Kiểm tra xem ReferentCategoryId của mainCategory đã tồn tại trong list chưa
                    var existingCategory = categoryObjects.FirstOrDefault(x => x.Id == mainCategory.ReferentCategoryId.Value);

                    if (existingCategory == null)
                    {
                        // Nếu chưa tồn tại thì thêm vào đầu list
                        categoryObjects.Insert(0, new RefItem
                        {
                            Id = mainCategory.ReferentCategoryId.Value,
                            PriorityOrder = 0 // Tạm thời set = 0
                        });
                    }
                    else
                    {
                        // Nếu đã tồn tại, di chuyển nó lên đầu list
                        categoryObjects.Remove(existingCategory);
                        categoryObjects.Insert(0, existingCategory);
                    }
                }
            }

            // Đánh lại PriorityOrder từ 1 đến n
            for (int i = 0; i < categoryObjects.Count; i++)
            {
                categoryObjects[i].PriorityOrder = i + 1;
            }

            return categoryObjects;
        }

        public async Task<NewsCreateResult> UpdateAsync(ContentPage contentPage, List<int> tags, List<int> categories, string updateBy)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("[UpdateAsync] Start - ContentPageId: {ContentPageId}, NewsId: {NewsId}, Language: {Language}", 
                contentPage?.Id, contentPage?.NewsId, contentPage?.Language);

            if (contentPage == null)
            {
                _logger.LogWarning("[UpdateAsync] ContentPage is null");
                return new NewsCreateResult { Success = false, ErrorMessage = "ContentPage is null" };
            }
            
            var url = _settings.Value.EditEndPointVI;
            if (contentPage.Language == "en")
            {
                url = _settings.Value.EditEndPointEN;
            }
            
            _logger.LogDebug("[UpdateAsync] API Endpoint: {Url}", url);

            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogWarning("[UpdateAsync] Update endpoint URL is not configured");
                return new NewsCreateResult { Success = false, ErrorMessage = "Update endpoint URL is not configured" };
            }

            var portal = await _iPortalRepository.SingleOrDefaultAsync(true, x => x.Id == contentPage.PortalId);
            if (portal == null)
            {
                _logger.LogWarning("[UpdateAsync] Portal not found - PortalId: {PortalId}", contentPage.PortalId);
                return new NewsCreateResult { Success = false, ErrorMessage = "Portal not found" };
            }

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
            
            _logger.LogDebug("[UpdateAsync] Domain: {Domain}", domain);

            try
            {
                // Theo portal
                int sourceId = contentPage.PortalId == 1 ? 299 : 5;

                var link = await _ilinkRepository.SingleOrDefaultAsync(true, x => x.ObjectId == contentPage.Id && x.Type == contentPage.SlugType);
                if (link == null)
                {
                    _logger.LogWarning("[UpdateAsync] Link not found - ContentPageId: {ContentPageId}", contentPage.Id);
                    return new NewsCreateResult { Success = false, ErrorMessage = "Link not found for content page" };
                }

                var categoryConvertStopwatch = Stopwatch.StartNew();
                var categoryIds = await ConvertCategories(categories, contentPage.CategoryId);
                categoryConvertStopwatch.Stop();
                _logger.LogDebug("[UpdateAsync] Categories converted - Count: {Count}, Duration: {Duration}ms", 
                    categoryIds.Count, categoryConvertStopwatch.ElapsedMilliseconds);

                var tagsConvertStopwatch = Stopwatch.StartNew();
                var convertTags = await ConvertTags(tags);
                tagsConvertStopwatch.Stop();
                _logger.LogDebug("[UpdateAsync] Tags converted - Count: {Count}, Duration: {Duration}ms", 
                    convertTags.Count, tagsConvertStopwatch.ElapsedMilliseconds);

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
                    Categories = categoryIds,
                    TypeIds = [],
                    SourceIds = [sourceId],
                    Entities = [],
                    Tags = convertTags,
                    ICBs = [],
                    VSICs = [],
                    NewsId = contentPage.NewsId.HasValue ? contentPage.NewsId.Value : 0,
                    StatusId = contentPage.Status ? 1 : 4,
                };

                _logger.LogDebug("[UpdateAsync] Request payload prepared - Title: {Title}, NewsId: {NewsId}, StatusId: {StatusId}", 
                    cmd.Title, cmd.NewsId, cmd.StatusId);

                async Task<HttpResponseMessage> PutWithTokenAsync(string bearerToken, string endpoint)
                {
                    using var client = _httpClientFactory.CreateClient();
                    if (!string.IsNullOrWhiteSpace(bearerToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                    }

                    var json = JsonConvert.SerializeObject(cmd);
                    _logger.LogDebug("[UpdateAsync] Request JSON length: {Length} characters", json.Length);
                    
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    var apiStopwatch = Stopwatch.StartNew();
                    var result = await client.PutAsync(endpoint, content);
                    apiStopwatch.Stop();
                    
                    _logger.LogDebug("[UpdateAsync] API call completed - Status: {StatusCode}, Duration: {Duration}ms", 
                        result.StatusCode, apiStopwatch.ElapsedMilliseconds);
                    
                    return result;
                }

                var tokenStopwatch = Stopwatch.StartNew();
                var token = await GetAccessTokenAsync();
                tokenStopwatch.Stop();
                _logger.LogDebug("[UpdateAsync] Token retrieved - Duration: {Duration}ms", tokenStopwatch.ElapsedMilliseconds);

                var response = await PutWithTokenAsync(token, url);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("[UpdateAsync] Unauthorized response, retrying with new token");
                    response.Dispose();
                    
                    var retryTokenStopwatch = Stopwatch.StartNew();
                    token = await GetAccessTokenAsync(clearCache: true);
                    retryTokenStopwatch.Stop();
                    _logger.LogDebug("[UpdateAsync] New token retrieved - Duration: {Duration}ms", retryTokenStopwatch.ElapsedMilliseconds);
                    
                    response = await PutWithTokenAsync(token, url);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var statusCode = (int)response.StatusCode;

                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    response.Dispose();
                    var data = JsonConvert.DeserializeObject<NewsDTO>(responseContent);
                    _logger.LogDebug("[UpdateAsync] Success - NewsId: {NewsId}, Total Duration: {Duration}ms", 
                        data?.NewsId, stopwatch.ElapsedMilliseconds);
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
                    
                    _logger.LogError("[UpdateAsync] Failed - StatusCode: {StatusCode}, Error: {Error}, Total Duration: {Duration}ms", 
                        statusCode, errorMsg, stopwatch.ElapsedMilliseconds);
                    return new NewsCreateResult { Success = false, ErrorMessage = errorMsg, StatusCode = statusCode };
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "[UpdateAsync] Exception - Total Duration: {Duration}ms", stopwatch.ElapsedMilliseconds);
                return new NewsCreateResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<NewsCreateResult> DeleteAsync(int newsId, string deleteBy, string language = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("[DeleteAsync] Start - NewsId: {NewsId}, DeleteBy: {DeleteBy}, Language: {Language}", 
                newsId, deleteBy, language);

            if (newsId <= 0)
            {
                _logger.LogWarning("[DeleteAsync] Invalid newsId: {NewsId}", newsId);
                return new NewsCreateResult { Success = false, ErrorMessage = "Invalid newsId" };
            }

            var url = language == "en" ? _settings.Value.DeleteEndPointEN : _settings.Value.DeleteEndPointVI;
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogWarning("[DeleteAsync] Delete endpoint URL is not configured for language: {Language}", language);
                return new NewsCreateResult { Success = false, ErrorMessage = "Delete endpoint URL is not configured" };
            }

            _logger.LogDebug("[DeleteAsync] API Endpoint: {Url}", url);

            var request = new DeleteNewsRequest { NewsId = newsId, DeleteBy = deleteBy };

            try
            {
                async Task<HttpResponseMessage> DeleteWithTokenAsync(string bearerToken)
                {
                    using var client = _httpClientFactory.CreateClient();
                    if (!string.IsNullOrWhiteSpace(bearerToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                    }
                    
                    var deleteUrl = $"{url}";
                    _logger.LogDebug("[DeleteAsync] Delete URL: {DeleteUrl}", deleteUrl);
                    
                    var apiStopwatch = Stopwatch.StartNew();
                    using var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    var result = await client.PostAsync(deleteUrl, content);
                    apiStopwatch.Stop();
                    
                    _logger.LogDebug("[DeleteAsync] API call completed - Status: {StatusCode}, Duration: {Duration}ms", 
                        result.StatusCode, apiStopwatch.ElapsedMilliseconds);
                    
                    return result;
                }

                var tokenStopwatch = Stopwatch.StartNew();
                var token = await GetAccessTokenAsync();
                tokenStopwatch.Stop();
                _logger.LogDebug("[DeleteAsync] Token retrieved - Duration: {Duration}ms", tokenStopwatch.ElapsedMilliseconds);

                var response = await DeleteWithTokenAsync(token);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("[DeleteAsync] Unauthorized response, retrying with new token");
                    response.Dispose();
                    
                    var retryTokenStopwatch = Stopwatch.StartNew();
                    token = await GetAccessTokenAsync(clearCache: true);
                    retryTokenStopwatch.Stop();
                    _logger.LogDebug("[DeleteAsync] New token retrieved - Duration: {Duration}ms", retryTokenStopwatch.ElapsedMilliseconds);
                    
                    response = await DeleteWithTokenAsync(token);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var statusCode = (int)response.StatusCode;

                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    response.Dispose();
                    var data = JsonConvert.DeserializeObject<NewsDTO>(responseContent);
                    _logger.LogDebug("[DeleteAsync] Success - NewsId: {NewsId}, Total Duration: {Duration}ms", 
                        newsId, stopwatch.ElapsedMilliseconds);
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
                    
                    _logger.LogError("[DeleteAsync] Failed - StatusCode: {StatusCode}, Error: {Error}, Total Duration: {Duration}ms", 
                        statusCode, errorMsg, stopwatch.ElapsedMilliseconds);
                    return new NewsCreateResult { Success = false, ErrorMessage = errorMsg, StatusCode = statusCode };
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "[DeleteAsync] Exception - Total Duration: {Duration}ms", stopwatch.ElapsedMilliseconds);
                return new NewsCreateResult { Success = false, ErrorMessage = ex.Message };
            }
        }
    }
}