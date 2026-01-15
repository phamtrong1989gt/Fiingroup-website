using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PT.Domain.Model;
using PT.Domain.Model.Common;
using PT.Domain.Model.Misa;

namespace PT.Base.Services
{
    /// <summary>
    /// MISA CRM API Service Implementation
    /// Handles authentication and contact creation with MISA CRM API
    /// </summary>
    public class MisaAPIService : IMisaAPIService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<MisaSettings> _misaSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAPILoggerService _apiLogger;
        
        private const string TOKEN_CACHE_KEY = "MisaAPI_AccessToken";
        private const int DEFAULT_TOKEN_EXPIRY_MINUTES = 30;

        public MisaAPIService(
            IMemoryCache memoryCache,
            IOptions<MisaSettings> misaSettings,
            IHttpClientFactory httpClientFactory,
            IAPILoggerService apiLogger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _misaSettings = misaSettings ?? throw new ArgumentNullException(nameof(misaSettings));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _apiLogger = apiLogger ?? throw new ArgumentNullException(nameof(apiLogger));
        }

        /// <summary>
        /// Get access token from MISA API with caching support
        /// </summary>
        /// <param name="clearCache">True to force refresh token, False to use cached token if available</param>
        /// <returns>Access token string</returns>
        /// <exception cref="Exception">Thrown when token retrieval fails</exception>
        public async Task<string> GetAccessTokenAsync(bool clearCache = false)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var settings = _misaSettings.Value;
            var tokenUrl = settings.APIGetTokenURL;

            try
            {
                // Clear cache if requested
                if (clearCache)
                {
                    _memoryCache.Remove(TOKEN_CACHE_KEY);
                }

                // Return cached token if available
                if (_memoryCache.TryGetValue(TOKEN_CACHE_KEY, out string cachedToken))
                {
                    return cachedToken;
                }

                // Prepare token request
                var tokenRequest = new MisaTokenRequest
                {
                    ClientId = settings.ClientId,
                    ClientSecret = settings.ClientSecret
                };

                var jsonContent = JsonConvert.SerializeObject(tokenRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Call MISA API to get token
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var response = await httpClient.PostAsync(tokenUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Ensure success status
                response.EnsureSuccessStatusCode();

                // Validate response content
                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    throw new Exception("Response content is null or empty");
                }

                // Deserialize response
                var tokenResponse = JsonConvert.DeserializeObject<MisaTokenResponse>(responseContent);

                if (tokenResponse == null || !tokenResponse.Success)
                {
                    throw new Exception($"Token request failed: {tokenResponse?.Code ?? 0}");
                }

                if (string.IsNullOrWhiteSpace(tokenResponse.Data))
                {
                    throw new Exception("Access token is null or empty");
                }

                // Cache the token
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(DEFAULT_TOKEN_EXPIRY_MINUTES),
                    Priority = CacheItemPriority.High,
                    Size = 10
                };

                _memoryCache.Set(TOKEN_CACHE_KEY, tokenResponse.Data, cacheOptions);

                stopwatch.Stop();

                // Log success
                _ = _apiLogger.LogAPICallAsync(new APILogModel
                {
                    Type = LogType.API_GetToken,
                    Endpoint = tokenUrl,
                    Method = "POST",
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    StatusCode = 200,
                    Success = true,
                    RequestParams = $"client_id={settings.ClientId}"
                });

                return tokenResponse.Data;
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();

                // Log HTTP error
                _ = _apiLogger.LogAPICallAsync(new APILogModel
                {
                    Type = LogType.API_Error_Other,
                    Endpoint = tokenUrl,
                    Method = "POST",
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    StatusCode = (int)(ex.StatusCode ?? HttpStatusCode.InternalServerError),
                    Success = false,
                    ErrorMessage = ex.Message
                });

                throw new Exception($"Failed to get access token: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                stopwatch.Stop();

                // Log JSON parsing error
                _ = _apiLogger.LogAPICallAsync(new APILogModel
                {
                    Type = LogType.API_Error_Other,
                    Endpoint = tokenUrl,
                    Method = "POST",
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    StatusCode = 500,
                    Success = false,
                    ErrorMessage = $"JSON Error: {ex.Message}"
                });

                throw new Exception($"Invalid JSON response: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Log general error
                _ = _apiLogger.LogAPICallAsync(new APILogModel
                {
                    Type = LogType.API_Error_Other,
                    Endpoint = tokenUrl,
                    Method = "POST",
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    StatusCode = 500,
                    Success = false,
                    ErrorMessage = ex.Message
                });

                throw new Exception($"Error getting access token: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Create a single contact in MISA CRM
        /// </summary>
        /// <param name="contact">Contact information</param>
        /// <returns>MISA contact creation response</returns>
        /// <exception cref="ArgumentNullException">Thrown when contact is null</exception>
        /// <exception cref="Exception">Thrown when contact creation fails</exception>
        public async Task<MisaContactResponse> CreateContactAsync(MisaContactRequest contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            return await CreateContactsAsync(new[] { contact });
        }

        /// <summary>
        /// Create multiple contacts in MISA CRM with automatic retry on 401 Unauthorized
        /// </summary>
        /// <param name="contacts">Array of contact information</param>
        /// <returns>MISA contact creation response</returns>
        /// <exception cref="ArgumentNullException">Thrown when contacts is null or empty</exception>
        /// <exception cref="Exception">Thrown when contact creation fails</exception>
        public async Task<MisaContactResponse> CreateContactsAsync(MisaContactRequest[] contacts)
        {
            if (contacts == null || contacts.Length == 0)
            {
                throw new ArgumentNullException(nameof(contacts));
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var settings = _misaSettings.Value;
            var createUrl = settings.APICreatedContact;

            try
            {
                // Get access token
                string token;
                try
                {
                    token = await GetAccessTokenAsync(clearCache: false);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to get access token for contact creation: {ex.Message}", ex);
                }

                // Try to create contacts
                try
                {
                    return await CallCreateContactAPIAsync(createUrl, token, contacts, settings.ClientId);
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Log unauthorized and retry with new token
                    _ = _apiLogger.LogAPICallAsync(new APILogModel
                    {
                        Type = LogType.API_Error_Unauthorized,
                        Endpoint = createUrl,
                        Method = "POST",
                        StatusCode = 401,
                        Success = false,
                        ErrorMessage = "Token expired, retrying with new token"
                    });

                    try
                    {
                        // Get fresh token and retry
                        token = await GetAccessTokenAsync(clearCache: true);
                        return await CallCreateContactAPIAsync(createUrl, token, contacts, settings.ClientId);
                    }
                    catch (Exception retryEx)
                    {
                        throw new Exception($"Retry failed after token refresh: {retryEx.Message}", retryEx);
                    }
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Log final error
                _ = _apiLogger.LogAPICallAsync(new APILogModel
                {
                    Type = LogType.API_Error_Other,
                    Endpoint = createUrl,
                    Method = "POST",
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    StatusCode = 500,
                    Success = false,
                    ErrorMessage = ex.Message
                });

                throw;
            }
        }

        /// <summary>
        /// Internal method to call MISA Create Contact API
        /// </summary>
        /// <param name="url">API endpoint URL</param>
        /// <param name="token">Bearer token</param>
        /// <param name="contacts">Array of contacts to create</param>
        /// <param name="clientId">MISA Client ID</param>
        /// <returns>MISA contact creation response</returns>
        private async Task<MisaContactResponse> CallCreateContactAPIAsync(
            string url,
            string token,
            MisaContactRequest[] contacts,
            string clientId)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(60);

                // Set headers
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpClient.DefaultRequestHeaders.Add("Clientid", clientId);

                // Serialize contacts to JSON
                var jsonContent = JsonConvert.SerializeObject(contacts);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Send POST request
                var response = await httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Check for unauthorized
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);
                }

                // Ensure success status
                response.EnsureSuccessStatusCode();

                // Validate response content
                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    throw new Exception("Response content is null or empty");
                }

                // Deserialize response
                var contactResponse = JsonConvert.DeserializeObject<MisaContactResponse>(responseContent);

                if (contactResponse == null)
                {
                    throw new Exception("Failed to deserialize contact response");
                }

                stopwatch.Stop();

                // Log success
                _ = _apiLogger.LogAPICallAsync(new APILogModel
                {
                    Type = LogType.Create,
                    Endpoint = url,
                    Method = "POST",
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    StatusCode = 200,
                    Success = contactResponse.Success,
                    RequestParams = $"contacts_count={contacts.Length}, client_id={clientId}"
                });

                return contactResponse;
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();

                // Log HTTP error
                _ = _apiLogger.LogAPICallAsync(new APILogModel
                {
                    Type = LogType.API_Error_Other,
                    Endpoint = url,
                    Method = "POST",
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    StatusCode = (int)(ex.StatusCode ?? HttpStatusCode.InternalServerError),
                    Success = false,
                    ErrorMessage = ex.Message
                });

                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Log general error
                _ = _apiLogger.LogAPICallAsync(new APILogModel
                {
                    Type = LogType.API_Error_Other,
                    Endpoint = url,
                    Method = "POST",
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    StatusCode = 500,
                    Success = false,
                    ErrorMessage = ex.Message
                });

                throw;
            }
        }
    }
}
