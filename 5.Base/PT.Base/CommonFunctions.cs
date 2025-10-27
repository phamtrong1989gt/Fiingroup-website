using PT.Domain.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PT.Shared;
using Microsoft.Extensions.Caching.Memory;
namespace PT.Base
{
    public class CommonFunctions
    {
        private static IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions()); // Khởi tạo MemoryCache

        public static string GetStringModule(ModuleType type, int id)
        {
            return $"<vc:view-module id=\"{id}\" type=\"{type.ToString()}\"></vc:view-module>";
        }

        public static string GetStringModule(ModuleType type, string code, string language, int portalId)
        {
            return $"@await Component.InvokeAsync(\"ViewModule\", new {{ type = ModuleType.{type}, code = \"{code}\", language = language, portalId = portalId }})";
        }

        public static string GetStringModule(ModuleType type, string code)
        {
            return $"<vc:view-module code=\"{code}\" type=\"{type.ToString()}\"></vc:view-module>";
        }

        public static string GenContentModule(string data, ModuleType type, string code, string language)
        {
            string cacheKey = $"{type}_{code}"; // Tạo khóa cache duy nhất
            string url = "";
            string newKyTu = "";

            // Xác định URL quản lý
            if (type == ModuleType.Menu)
            {
                url = $"/Admin/Manager/MenuManager?code={code}&language={language}#openPopup";
            }
            else if (type == ModuleType.PhotoSlide)
            {
                url = $"/Admin/Manager/PhotoSlideManager?code={code}&language={language}#openPopup";
            }
            else if (type == ModuleType.StaticInformation)
            {
                url = $"/Admin/Manager/StaticInformationManager?code={code}&language={language}#openPopup";
            }
            else if (type == ModuleType.AdvertisingBanner)
            {
                url = $"/Admin/Manager/AdvertisingBannerManager?code={code}&language={language}#openPopup";
            }
            newKyTu += $"<!-- module {code} {type.GetDisplayName()} -->";

            // Tạo nội dung HTML
            if (!string.IsNullOrEmpty(url))
            {
                newKyTu += "<div class='formadmin'>";
                newKyTu += "<span data-href='" + url + "' class='bindata'></span>";
                newKyTu += "<div  data-editable data-name=\"main-content\">";
                newKyTu += data;
                newKyTu += "</div>";
                newKyTu += "</div>";
                newKyTu = Functions.ZipStringHTML(newKyTu);
            }
            else
            {
                newKyTu = Functions.ZipStringHTML(data);
            }
            return newKyTu;
        }

        public static void TriggerCacheModuleClear(string data, ModuleType type, string code, string language, int portalId)
        {
            // Call API đến FE để FE Lấy Cache Mới
            string cacheKey = $"{type}_{code}_{language}_{portalId}";
            return;
        }

        public static void GenModule(string map, string data, ModuleType type, string code, string language)
        {
            string cacheKey = $"{type}_{code}"; // Tạo khóa cache duy nhất
            string url = "";
            string NewKyTu = "";

            // Xác định URL quản lý
            if (type == ModuleType.Menu)
            {
                url = $"/Admin/Manager/MenuManager?code={code}&language={language}#openPopup";
            }
            else if (type == ModuleType.PhotoSlide)
            {
                url = $"/Admin/Manager/PhotoSlideManager?code={code}&language={language}#openPopup";
            }
            else if (type == ModuleType.StaticInformation)
            {
                url = $"/Admin/Manager/StaticInformationManager?code={code}&language={language}#openPopup";
            }
            else if (type == ModuleType.AdvertisingBanner)
            {
                url = $"/Admin/Manager/AdvertisingBannerManager?code={code}&language={language}#openPopup";
            }
            NewKyTu += $"<!-- module {code} {type.GetDisplayName()} -->";

            // Tạo nội dung HTML
            if (!string.IsNullOrEmpty(url))
            {
                NewKyTu += "<div class='formadmin'>";
                NewKyTu += "<span data-href='" + url + "' class='bindata'></span>";
                NewKyTu += "<div  data-editable data-name=\"main-content\">";
                NewKyTu += data;
                NewKyTu += "</div>";
                NewKyTu += "</div>";
                NewKyTu = Functions.ZipStringHTML(NewKyTu);
            }
            else
            {
                NewKyTu = Functions.ZipStringHTML(data);
            }

            // Lưu vào cache
            _cache.Set(cacheKey, NewKyTu, TimeSpan.FromHours(24)); // Lưu cache với thời gian hết hạn là 24 giờ
            var path = @$"{map}/Module";
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path); // Tạo thư mục nếu chưa tồn tại
            }
            // Lưu vào file HTML
            string filePath = $"{path}/{type.ToString()}_{code}.html";
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)); // Đảm bảo thư mục tồn tại
            File.WriteAllText(filePath, NewKyTu);
        }

        public static void GenModule(string map, string data, ModuleType type, int id, string language)
        {
            string cacheKey = $"{type}_{id}"; // Tạo khóa cache duy nhất
            string url = "";
            string newKyTu = "";

            // Xác định URL quản lý
            if (type == ModuleType.Menu)
            {
                url = $"/Admin/Manager/MenuManager?id={id}&language={language}#openPopup";
            }
            else if (type == ModuleType.PhotoSlide)
            {
                url = $"/Admin/Manager/PhotoSlideManager?id={id}&language={language}#openPopup";
            }
            else if (type == ModuleType.StaticInformation)
            {
                url = $"/Admin/Manager/StaticInformationManager?id={id}&language={language}#openPopup";
            }
            else if (type == ModuleType.AdvertisingBanner)
            {
                url = $"/Admin/Manager/AdvertisingBannerManager?id={id}&language={language}#openPopup";
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

            // Lưu vào cache
            _cache.Set(cacheKey, newKyTu, TimeSpan.FromHours(24)); // Lưu cache với thời gian hết hạn là 24 giờ
        }

        public static string GetModuleFromCacheOrFile(string map, ModuleType type, string code)
        {
            string cacheKey = $"{type}_{code}"; // Tạo khóa cache duy nhất

            // Kiểm tra dữ liệu trong cache
            if (_cache.TryGetValue(cacheKey, out string cachedData))
            {
                return cachedData; // Trả về dữ liệu từ cache nếu có
            }
            var path = @$"{map}/Module";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path); // Tạo thư mục nếu chưa tồn tại
            }
            // Lưu vào file HTML
            string filePath = $"{path}/{type.ToString()}_{code}.html";
            if (File.Exists(filePath))
            {
                string fileData = File.ReadAllText(filePath);

                // Lưu lại vào cache
                _cache.Set(cacheKey, fileData, TimeSpan.FromHours(24));
                return fileData;
            }

            // Nếu không có dữ liệu trong cache và file, trả về chuỗi rỗng
            return string.Empty;
        }

        public static string GetModuleFromCacheOrFile(string map, ModuleType type, int id, string language)
        {
            string cacheKey = $"{type}_{id}_{language}"; // Tạo khóa cache duy nhất

            // Kiểm tra dữ liệu trong cache
            if (_cache.TryGetValue(cacheKey, out string cachedData))
            {
                return cachedData; // Trả về dữ liệu từ cache nếu có
            }

            // Nếu cache không có, đọc từ file HTML
            string filePath = $"{map}/{type.ToString()}_{id}.html";
            if (File.Exists(filePath))
            {
                string fileData = File.ReadAllText(filePath);

                // Lưu lại vào cache
                _cache.Set(cacheKey, fileData, TimeSpan.FromHours(24));
                return fileData;
            }

            // Nếu không có dữ liệu trong cache và file, trả về chuỗi rỗng
            return string.Empty;
        }
    }
}
