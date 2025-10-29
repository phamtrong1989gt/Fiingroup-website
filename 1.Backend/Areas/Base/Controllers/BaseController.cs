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

namespace PT.UI.Areas.Base.Controllers
{
    public class BaseController : Controller
    {
        
        protected  string controllerName = "";
        protected  string tableName = "";

        public async Task AddFileData(int objectId, string path, CategoryType type, string altId)
        {
            var iFileDataRepository = (IFileDataRepository)AppHttpContext.Current.RequestServices.GetService(typeof(IFileDataRepository));
            await iFileDataRepository.AddAsync(new FileData { 
                CreatedDate = DateTime.Now,
                ObjectId  = objectId,
                Path = path,
                Type = type,
                AltId = altId
            });
            await iFileDataRepository.CommitAsync();
        }

        public async Task UpdateFileData(int objectId,CategoryType type, string altId)
        {
            var iFileDataRepository = (IFileDataRepository)AppHttpContext.Current.RequestServices.GetService(typeof(IFileDataRepository));
            var listFiles = await iFileDataRepository.SearchAsync(false, 0, 0, x => x.AltId == altId && x.Type == type);
            foreach(var item in listFiles)
            {
                item.ObjectId = objectId;
                iFileDataRepository.Update(item);
            }    
            await iFileDataRepository.CommitAsync();
        }

        public async Task RemoveFileData(int objectId, CategoryType type)
        {
            var iFileDataRepository = (IFileDataRepository)AppHttpContext.Current.RequestServices.GetService(typeof(IFileDataRepository));
            var iWebHostEnvironment = (IWebHostEnvironment)AppHttpContext.Current.RequestServices.GetService(typeof(IWebHostEnvironment));
            var listFiles = await iFileDataRepository.SearchAsync(false, 0, 0, x=> x.ObjectId == objectId && x.Type == type);
            foreach(var item in listFiles)
            {
                var path = $"{iWebHostEnvironment.WebRootPath}/{item.Path}";
                if(System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }    
                iFileDataRepository.Delete(item);
            }    
            await iFileDataRepository.CommitAsync();
        }

        public async  Task AddLog(LogModel input)
        {
            var iLogRepository = (ILogRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILogRepository));
            var baseSettings = (IOptions<LogSettings>)AppHttpContext.Current.RequestServices.GetService(typeof(IOptions<LogSettings>));
            if (baseSettings.Value.Is)
            {
                var data = new Domain.Model.Log
                {
                    ObjectId = input.ObjectId,
                    Name = input.Name,
                    AcctionUser = Newtonsoft.Json.JsonConvert.SerializeObject(new { DataUserInfo.UserId, DataUserInfo.DisplayName , DataUserInfo.UserName , DataUserInfo.Email }),
                    Object = tableName,
                    ObjectType = controllerName,
                    Type = input.Type,
                    ActionTime = DateTime.Now
                };
                if (baseSettings.Value.IsUseMongo)
                {
                    // Code mongo
                }
                else
                {
                    await iLogRepository.AddAsync(data);
                    await iLogRepository.CommitAsync();
                }
            }
        }

        public async Task<object> AddTagLink(string name,string language)
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
                Name = name
            };
            var check = await _iTagRepository.SingleOrDefaultAsync(true, x => x.Name.ToLower() == use.Name.ToLower() && x.Language == use.Language);
            if (check != null)
            {
                return new { id = check.Id, name = check.Name };
            }

            var ktLug = await _iLinkRepository.AnyAsync(x => x.Slug == use.Slug && x.Language == language);
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
                Language = use.Language
            };
            await _iTagRepository.AddAsync(data);
            await _iTagRepository.CommitAsync();
            var seoModel = MapModel<SeoModel>.Go(use);
            await AddSeoLink(CategoryType.Tag, data.Language, data.Id, seoModel, name, "", "TagHome", "Details");

            return new { id = data.Id, name = data.Name };
        }

        public async Task<int> AddSeoLink(CategoryType type, string language, int id, SeoModel model, string name, string area, string controller, string action)
        {
            var _iLinkRepository = (ILinkRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkRepository));
            var _iLinkReferenceRepository = (ILinkReferenceRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkReferenceRepository));
            var baseSettings = (IOptions<BaseSettings>)AppHttpContext.Current.RequestServices.GetService(typeof(IOptions<BaseSettings>));

            if (id > 0)
            {
                var ktLug = await _iLinkRepository.AnyAsync(x => x.Slug == model.Slug && x.ObjectId != id && x.Language == language);
                if (ktLug)
                {
                    model.Slug = $"{model.Slug}-{DateTime.Now:yyyyMMddHHmmss}";
                }
            }
            else
            {
                var ktLug = await _iLinkRepository.AnyAsync(x => x.Slug == model.Slug && x.Language == language);
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
                Lastmod = model.Lastmod??DateTime.Now,
                Priority = model.Priority.ConvertToDouble(),
                Delete = model.Delete,
                Description= model.Description,
                FacebookBanner = model.FacebookBanner,
                FacebookDescription = model.FacebookDescription,
                FocusKeywords=model.FocusKeywords,
                GooglePlusDescription = model.GooglePlusDescription,
                IncludeSitemap = model.IncludeSitemap,
                Keywords = model.Keywords,
                MetaRobotsAdvance = model.MetaRobotsAdvance,
                MetaRobotsFollow = model.MetaRobotsFollow,
                MetaRobotsIndex = model.MetaRobotsIndex,
                Redirect301 = model.Redirect301,
                Title= model.Title,
                Status = model.Status,
                Area = area,
                Controller = controller,
                Acction = action
            };
            await _iLinkRepository.AddAsync(dlLug);
            await _iLinkRepository.CommitAsync();

            // Update ánh xạ
            if(baseSettings.Value.MultipleLanguage)
            {
                model.Language = language;
                model.Type = dlLug.Type;
                model.LinkId = dlLug.Id;
                await _iLinkReferenceRepository.ReferenceUpdate(model);
            }
            // Update memory cache link
            var cache = (IMemoryCache)AppHttpContext.Current.RequestServices.GetService(typeof(IMemoryCache));
            var cacheKey = $"Link_{dlLug.Slug}_{language}";
            if (cache.TryGetValue(cacheKey, out Link link))
            {
                cache.Remove(cacheKey);
            }
            cache.Set(cacheKey, dlLug, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60 * 6)));
            return dlLug.Id;
        }

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
                    Acction = action
                };
                await _iLinkRepository.AddAsync(ktLink);
                await _iLinkRepository.CommitAsync();
            }
            else if (changeSlug)
            {
                ktLink.Slug = model.Slug;
            }
            ktLink.Changefreq = model.Changefreq;
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
                // Update ánh xạ
                await _iLinkReferenceRepository.ReferenceUpdate(model);
            }

            // Update memory cache link
            var cache = (IMemoryCache)AppHttpContext.Current.RequestServices.GetService(typeof(IMemoryCache));
            var cacheKey = $"Link_{ktLink.Slug}_{language}";
            if (cache.TryGetValue(cacheKey, out Link link))
            {
                cache.Remove(cacheKey);
            }
            cache.Set(cacheKey, ktLink, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60 * 6)));
        }

        public async Task DeleteSeoLink(CategoryType type, int id)
        {
            var _iLinkRepository = (ILinkRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkRepository));
            var ktLink = await _iLinkRepository.SingleOrDefaultAsync(false, x => x.ObjectId == id && x.Type == type);
            if (ktLink != null)
            {
                _iLinkRepository.Delete(ktLink);
                await _iLinkRepository.CommitAsync();
            }
            // Update memory cache link
            var cache = (IMemoryCache)AppHttpContext.Current.RequestServices.GetService(typeof(IMemoryCache));
            var cacheKey = $"Link_{ktLink.Slug}_{ktLink.Language}";
            if (cache.TryGetValue(cacheKey, out Link link))
            {
                cache.Remove(cacheKey);
            }
        }
    }
}