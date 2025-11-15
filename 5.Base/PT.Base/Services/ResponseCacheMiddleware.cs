using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PT.Base.Services
{
    public class ResponseCacheMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public ResponseCacheMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Chỉ cache GET requests, không cache Admin/Login/Logout
            if (context.Request.Method != "GET" ||
                context.Request.Path.StartsWithSegments("/Admin") ||
                context.Request.Path.StartsWithSegments("/Login") ||
                context.Request.Path.StartsWithSegments("/Logout"))
            {
                await _next(context);
                return;
            }

            // Tạo cache key từ URL
            var cacheKey = $"ResponseCache::{context.Request.Path}{context.Request.QueryString}";

            // Kiểm tra cache
            if (_cache.TryGetValue(cacheKey, out CachedResponse cachedResponse))
            {
                // Trả về từ cache
                context.Response.StatusCode = cachedResponse.StatusCode;
                context.Response.ContentType = cachedResponse.ContentType;

                foreach (var header in cachedResponse.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value;
                }

                await context.Response.WriteAsync(cachedResponse.Body);
                return;
            }

            // Không có cache: capture response
            var originalBodyStream = context.Response.Body;
            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                // Gọi next middleware (controller action)
                await _next(context);

                // Đọc response body
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                // Lưu vào cache (chỉ cache status 200)
                if (context.Response.StatusCode == 200)
                {
                    var cached = new CachedResponse
                    {
                        StatusCode = context.Response.StatusCode,
                        ContentType = context.Response.ContentType,
                        Body = body,
                        Headers = new Dictionary<string, string>()
                    };

                    foreach (var header in context.Response.Headers)
                    {
                        cached.Headers[header.Key] = header.Value.ToString();
                    }

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10),
                        Size = 1,
                    };
                    _cache.Set(cacheKey, cached, cacheOptions);
                }

                // Copy response về stream gốc
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }

    /// <summary>
    /// Model lưu cached response
    /// </summary>
    public class CachedResponse
    {
        public int StatusCode { get; set; }
        public string ContentType { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }
}
