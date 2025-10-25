using Microsoft.EntityFrameworkCore;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PT.Infrastructure.Repositories
{
    public class ContentPageRelatedRepository : BaseRepository<ContentPageRelated>, IContentPageRelatedRepository
    {
        private readonly ApplicationContext _context;
        public ContentPageRelatedRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }
        public async Task<List<ContentPage>> GetContentPageAsync(int contentPageId,int skip = 0, int Take = 0, Expression<Func<ContentPage, bool>> predicate = null, Func<IQueryable<ContentPage>, IOrderedQueryable<ContentPage>> orderBy = null, Expression<Func<ContentPage, ContentPage>> select = null)
        {
            var query = _context.ContentPages.AsNoTracking().AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }
            query = query.Where(x => _context.ContentPageRelateds.Any(m => m.ContentPageId == x.Id && m.ParentId== contentPageId));
            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }
            if (Take > 0)
            {
                query = query.Skip(skip < 0 ? 0 : skip).Take(Take).AsQueryable();
            }
            query = query
               .GroupJoin(_context.Links.Where(x => (x.Type == CategoryType.Blog || x.Type == CategoryType.FAQ || x.Type == CategoryType.Service || x.Type == CategoryType.Page)).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
               .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new ContentPage
               {
                   Link = y,
                   Id = x.data.Id,
                   Author = x.data.Author,
                   Banner = x.data.Banner,
                   Content = x.data.Content,
                   DatePosted = x.data.DatePosted,
                   Name = x.data.Name,
                   Language = x.data.Language,
                   Price = x.data.Price,
                   Serice = x.data.Serice,
                   ServiceId = x.data.ServiceId,
                   Status = x.data.Status,
                   Summary = x.data.Summary,
                   Tags = x.data.Tags,
                   Type = x.data.Type
               }).AsQueryable();
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
            return await query.ToListAsync();
        }
    }
}

