using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System;
using PT.Base.Services;

namespace PT.UI.Helpers
{
    /// <summary>
    /// HTML Helper to generate auto CSS link tags
    /// </summary>
    public static class AutoCssHelper
    {
        /// <summary>
        /// Generate CSS link tag for current view
        /// Usage in View: @Html.AutoCss(linkId)
        /// </summary>
        public static IHtmlContent AutoCss(this IHtmlHelper htmlHelper, int linkId)
        {
            System.Diagnostics.Debug.WriteLine($"[AutoCssHelper] Called with linkId={linkId}");
            
            // Get language from culture helper or default to "vi"
            var language = PT.Base.CultureHelper.GetCurrentCulture?.Id ?? "vi";
            System.Diagnostics.Debug.WriteLine($"[AutoCssHelper] Language={language}");

            // Get AutoCssService from DI
            var autoCssService = htmlHelper.ViewContext.HttpContext.RequestServices
                .GetService(typeof(IAutoCssService)) as IAutoCssService;

            if (autoCssService == null)
            {
                System.Diagnostics.Debug.WriteLine($"[AutoCssHelper] ERROR: AutoCssService is NULL! Service not registered in DI?");
                // Return comment instead of empty to see in HTML
                return new HtmlString($"<!-- AutoCss ERROR: Service not registered for linkId={linkId} -->");
            }

            // ⭐ EAGER CREATE: Tạo file ngay lập tức (sync call)
            try
            {
                System.Diagnostics.Debug.WriteLine($"[AutoCssHelper] Creating CSS file for linkId={linkId}, language={language}");
                // Call async method and wait
                var filePathTask = autoCssService.GetCssFilePathAsync(linkId, language);
                filePathTask.Wait(); // Block until file is created
                System.Diagnostics.Debug.WriteLine($"[AutoCssHelper] File created successfully");
            }
            catch (Exception ex)
            {
                // Log error but don't break page render
                System.Diagnostics.Debug.WriteLine($"[AutoCssHelper] ERROR creating CSS file: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[AutoCssHelper] Stack trace: {ex.StackTrace}");
                // Return comment with error info
                return new HtmlString($"<!-- AutoCss ERROR: {ex.Message} for linkId={linkId} -->");
            }

            // ⭐ Generate dynamic route URL: /AutoCss/View/{linkId}/{language}
            var cssUrl = $"/AutoCss/View/{linkId}/{language}";
            System.Diagnostics.Debug.WriteLine($"[AutoCssHelper] Generated dynamic CSS URL: {cssUrl}");
            
            // Generate link tag
            var linkTag = new StringBuilder();
            linkTag.AppendLine("<!-- Auto-generated CSS -->");
            linkTag.Append($"<link rel=\"stylesheet\" href=\"{cssUrl}\" data-auto-css=\"link_{linkId}_{language}\" />");

            var result = linkTag.ToString();
            System.Diagnostics.Debug.WriteLine($"[AutoCssHelper] Generated HTML: {result}");
            return new HtmlString(result);
        }

        /// <summary>
        /// Check if auto-generated CSS file exists
        /// </summary>
        /// <param name="webHostEnvironment">IWebHostEnvironment instance</param>
        /// <param name="linkId">Link ID</param>
        /// <param name="language">Language code (vi, en, etc.)</param>
        /// <returns>True if file exists, false otherwise</returns>
        public static bool AutoCssExists(IWebHostEnvironment webHostEnvironment, int linkId, string language)
        {
            if (linkId <= 0 || string.IsNullOrWhiteSpace(language))
            {
                return false;
            }

            try
            {
                var webRoot = webHostEnvironment.WebRootPath;
                if (string.IsNullOrEmpty(webRoot))
                {
                    return false;
                }

                var filePath = Path.Combine(webRoot, "auto-css", $"link_{linkId}_{language}.css");
                return File.Exists(filePath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ensure auto-generated CSS file exists (create if not exists)
        /// </summary>
        /// <param name="webHostEnvironment">IWebHostEnvironment instance</param>
        /// <param name="linkId">Link ID</param>
        /// <param name="language">Language code</param>
        /// <returns>True if file exists or was created successfully</returns>
        public static async Task<bool> EnsureAutoCssFileAsync(IWebHostEnvironment webHostEnvironment, int linkId, string language)
        {
            if (linkId <= 0 || string.IsNullOrWhiteSpace(language))
            {
                return false;
            }

            try
            {
                var webRoot = webHostEnvironment.WebRootPath;
                if (string.IsNullOrEmpty(webRoot))
                {
                    return false;
                }

                var autoCssDir = Path.Combine(webRoot, "auto-css");
                var filePath = Path.Combine(autoCssDir, $"link_{linkId}_{language}.css");

                // If file already exists, return true
                if (File.Exists(filePath))
                {
                    return true;
                }

                // Create directory if not exists
                if (!Directory.Exists(autoCssDir))
                {
                    Directory.CreateDirectory(autoCssDir);
                }

                // Create template CSS file
                var cssTemplate = GenerateCssTemplate(linkId, language);
                await File.WriteAllTextAsync(filePath, cssTemplate, Encoding.UTF8);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get auto-generated CSS file path (for asp-append-version)
        /// </summary>
        /// <param name="linkId">Link ID</param>
        /// <param name="language">Language code</param>
        /// <returns>CSS file path relative to wwwroot</returns>
        public static string GetAutoCssPath(int linkId, string language)
        {
            if (linkId <= 0 || string.IsNullOrWhiteSpace(language))
            {
                return string.Empty;
            }

            return $"/auto-css/link_{linkId}_{language}.css";
        }

        /// <summary>
        /// Generate CSS template for new file
        /// </summary>
        private static string GenerateCssTemplate(int linkId, string language)
        {
            var now = System.DateTime.Now;
            return "";
        }
    }
}
