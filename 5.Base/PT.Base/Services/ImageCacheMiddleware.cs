using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PT.Base.Services
{
    public class ImageCacheMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".ico" };

        public ImageCacheMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;

            // Check if it's an image request
            if (IsImageRequest(path))
            {
                var cacheKey = $"image_{path}";

                if (_cache.TryGetValue(cacheKey, out byte[] cachedImage))
                {
                    // Serve from cache
                    context.Response.ContentType = GetContentType(path);
                    context.Response.Headers.Append("X-Cache", "HIT");
                    context.Response.Headers.Append("Cache-Control", "public, max-age=31536000"); // 1 year
                    await context.Response.Body.WriteAsync(cachedImage, 0, cachedImage.Length);
                    return;
                }
            }

            await _next(context);
        }

        private bool IsImageRequest(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            foreach (var ext in ImageExtensions)
            {
                if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private string GetContentType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                ".ico" => "image/x-icon",
                _ => "application/octet-stream"
            };
        }
    }
}
