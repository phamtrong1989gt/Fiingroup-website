using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using PT.Base;
using PT.Domain.Model;
using System;
using System.Threading.Tasks;

namespace PT.Component
{
    // Hiển thị module động từ cache hoặc file
    [ViewComponent(Name = "ViewModule")]
    public class ViewModuleComponent : ViewComponent
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ViewModuleComponent(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<HtmlString> InvokeAsync(ModuleType type, string code)
        {
            try
            {
                // Giả lập bất đồng bộ, thay bằng logic thực tế nếu cần
                await Task.Yield();

                var cachedData = CommonFunctions.GetModuleFromCacheOrFile(_hostingEnvironment.WebRootPath, type, code);
                return new HtmlString(cachedData ?? string.Empty);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine($"Error in ViewModuleComponent: {ex.Message}");
                return new HtmlString(string.Empty);
            }
        }
    }
}