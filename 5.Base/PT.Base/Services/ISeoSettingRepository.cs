using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Infrastructure.Repositories;

namespace PT.Base.Services
{
    public interface ISettingService
    {
        Task<SeoSetting> SeoSettingGet(string languge, int portalId);
        //Task RefreshSeoSettingCache(string language, int portalId);
        void RefreshByKey(string cacheKey);
        Task<BindContentSetting> BindContentSettingGet(int portalId);
    }

    public class SettingService :  ISettingService
    {
        private readonly ISeoSettingRepository _iSeoSettingRepository;
        private readonly IBindContentSettingRepository _iBindContentSettingRepository;
        private readonly IMemoryCache _memoryCache;
        public SettingService(ISeoSettingRepository iSeoSettingRepository, IMemoryCache memoryCache, IBindContentSettingRepository iBindContentSettingRepository) 
        {
            _iSeoSettingRepository = iSeoSettingRepository;
            _memoryCache = memoryCache;
            _iBindContentSettingRepository = iBindContentSettingRepository;
        }

        // Use memory cache for24 hours. Method is async to use GetOrCreateAsync.
        public async Task<SeoSetting> SeoSettingGet(string language, int portalId)
        {
            if (string.IsNullOrEmpty(language)) language = "vi";
            var cacheKey = $"SeoSetting_{language}_{portalId}";

            var result = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
                // fetch from underlying repository
                var data = await _iSeoSettingRepository.SingleOrDefaultAsync(true, s => s.Language == language && s.PortalId == portalId);
                if(data == null)
                {
                    data = new SeoSetting
                    {
                        Language = language,
                        PortalId = portalId,
                        Title = string.Empty,
                        Description = string.Empty,
                        Keywords = string.Empty,
                        MetaGoogle = string.Empty,
                        Robots = string.Empty
                    };
                }    
                return data;
            });
            return result;
        }

        // Use memory cache for24 hours. Method is async to use GetOrCreateAsync.
        public async Task<BindContentSetting> BindContentSettingGet(int portalId)
        {
            var cacheKey = $"BindContentSetting_{portalId}";

            var result = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
                // fetch from underlying repository
                var data = await _iBindContentSettingRepository.SingleOrDefaultAsync(true, s => s.PortalId == portalId);
                if (data == null)
                {
                    data = new BindContentSetting
                    {
                        PortalId = portalId
                    };
                }
                return data;
            });
            return result;
        }

        // Refresh (invalidate and optionally repopulate) the cache for the given language and portalId
        public async Task RefreshSeoSettingCache(string language, int portalId)
        {
            if (string.IsNullOrEmpty(language)) language = "vi";
            var cacheKey = $"SeoSetting_{language}_{portalId}";
            // Remove existing cache entry
            _memoryCache.Remove(cacheKey);
            await SeoSettingGet(language, portalId);
        }

        public void RefreshByKey(string cacheKey)
        {
            _memoryCache.Remove(cacheKey);
        }
    }
}
