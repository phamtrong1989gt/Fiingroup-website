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
        private readonly IEmailSettingRepository _iEmailSettingRepository;
        public SettingService(ISeoSettingRepository iSeoSettingRepository, IMemoryCache memoryCache, IBindContentSettingRepository iBindContentSettingRepository, IEmailSettingRepository iEmailSettingRepository) 
        {
            _iSeoSettingRepository = iSeoSettingRepository;
            _memoryCache = memoryCache;
            _iBindContentSettingRepository = iBindContentSettingRepository;
            _iEmailSettingRepository = iEmailSettingRepository;
        }

        // Use memory cache for24 hours. Method is async to use GetOrCreateAsync.
        public async Task<SeoSetting> SeoSettingGet(string language, int portalId)
        {
            if (string.IsNullOrEmpty(language)) language = "vi";
            var cacheKey = $"SeoSetting::{language}::{portalId}";

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
            var cacheKey = $"BindContentSetting::{portalId}";

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

        public async Task<EmailSetting> EmailSettingGet(int portalId)
        {
            var cacheKey = $"EmailSetting::{portalId}";

            var result = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
                // fetch from underlying repository
                var data = await _iEmailSettingRepository.SingleOrDefaultAsync(true, s => s.PortalId == portalId);
                if (data == null)
                {
                    data = new EmailSetting
                    {
                        PortalId = portalId
                    };
                }
                return data;
            });
            return result;
        }

        public void RefreshByKey(string cacheKey)
        {
            _memoryCache.Remove(cacheKey);
        }
    }
}
