using Microsoft.Extensions.Logging;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PT.Base.Services
{
    /// <summary>
    /// Service để ghi log các API calls với thông tin hiệu năng và lỗi
    /// </summary>
    public interface IAPILoggerService
    {
        Task LogAPICallAsync(APILogModel model);
        Task<T> TrackAPICallAsync<T>(LogType logType, string endpoint, string method, Func<Task<T>> apiCall, string language = null, string requestParams = null, int portalId = 1);
    }

    public class APILoggerService : IAPILoggerService
    {
        private readonly ILogRepository _logRepository;
        private readonly ILogger<APILoggerService> _logger;

        public APILoggerService(ILogRepository logRepository, ILogger<APILoggerService> logger)
        {
            _logRepository = logRepository;
            _logger = logger;
        }

        /// <summary>
        /// Ghi log một API call với đầy đủ thông tin
        /// ⚠️ KHÔNG AWAIT hàm này trong request chính để tránh concurrent DbContext access
        /// </summary>
        public async Task LogAPICallAsync(APILogModel model)
        {
            try
            {
                _logger.LogDebug("Logging API Call: {LogName}", BuildLogName(model));

                var log = new Log
                {
                    Type = model.Type,
                    Name = BuildLogName(model),
                    Object = "NewsAPIService",
                    ObjectType = model.PortalId.ToString(),
                    ActionTime = DateTime.Now,
                    AcctionUser = "System",
                    ObjectId = model.StatusCode,
                };

                await _logRepository.AddAsync(log);
                await _logRepository.CommitAsync();
            }
            catch (Exception)
            {
                // Không throw exception để tránh ảnh hưởng đến logic chính
            }
        }

        /// <summary>
        /// Track một API call, tự động đo thời gian và ghi log
        /// </summary>
        public async Task<T> TrackAPICallAsync<T>(
            LogType logType,
            string endpoint,
            string method,
            Func<Task<T>> apiCall,
            string language = null,
            string requestParams = null,
            int portalId = 1)
        {
            var stopwatch = Stopwatch.StartNew();
            T result = default;
            bool success = false;
            int statusCode = 0;
            string errorMessage = null;

            try
            {
                result = await apiCall();
                success = result != null;
                statusCode = success ? 200 : 204;
            }
            catch (Exception ex)
            {
                success = false;
                statusCode = 500;
                errorMessage = ex.Message;
                throw;
            }
            finally
            {
                stopwatch.Stop();

                var logModel = new APILogModel
                {
                    Type = logType,
                    Endpoint = endpoint,
                    Method = method,
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    StatusCode = statusCode,
                    Success = success,
                    ErrorMessage = errorMessage,
                    Language = language,
                    RequestParams = requestParams,
                    PortalId = portalId
                };

                await LogAPICallAsync(logModel);
            }

            return result;
        }

        private string BuildLogName(APILogModel model)
        {
            var status = model.Success ? "✅" : "❌";
            var portal = model.PortalId > 0 ? $"[P{model.PortalId}]" : "";
            var language = string.IsNullOrEmpty(model.Language) ? "" : $"[{model.Language.ToUpper()}]";
            var duration = $"{model.DurationMs}ms";
            var endpoint = TruncateEndpoint(model.Endpoint, 50);

            if (model.Success)
            {
                return $"{status} {portal}{language} {model.Method} {endpoint} - {duration}";
            }
            else
            {
                var error = string.IsNullOrEmpty(model.ErrorMessage) ? $"HTTP {model.StatusCode}" : model.ErrorMessage;
                return $"{status} {portal}{language} {model.Method} {endpoint} - {duration} - {TruncateString(error, 100)}";
            }
        }

        private string TruncateEndpoint(string url, int maxLength)
        {
            if (string.IsNullOrEmpty(url) || url.Length <= maxLength)
                return url;

            return "..." + url.Substring(url.Length - maxLength);
        }

        private string TruncateString(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }
    }
}
