using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PT.Base.Services
{
    /// <summary>
    /// Service to manage auto-generated CSS files per view/action
    /// Pattern: {controller}_{action}_{language}.css
    /// </summary>
    public interface IAutoCssService
    {
        /// <summary>
        /// Get CSS file path for a specific link ID
        /// Creates empty file if not exists
        /// </summary>
        Task<string> GetCssFilePathAsync(int linkId, string language);

        /// <summary>
        /// Get CSS content for a specific link ID
        /// Returns empty string if file doesn't exist
        /// </summary>
        Task<string> GetCssContentAsync(int linkId, string language);

        /// <summary>
        /// Check if CSS file exists
        /// </summary>
        bool CssFileExists(int linkId, string language);

        /// <summary>
        /// Generate CSS file URL for view
        /// </summary>
        string GenerateCssUrl(int linkId, string language);
    }

    public class AutoCssService : IAutoCssService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const string AUTO_CSS_FOLDER = "auto-css";

        public AutoCssService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            
            // Debug: Log WebRootPath ?? verify
            System.Diagnostics.Debug.WriteLine($"[AutoCssService] WebRootPath: {_webHostEnvironment.WebRootPath}");
        }

        /// <summary>
        /// Get sanitized file name: link_{linkId}_{language}.css
        /// </summary>
        private string GetFileName(int linkId, string language)
        {
            language = SanitizeFileName(language);

            return $"link_{linkId}_{language}.css".ToLowerInvariant();
        }

        /// <summary>
        /// Sanitize file name to prevent security issues
        /// </summary>
        private string SanitizeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "default";

            // Remove invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                input = input.Replace(c.ToString(), "");
            }

            // Remove dots and slashes to prevent directory traversal
            input = input.Replace(".", "").Replace("/", "").Replace("\\", "");

            return input;
        }

        /// <summary>
        /// Get full directory path for auto-css folder
        /// </summary>
        private string GetAutoCssDirectory()
        {
            return Path.Combine(_webHostEnvironment.WebRootPath, AUTO_CSS_FOLDER);
        }

        /// <summary>
        /// Ensure auto-css directory exists
        /// </summary>
        private void EnsureDirectoryExists()
        {
            var directory = GetAutoCssDirectory();
            
            System.Diagnostics.Debug.WriteLine($"[AutoCssService] Checking directory: {directory}");
            
            if (!Directory.Exists(directory))
            {
                System.Diagnostics.Debug.WriteLine($"[AutoCssService] Creating directory: {directory}");
                Directory.CreateDirectory(directory);
                System.Diagnostics.Debug.WriteLine($"[AutoCssService] Directory created successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[AutoCssService] Directory already exists");
            }
        }

        public bool CssFileExists(int linkId, string language)
        {
            // Note: LinkId is used to locate the file, controller/action info is not needed
            // But keeping parameters for compatibility
            var fileName = GetFileName(linkId, language);
            var directory = GetAutoCssDirectory();
            var filePath = Path.Combine(directory, fileName);
            return File.Exists(filePath);
        }

        public async Task<string> GetCssFilePathAsync(int linkId, string language)
        {
            System.Diagnostics.Debug.WriteLine($"[AutoCssService] GetCssFilePathAsync called: linkId={linkId}, language={language}");
            
            EnsureDirectoryExists();

            var fileName = GetFileName(linkId, language);
            var directory = GetAutoCssDirectory();
            var filePath = Path.Combine(directory, fileName);

            System.Diagnostics.Debug.WriteLine($"[AutoCssService] Target file path: {filePath}");
            
            // Create empty file if not exists
            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"[AutoCssService] File does not exist, creating: {filePath}");
                
                try
                {
                    await File.WriteAllTextAsync(filePath, GetDefaultCssContent(linkId, language));
                    System.Diagnostics.Debug.WriteLine($"[AutoCssService] File created successfully: {filePath}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AutoCssService] ERROR creating file: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[AutoCssService] Stack trace: {ex.StackTrace}");
                    throw;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[AutoCssService] File already exists: {filePath}");
            }

            return filePath;
        }

        public async Task<string> GetCssContentAsync(int linkId, string language)
        {
            var filePath = await GetCssFilePathAsync(linkId, language);

            if (File.Exists(filePath))
            {
                return await File.ReadAllTextAsync(filePath);
            }

            return string.Empty;
        }

        public string GenerateCssUrl(int linkId, string language)
        {
            var fileName = GetFileName(linkId, language);
            return $"/{AUTO_CSS_FOLDER}/{fileName}";
        }

        /// <summary>
        /// Get default CSS content template with helpful comments
        /// </summary>
        private string GetDefaultCssContent(int linkId, string language)
        {
            return $@"/* 
 * Auto-generated CSS file
 * Link ID: {linkId}
 * Language: {language}
 * Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
 * 
 * Add your custom CSS styles here
 */

/* Example:
.link-{linkId} {{
    /* Your styles here *\/
}}
*/
";
        }
    }
}
