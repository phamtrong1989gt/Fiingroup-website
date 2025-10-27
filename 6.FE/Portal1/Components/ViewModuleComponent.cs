using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Caching.Memory;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;
using System;
using System.Threading.Tasks;

namespace PT.Component
{
    // Hiển thị module động từ cache hoặc file
    [ViewComponent(Name = "ViewModule")]
    public class ViewModuleComponent : ViewComponent
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private static IMemoryCache _iMemoryCache;
        private readonly IMenuRepository _iMenuRepository;
        private readonly IBannerRepository _iBannerRepository;
        private readonly IStaticInformationRepository _iStaticInformationRepository;
        public ViewModuleComponent(IWebHostEnvironment hostingEnvironment, IMemoryCache iMemoryCache, IMenuRepository iMenuRepository, IMenuItemRepository iMenuItemRepository, IBannerRepository iBannerRepository, IStaticInformationRepository iStaticInformationRepository)
        {
            _hostingEnvironment = hostingEnvironment;
            _iMemoryCache = iMemoryCache;
            _iMenuRepository = iMenuRepository;
            _iBannerRepository = iBannerRepository;
            _iStaticInformationRepository = iStaticInformationRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(ModuleType type, string code, string language = "vi", int portalId = 1)
        {
            try
            {
                if (portalId > 0)
                {
                    // Cache key duy nhất cho module (bao gồm type, code, language, portalId)
                    string cacheKey = $"ModuleHtml::{type}::{code}::{language}::{portalId}";

                    // Thử lấy từ cache trước
                    if (_iMemoryCache.TryGetValue(cacheKey, out string cachedHtml) && !string.IsNullOrEmpty(cachedHtml))
                    {
                        return new HtmlContentViewComponentResult(new HtmlString(cachedHtml));
                    }

                    string url = "";
                    string newKyTu = "";
                    string data = "";

                    // Xác định URL quản lý và lấy nội dung tương ứng
                    if (type == ModuleType.Menu)
                    {
                        var menu = await _iMenuRepository.SingleOrDefaultAsync(true, x => x.Code == code && x.Delete == false && x.Language == language && x.PortalId == portalId);
                        if (menu != null)
                        {
                            data = menu.Content;
                        }
                        url = $"/Admin/Manager/MenuManager?id={menu?.Id}&language={language}&portalId={portalId}#openPopup";
                    }
                    else if (type == ModuleType.PhotoSlide)
                    {
                        var banner = await _iBannerRepository.SingleOrDefaultAsync(true, x => x.Code == code && x.Delete == false && x.Language == language && x.PortalId == portalId && x.Type == BannerType.Slide);
                        if (banner != null)
                        {
                            data = banner.Content;
                        }
                        url = $"/Admin/Manager/PhotoSlideManager?id={banner?.Id}&language={language}&portalId={portalId}#openPopup";
                    }
                    else if (type == ModuleType.AdvertisingBanner)
                    {
                        var banner = await _iBannerRepository.SingleOrDefaultAsync(true, x => x.Code == code && x.Delete == false && x.Language == language && x.PortalId == portalId && x.Type == BannerType.Advertising);
                        if (banner != null)
                        {
                            data = banner.Content;
                        }
                        url = $"/Admin/Manager/AdvertisingBannerManager?id={banner?.Id}&language={language}&portalId={portalId}#openPopup";
                    }
                    else if (type == ModuleType.StaticInformation)
                    {
                        var info = await _iStaticInformationRepository.SingleOrDefaultAsync(true, x => x.Code == code && x.Delete == false && x.Language == language && x.PortalId == portalId);
                        if (info != null)
                        {
                            data = info.Content;
                        }
                        url = $"/Admin/Manager/StaticInformationManager?id={info?.Id}&language={language}&portalId={portalId}#openPopup";
                    }

                    // Tạo nội dung HTML
                    if (!string.IsNullOrEmpty(url))
                    {
                        newKyTu += "<div class='formadmin' data-editable data-name=\"main-content\">";
                        newKyTu += "<span data-href='" + url + "' class='bindata'></span>";
                        newKyTu += data;
                        newKyTu += "</div>";
                        newKyTu = Functions.ZipStringHTML(newKyTu);
                    }
                    else
                    {
                        newKyTu = Functions.ZipStringHTML(data);
                    }

                    // Lưu rendered HTML vào cache trong 24 giờ
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
                        Priority = CacheItemPriority.Normal
                    };

                    _iMemoryCache.Set(cacheKey, newKyTu, cacheOptions);

                    return new HtmlContentViewComponentResult(new HtmlString(newKyTu ?? string.Empty));
                }
                else
                {
                    var cachedData = CommonFunctions.GetModuleFromCacheOrFile(_hostingEnvironment.WebRootPath, type, code);
                    return new HtmlContentViewComponentResult(new HtmlString(cachedData));
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine($"Error in ViewModuleComponent: {ex.Message}");
                return new HtmlContentViewComponentResult(new HtmlString(null));
            }
        }
    }
}