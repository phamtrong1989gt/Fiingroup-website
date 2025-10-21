using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Infrastructure.Repositories;
using PT.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PT.BE.Areas.Base.Controllers
{
    /// <summary>
    /// Controller cơ sở cho các controller khác kế thừa, cung cấp các hàm dùng chung như quản lý file, log, tag, SEO link, và trả về kết quả chuẩn hóa.
    /// </summary>
    public class BaseController : Controller
    {
        /// <summary>
        /// Tên controller hiện tại (dùng cho log, tracking)
        /// </summary>
        protected string controllerName = "";
        /// <summary>
        /// Tên bảng dữ liệu liên quan (dùng cho log, tracking)
        /// </summary>
        protected string tableName = "";

        /// <summary>
        /// Thêm dữ liệu file vào hệ thống
        /// </summary>
        /// <param name="objectId">Id đối tượng liên kết file</param>
        /// <param name="path">Đường dẫn file</param>
        /// <param name="type">Loại danh mục file</param>
        /// <param name="altId">Id thay thế (nếu có)</param>
        public async Task AddFileData(int objectId, string path, CategoryType type, string altId)
        {
            var iFileDataRepository = (IFileDataRepository)AppHttpContext.Current.RequestServices.GetService(typeof(IFileDataRepository));
            await iFileDataRepository.AddAsync(new FileData {
                CreatedDate = DateTime.Now,
                ObjectId = objectId,
                Path = path,
                Type = type,
                AltId = altId
            });
            await iFileDataRepository.CommitAsync();
        }

        /// <summary>
        /// Cập nhật dữ liệu file liên kết với đối tượng
        /// </summary>
        /// <param name="objectId">Id đối tượng</param>
        /// <param name="type">Loại danh mục file</param>
        /// <param name="altId">Id thay thế</param>
        public async Task UpdateFileData(int objectId, CategoryType type, string altId)
        {
            var iFileDataRepository = (IFileDataRepository)AppHttpContext.Current.RequestServices.GetService(typeof(IFileDataRepository));
            var listFiles = await iFileDataRepository.SearchAsync(false, 0, 0, x => x.AltId == altId && x.Type == type);
            foreach (var item in listFiles)
            {
                item.ObjectId = objectId;
                iFileDataRepository.Update(item);
            }
            await iFileDataRepository.CommitAsync();
        }

        /// <summary>
        /// Xóa dữ liệu file liên kết với đối tượng và xóa file vật lý trên hệ thống
        /// </summary>
        /// <param name="objectId">Id đối tượng</param>
        /// <param name="type">Loại danh mục file</param>
        public async Task RemoveFileData(int objectId, CategoryType type)
        {
            var iFileDataRepository = (IFileDataRepository)AppHttpContext.Current.RequestServices.GetService(typeof(IFileDataRepository));
            var iWebHostEnvironment = (IWebHostEnvironment)AppHttpContext.Current.RequestServices.GetService(typeof(IWebHostEnvironment));
            var listFiles = await iFileDataRepository.SearchAsync(false, 0, 0, x => x.ObjectId == objectId && x.Type == type);
            foreach (var item in listFiles)
            {
                var path = $"{iWebHostEnvironment.WebRootPath}/{item.Path}";
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                iFileDataRepository.Delete(item);
            }
            await iFileDataRepository.CommitAsync();
        }

        /// <summary>
        /// Ghi log thao tác vào hệ thống
        /// </summary>
        /// <param name="input">Thông tin log cần ghi</param>
        public async Task AddLog(LogModel input)
        {
            var iLogRepository = (ILogRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILogRepository));
            var baseSettings = (IOptions<LogSettings>)AppHttpContext.Current.RequestServices.GetService(typeof(IOptions<LogSettings>));
            if (baseSettings.Value.Is)
            {
                var data = new Domain.Model.Log
                {
                    ObjectId = input.ObjectId,
                    Name = input.Name,
                    AcctionUser = Newtonsoft.Json.JsonConvert.SerializeObject(new { DataUserInfo.UserId, DataUserInfo.DisplayName, DataUserInfo.UserName, DataUserInfo.Email }),
                    Object = tableName,
                    ObjectType = controllerName,
                    Type = input.Type,
                    ActionTime = DateTime.Now
                };
                if (baseSettings.Value.IsUseMongo)
                {
                    // Code ghi log vào MongoDB nếu cấu hình bật
                }
                else
                {
                    await iLogRepository.AddAsync(data);
                    await iLogRepository.CommitAsync();
                }
            }
        }

        /// <summary>
        /// Thêm tag và liên kết SEO cho đối tượng
        /// </summary>
        /// <param name="name">Tên tag</param>
        /// <param name="language">Ngôn ngữ</param>
        /// <returns>Thông tin tag vừa thêm</returns>
        public async Task<object> AddTagLink(string name, string language, int portalId = 1)
        {
            var _iLinkRepository = (ILinkRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkRepository));
            var _iTagRepository = (ITagRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ITagRepository));

            if (name == null || language == null)
            {
                return null;
            }
            var use = new TagModel
            {
                Type = CategoryType.Tag,
                Slug = Functions.ToUrlSlug(name),
                Language = language,
                Name = name,
                PortalId = portalId
            };
            var check = await _iTagRepository.SingleOrDefaultAsync(true, x => x.Name.ToLower() == use.Name.ToLower().Trim() && x.Language == use.Language && x.PortalId == use.PortalId);
            if (check != null)
            {
                return new { id = check.Id, name = check.Name };
            }

            var ktLug = await _iLinkRepository.AnyAsync(x => x.Slug == use.Slug && x.Language == language && x.PortalId == portalId);
            if (ktLug)
            {
                for (int i = 1; i <= 30; i++)
                {
                    use.Slug = $"{use.Slug}-{i}";
                    var ktLug2 = await _iLinkRepository.AnyAsync(x => x.Slug == use.Slug && x.Language == language);
                    if (!ktLug2)
                    {
                        break;
                    }
                    if (i == 30)
                    {
                        return null;
                    }
                }
            }

            var data = new Tag
            {
                Name = use.Name,
                Delete = false,
                Status = true,
                Language = use.Language,
                PortalId = use.PortalId ?? 1
            };
            await _iTagRepository.AddAsync(data);
            await _iTagRepository.CommitAsync();
            var seoModel = MapModel<SeoModel>.Go(use);
            await AddSeoLink(CategoryType.Tag, data.Language, data.Id, seoModel, name, "", "TagHome", "Details", portalId);

            return new { id = data.Id, name = data.Name };
        }

        /// <summary>
        /// Thêm liên kết SEO cho đối tượng
        /// </summary>
        /// <param name="type">Loại danh mục</param>
        /// <param name="language">Ngôn ngữ</param>
        /// <param name="id">Id đối tượng</param>
        /// <param name="model">Model SEO</param>
        /// <param name="name">Tên đối tượng</param>
        /// <param name="area">Khu vực</param>
        /// <param name="controller">Controller</param>
        /// <param name="action">Action</param>
        /// <returns>Id liên kết SEO vừa thêm</returns>
        public async Task<int> AddSeoLink(CategoryType type, string language, int id, SeoModel model, string name, string area, string controller, string action, int portalId = 1)
        {
            // Check cả slug đã delete, path delete sẽ tự động redrirect về trang chủ
            var _iLinkRepository = (ILinkRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkRepository));
            var _iLinkReferenceRepository = (ILinkReferenceRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkReferenceRepository));
            var baseSettings = (IOptions<BaseSettings>)AppHttpContext.Current.RequestServices.GetService(typeof(IOptions<BaseSettings>));
            if (id > 0)
            {
                var ktLug = await _iLinkRepository.AnyAsync(x => x.Slug == model.Slug && x.ObjectId != id && x.Language == language && x.PortalId == portalId);
                if (ktLug)
                {
                    model.Slug = $"{model.Slug}-{DateTime.Now:yyyyMMddHHmmss}";
                }
            }
            else
            {
                var ktLug = await _iLinkRepository.AnyAsync(x => x.Slug == model.Slug && x.Language == language && x.PortalId == portalId);
                if (ktLug)
                {
                    model.Slug = $"{model.Slug}-{DateTime.Now:yyyyMMddHHmmss}";
                }
            }

            var dlLug = new Link
            {
                Slug = model.Slug,
                Name = name,
                Type = type,
                ObjectId = id,
                Language = language,
                IsStatic = false,
                Changefreq = model.Changefreq,
                Lastmod = model.Lastmod ?? DateTime.Now,
                Priority = model.Priority.ConvertToDouble(),
                Delete = model.Delete,
                Description = model.Description,
                FacebookBanner = model.FacebookBanner,
                FacebookDescription = model.FacebookDescription,
                FocusKeywords = model.FocusKeywords,
                GooglePlusDescription = model.GooglePlusDescription,
                IncludeSitemap = model.IncludeSitemap,
                Keywords = model.Keywords,
                MetaRobotsAdvance = model.MetaRobotsAdvance,
                MetaRobotsFollow = model.MetaRobotsFollow,
                MetaRobotsIndex = model.MetaRobotsIndex,
                Redirect301 = model.Redirect301,
                Title = model.Title,
                Status = model.Status,
                Area = area,
                Controller = controller,
                Acction = action,
                PortalId = portalId
            };
            await _iLinkRepository.AddAsync(dlLug);
            await _iLinkRepository.CommitAsync();

            // Nếu cấu hình đa ngôn ngữ, cập nhật ánh xạ
            if (baseSettings.Value.MultipleLanguage)
            {
                model.Language = language;
                model.Type = dlLug.Type;
                model.LinkId = dlLug.Id;
                await _iLinkReferenceRepository.ReferenceUpdate(model);
            }
            // Cập nhật cache liên kết SEO
            var cache = (IMemoryCache)AppHttpContext.Current.RequestServices.GetService(typeof(IMemoryCache));
            var cacheKey = $"Link_{dlLug.Slug}_{language}_{dlLug.PortalId}";
            if (cache.TryGetValue(cacheKey, out Link link))
            {
                cache.Remove(cacheKey);
            }
            cache.Set(cacheKey, dlLug, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60 * 6)));
            return dlLug.Id;
        }

        /// <summary>
        /// Cập nhật liên kết SEO cho đối tượng
        /// </summary>
        /// <param name="changeSlug">Có thay đổi slug không</param>
        /// <param name="type">Loại danh mục</param>
        /// <param name="id">Id đối tượng</param>
        /// <param name="language">Ngôn ngữ</param>
        /// <param name="model">Model SEO</param>
        /// <param name="name">Tên đối tượng</param>
        /// <param name="area">Khu vực</param>
        /// <param name="controller">Controller</param>
        /// <param name="action">Action</param>
        public async Task UpdateSeoLink(bool changeSlug, CategoryType type, int id, string language, SeoModel model, string name, string area, string controller, string action)
        {
            var _iLinkRepository = (ILinkRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkRepository));
            var _iLinkReferenceRepository = (ILinkReferenceRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkReferenceRepository));
            var baseSettings = (IOptions<BaseSettings>)AppHttpContext.Current.RequestServices.GetService(typeof(IOptions<BaseSettings>));

            var ktLink = await _iLinkRepository.SingleOrDefaultAsync(false, x => x.ObjectId == id && x.Type == type);
            if (ktLink == null)
            {
                ktLink = new Link
                {
                    Slug = model.Slug,
                    Type = type,
                    Name = name,
                    ObjectId = id,
                    Language = language,
                    IsStatic = false,
                    Changefreq = model.Changefreq,
                    Lastmod = model.Lastmod ?? DateTime.Now,
                    Priority = model.Priority.ConvertToDouble(),
                    Delete = model.Delete,
                    Description = model.Description,
                    FacebookBanner = model.FacebookBanner,
                    FacebookDescription = model.FacebookDescription,
                    FocusKeywords = model.FocusKeywords,
                    GooglePlusDescription = model.GooglePlusDescription,
                    IncludeSitemap = model.IncludeSitemap,
                    Keywords = model.Keywords,
                    MetaRobotsAdvance = model.MetaRobotsAdvance,
                    MetaRobotsFollow = model.MetaRobotsFollow,
                    MetaRobotsIndex = model.MetaRobotsIndex,
                    Redirect301 = model.Redirect301,
                    Title = model.Title,
                    Status = model.Status,
                    Area = area,
                    Controller = controller,
                    Acction = action,
                    PortalId = model.PortalId ?? 0
                };
                await _iLinkRepository.AddAsync(ktLink);
                await _iLinkRepository.CommitAsync();
            }
            else if (changeSlug)
            {
                // nếu thay đổi slug thì phải tạo 1 link mới với Slug cũ trạng thái xóa và redirect 301 về Slug mới
                var dlLugOld = new Link
                {
                    Slug = ktLink.Slug,
                    Name = ktLink.Name,
                    Type = ktLink.Type,
                    ObjectId = ktLink.ObjectId,
                    Language = ktLink.Language,
                    IsStatic = false,
                    Changefreq = ktLink.Changefreq,
                    Lastmod = DateTime.Now,
                    Priority = ktLink.Priority,
                    Delete = true,
                    Description = ktLink.Description,
                    FacebookBanner = ktLink.FacebookBanner,
                    FacebookDescription = ktLink.FacebookDescription,
                    FocusKeywords = ktLink.FocusKeywords,
                    GooglePlusDescription = ktLink.GooglePlusDescription,
                    IncludeSitemap = ktLink.IncludeSitemap,
                    Keywords = ktLink.Keywords,
                    MetaRobotsAdvance = ktLink.MetaRobotsAdvance,
                    MetaRobotsFollow = ktLink.MetaRobotsFollow,
                    MetaRobotsIndex = ktLink.MetaRobotsIndex,
                    Redirect301 = $"/{language}/{model.Slug}.html",
                    Title = ktLink.Title,
                    Status = false,
                    Area = ktLink.Area,
                    Controller = ktLink.Controller,
                    Acction = ktLink.Acction,
                    PortalId = ktLink.PortalId
                };
                await _iLinkRepository.AddAsync(dlLugOld);
                await _iLinkRepository.CommitAsync();
                ktLink.Slug = model.Slug;
            }
            ktLink.Changefreq = model.Changefreq;
            ktLink.Lastmod = model.Lastmod ?? DateTime.Now;
            ktLink.Priority = model.Priority.ConvertToDouble();
            ktLink.Description = model.Description;
            ktLink.FacebookBanner = model.FacebookBanner;
            ktLink.FacebookDescription = model.FacebookDescription;
            ktLink.FocusKeywords = model.FocusKeywords;
            ktLink.GooglePlusDescription = model.GooglePlusDescription;
            ktLink.IncludeSitemap = model.IncludeSitemap;
            ktLink.Keywords = model.Keywords;
            ktLink.MetaRobotsAdvance = model.MetaRobotsAdvance;
            ktLink.MetaRobotsFollow = model.MetaRobotsFollow;
            ktLink.MetaRobotsIndex = model.MetaRobotsIndex;
            ktLink.Redirect301 = model.Redirect301;
            ktLink.Title = model.Title;
            ktLink.Status = model.Status;
            ktLink.Name = name;
            ktLink.Area = area;
            ktLink.Controller = controller;
            ktLink.Acction = action;

            await _iLinkRepository.CommitAsync();
            if (baseSettings.Value.MultipleLanguage)
            {
                model.Language = language;
                await _iLinkReferenceRepository.ReferenceUpdate(model);
            }

            // Cập nhật cache liên kết SEO
            var cache = (IMemoryCache)AppHttpContext.Current.RequestServices.GetService(typeof(IMemoryCache));
            var cacheKey = $"Link_{ktLink.Slug}_{language}_{ktLink.PortalId}";
            if (cache.TryGetValue(cacheKey, out Link link))
            {
                cache.Remove(cacheKey);
            }
            cache.Set(cacheKey, ktLink, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60 * 6)));
        }

        /// <summary>
        /// Xóa liên kết SEO của đối tượng
        /// </summary>
        /// <param name="type">Loại danh mục</param>
        /// <param name="id">Id đối tượng</param>
        public async Task DeleteSeoLink(CategoryType type, int id)
        {
            var _iLinkRepository = (ILinkRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkRepository));
            var ktLink = await _iLinkRepository.SingleOrDefaultAsync(false, x => x.ObjectId == id && x.Type == type);
            if (ktLink != null)
            {
                ktLink.Delete = true;
                ktLink.Redirect301 = "/"+ktLink.Language;
                _iLinkRepository.Update(ktLink);
                // Thay vì xóa hẳn thì sẽ đánh dấu đã xóa và auto redirect 301 về trang chủ
                await _iLinkRepository.CommitAsync();
            }
            // Cập nhật cache liên kết SEO
            var cache = (IMemoryCache)AppHttpContext.Current.RequestServices.GetService(typeof(IMemoryCache));
            var cacheKey = $"Link_{ktLink.Slug}_{ktLink.Language}_{ktLink.PortalId}";
            if (cache.TryGetValue(cacheKey, out Link link))
            {
                cache.Remove(cacheKey);
            }
        }

        /// <summary>
        /// Tạo đối tượng ResponseModel chuẩn cho các thao tác CRUD.
        /// </summary>
        /// <param name="output">Kết quả thực hiện (1: thành công, 0: cảnh báo, -1: lỗi)</param>
        /// <param name="message">Thông báo trả về</param>
        /// <param name="type">Loại thông báo (Success, Warning, Danger...)</param>
        /// <param name="isClosePopup">Đóng popup sau khi thực hiện</param>
        /// <param name="isRedirect">Chuyển hướng sau khi thực hiện</param>
        /// <returns>Đối tượng ResponseModel</returns>
        protected ResponseModel CreateResponse(int output, string message, string type, bool isClosePopup = false, bool isRedirect = false)
        {
            return new ResponseModel
            {
                Output = output,
                Message = message,
                Type = type,
                IsClosePopup = isClosePopup,
                IsRedirect = isRedirect
            };
        }
    }
}