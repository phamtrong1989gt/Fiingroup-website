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
    public class TagRepository : BaseRepository<Tag>, ITagRepository
    {
        private readonly ApplicationContext _context;
        public TagRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }
        public override async Task<BaseSearchModel<List<Tag>>> SearchPagedListAsync(int page, int limit, Expression<Func<Tag, bool>> predicate = null, Func<IQueryable<Tag>, IOrderedQueryable<Tag>> orderBy = null, Expression<Func<Tag, Tag>> select = null, params Expression<Func<Tag, object>>[] includeProperties)
        {
            IQueryable<Tag> query = _context.Tags.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }
           
            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }
            query = query
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.Tag && x.Delete == false), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Tag
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Delete = x.data.Delete,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    PortalId = x.data.PortalId
                }).AsQueryable();
            var list = await query.Skip((page - 1) * limit).Take(limit).AsNoTracking().ToListAsync();


            var listLink = list.Select(x => x.Link?.Id);

            // List Phiên Bản
            var listReferences = await _context.LinkReferences
                .Where(x => listLink.Contains(x.LinkId1))
                .GroupJoin(_context.Links, x => x.LinkId2, y => y.Id, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new { x.data, link = y })
                .Select(x => new LinkReference
                {
                    Language = x.data.Language,
                    Id = x.data.Id,
                    LinkId2 = x.data.LinkId2,
                    LinkId1 = x.data.LinkId1,
                    Link2 = x.link
                }).ToListAsync();
            foreach (var item in list)
            {
                item.LinkReferences = listReferences.Where(x => x.LinkId1 == item.Link?.Id).ToList();
            }

            return new BaseSearchModel<List<Tag>>
            {
                Data = list,
                Limit = limit,
                Page = page,
                TotalRows = await query.CountAsync()
            };
        }
    }
}

