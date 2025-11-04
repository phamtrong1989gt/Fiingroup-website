using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using PT.Domain.Model;
using PT.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PT.Infrastructure.Interfaces
{
    public interface IPortalRepository : IGenericRepository<Portal>
    {
        Task<string> GetFullPathAsync(int portalId, string slug, string language = null, bool multipleLanguage = false);
        Task<string> GetFullPathAsync(int portalId, string slug, IEnumerable<Portal> portals, string language = null, bool multipleLanguage = false);
    }
    public class PortalRepository : BaseRepository<Portal>, IPortalRepository
    {
        private readonly ApplicationContext _context;
        private readonly IWebHostEnvironment _env;
        public PortalRepository(ApplicationContext context, IWebHostEnvironment env) : base(context)
        {
            _context = context;
            _env = env;
        }

        public async Task<string> GetFullPathAsync(int portalId, string slug, string language = null, bool multipleLanguage = false)
        {
            var portal = await _context.Portals.FindAsync(portalId);
            var domain = portal?.Domain?.TrimEnd('/') ?? string.Empty;
            var cleanedSlug = (slug ?? string.Empty).TrimStart('/');
            if (multipleLanguage && !string.IsNullOrWhiteSpace(language))
            {
                return $"{domain}/{language}/{cleanedSlug}.html";
            }
            return $"{domain}/{cleanedSlug}.html";
        }

        public Task<string> GetFullPathAsync(int portalId, string slug, IEnumerable<Portal> portals, string language = null, bool multipleLanguage = false)
        {
            var portal = portals?.FirstOrDefault(p => p.Id == portalId);
            var domain = portal?.Domain?.TrimEnd('/') ?? string.Empty;
            if(_env.IsDevelopment())
            {
                domain = portal.DomainDev;
            }
            var cleanedSlug = (slug ?? string.Empty).TrimStart('/');
            if (multipleLanguage && !string.IsNullOrWhiteSpace(language))
            {
                return Task.FromResult($"{domain}/{language}/{cleanedSlug}.html");
            }
            return Task.FromResult($"{domain}/{cleanedSlug}.html");
        }
    }
}
