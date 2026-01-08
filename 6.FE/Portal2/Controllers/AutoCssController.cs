using Microsoft.AspNetCore.Mvc;
using PT.Base.Services;
using System;
using System.Threading.Tasks;

namespace PT.UI.Controllers
{
    /// <summary>
    /// Controller for auto-generated CSS file management
    /// Production uses static files with asp-append-version
    /// This controller is for diagnostic/testing purposes only
    /// </summary>
    public class AutoCssController : Controller
    {
        private readonly IAutoCssService _autoCssService;

        public AutoCssController(IAutoCssService autoCssService)
        {
            _autoCssService = autoCssService;
        }

        /// <summary>
        /// Force create CSS file for testing
        /// GET: /AutoCss/Create?linkId=123&language=vi
        /// Returns: { success, message, filePath, exists, staticUrl }
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create(int linkId, string language)
        {
            if (linkId <= 0 || string.IsNullOrWhiteSpace(language))
            {
                return BadRequest("LinkId and language parameters are required.");
            }

            try
            {
                var filePath = await _autoCssService.GetCssFilePathAsync(linkId, language);
                var exists = System.IO.File.Exists(filePath);
                var staticUrl = $"/auto-css/link_{linkId}_{language}.css";
                
                return Json(new 
                { 
                    success = true,
                    message = exists ? "File already exists" : "File created successfully",
                    filePath,
                    exists,
                    staticUrl
                });
            }
            catch (Exception ex)
            {
                return Json(new 
                { 
                    success = false,
                    message = $"Error: {ex.Message}",
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
