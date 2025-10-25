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
    public class ContentPageTagRepository : BaseRepository<ContentPageTag>, IContentPageTagRepository
    {
        private readonly ApplicationContext _context;
        public ContentPageTagRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }
        public async Task<List<Tag>> GetTag(int skip = 0, int Take = 0,int pageContentId =0, Expression<Func<Tag, bool>> predicate = null, Func<IQueryable<Tag>, IOrderedQueryable<Tag>> orderBy = null)
        {
            IQueryable<Tag> query = _context.Tags.AsNoTracking().AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }
            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }

            query = _context.Tags
                .Where(x => _context.ContentPageTags.Any(m => m.TagId == x.Id && m.ContentPageId== pageContentId))
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.Tag).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { tag = x, links = y })
                         .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Tag
                         {
                             Link = y,
                             Id = x.tag.Id,
                             Banner = x.tag.Banner,
                             Name = x.tag.Name,
                             Language = x.tag.Language,
                             Status = x.tag.Status,
                         }).AsQueryable();
            if (Take > 0)
            {
                query = query.Skip(skip < 0 ? 0 : skip).Take(Take).AsQueryable();
            }
            return await query.ToListAsync();
        }
    }
}

