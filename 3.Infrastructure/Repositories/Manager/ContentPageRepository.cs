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
    public class ContentPageRepository : BaseRepository<ContentPage>, IContentPageRepository
    {
        private readonly ApplicationContext _context;
        public ContentPageRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<BaseSearchModel<List<ContentPage>>> FAQSearchPagedListAsync(int page, int limit, int? categoryId, int? tagId, Expression<Func<ContentPage, bool>> predicate = null, Func<IQueryable<ContentPage>, IOrderedQueryable<ContentPage>> orderBy = null, Expression<Func<ContentPage, ContentPage>> select = null, params Expression<Func<ContentPage, object>>[] includeProperties)
        {
            IQueryable<ContentPage> query = _context.ContentPages.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }
            if (tagId != null)
            {
                query = query.Where(x => _context.ContentPageTags.Any(m => m.ContentPageId == x.Id && m.TagId == tagId)).AsQueryable();
            }
            if (categoryId != null)
            {
                query = query.Where(x => x.CategoryId == categoryId || _context.Categorys.Any(m => m.ParentId == categoryId)).AsQueryable();
            }
            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
           
            query = query
               .GroupJoin(_context.Categorys.AsQueryable(), x => x.CategoryId, y => y.Id, (x, y) => new { data = x, categorys = y })
               .SelectMany(x => x.categorys.DefaultIfEmpty(),(x,y)=> new  { x.data, category = y })
               .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.FAQ).AsQueryable(), x => x.data.Id, y => y.ObjectId, (x, y) => new { x.data, x.category, links = y })
               .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new ContentPage
               {
                   Category = x.category,
                   Id = x.data.Id,
                   Author = x.data.Author,
                   Banner = x.data.Banner,
                   Content = x.data.Content,
                   DatePosted = x.data.DatePosted,
                   Delete = x.data.Delete,
                   Name = x.data.Name,
                   Language = x.data.Language,
                   Price = x.data.Price,
                   ServiceId = x.data.ServiceId,
                   Status = x.data.Status,
                   Summary = x.data.Summary,
                   Tags = x.data.Tags,
                   Type = x.data.Type,
                   Link = y,
                   IsHome = x.data.IsHome,
                   StartDate = x.data.StartDate,
                   EndDate = x.data.EndDate,
                   CategoryId = x.data.CategoryId
               }).AsQueryable();

            var list = await query.Skip((page - 1) * limit).Take(limit).AsNoTracking().ToListAsync();

            var blogIds = list.Select(x => x.Id).ToList();

            var listTag = await _context.ContentPageTags
                .Where(x => blogIds.Contains(x.ContentPageId))
                .GroupJoin(_context.Tags, x => x.TagId, y => y.Id, (x, y) => new { blogId = x.ContentPageId, tags = y })
                .SelectMany(x => x.tags.DefaultIfEmpty(), (x, y) => new { x.blogId, tag = y })
                .Select(x => new ContentPageTag { ContentPageId = x.blogId, TagId = x.tag == null ? 0 : x.tag.Id, Tag = x.tag }).ToListAsync();
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
                item.Tags = listTag.Where(x => x.ContentPageId == item.Id).Select(x => x.Tag).ToList();
                item.LinkReferences = listReferences.Where(x => x.LinkId1 == item.Link?.Id).ToList();
            }
            return new BaseSearchModel<List<ContentPage>>
            {
                Data = list,
                Limit = limit,
                Page = page,
                TotalRows = await query.CountAsync()
            };
        }

        public async Task<BaseSearchModel<List<ContentPage>>> SearchPagedListAsync(int page, int limit,int? categoryId,int? tagId, Expression<Func<ContentPage, bool>> predicate = null, Func<IQueryable<ContentPage>, IOrderedQueryable<ContentPage>> orderBy = null, Expression<Func<ContentPage, ContentPage>> select = null, params Expression<Func<ContentPage, object>>[] includeProperties)
        {
            IQueryable<ContentPage> query = _context.ContentPages.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }
            if(tagId!=null)
            {
                query = query.Where(x=> _context.ContentPageTags.Any(m=>m.ContentPageId==x.Id && m.TagId== tagId)).AsQueryable();
            }
            if (categoryId != null)
            {
                query = query.Where(x => _context.ContentPageCategorys.Any(m=>m.ContentPageId==x.Id && m.CategoryId==categoryId)).AsQueryable();
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
                .GroupJoin(_context.Links.Where(x=>(x.Type == CategoryType.Blog || x.Type == CategoryType.FAQ || x.Type == CategoryType.Service || x.Type == CategoryType.Page || x.Type == CategoryType.PromotionInformation)).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new ContentPage {
                    Link = y,
                    Id=x.data.Id,
                    Author=x.data.Author,
                    Banner =x.data.Banner,
                    Content=x.data.Content,
                    DatePosted=x.data.DatePosted,
                    Delete=x.data.Delete,
                    Name=x.data.Name,
                    Language=x.data.Language,
                    Price=x.data.Price,
                    Serice=x.data.Serice,
                    ServiceId=x.data.ServiceId,
                    Status=x.data.Status,
                    Summary=x.data.Summary,
                    Tags=x.data.Tags,
                    Type=x.data.Type,
                    IsHome = x.data.IsHome,
                    StartDate = x.data.StartDate,
                    EndDate=x.data.EndDate,
                    CategoryId=x.data.CategoryId
                    
                }).AsQueryable();

            var list = await query.Skip((page - 1) * limit).Take(limit).AsNoTracking().ToListAsync();

            var blogIds = list.Select(x => x.Id).ToList();

            var listCategory = await _context.ContentPageCategorys
                .Where(x => blogIds.Contains(x.ContentPageId))
                .GroupJoin(_context.Categorys, x => x.CategoryId, y => y.Id, (x, y) => new { blogId = x.ContentPageId, categorys = y })
                .SelectMany(x => x.categorys.DefaultIfEmpty(), (x, y) => new { x.blogId, category = y })
                .Select(x => new ContentPageCategory { ContentPageId=x.blogId, CategoryId =x.category==null?0: x.category.Id, Category =x.category }).ToListAsync();

            var listTag = await _context.ContentPageTags
                .Where(x => blogIds.Contains(x.ContentPageId))
                .GroupJoin(_context.Tags, x => x.TagId, y => y.Id, (x, y) => new { blogId = x.ContentPageId, tags = y })
                .SelectMany(x => x.tags.DefaultIfEmpty(), (x, y) => new { x.blogId, tag = y })
                .Select(x => new ContentPageTag { ContentPageId = x.blogId, TagId = x.tag == null ? 0 : x.tag.Id, Tag = x.tag }).ToListAsync();

            var listLink = list.Select(x => x.Link?.Id);

            // List Phiên Bản
            var listReferences = await _context.LinkReferences
                .Where(x => listLink.Contains(x.LinkId1))
                .GroupJoin(_context.Links, x => x.LinkId2, y => y.Id, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new { x.data, link = y })
                .Select(x => new LinkReference {
                    Language = x.data.Language,
                    Id =x.data.Id,
                    LinkId2 =x.data.LinkId2,
                    LinkId1=x.data.LinkId1,
                    Link2 = x.link
                }).ToListAsync();

            foreach (var item in list)
            {
                item.Categorys = listCategory.Where(x => x.ContentPageId == item.Id).Select(x=>x.Category).ToList();
                item.Tags = listTag.Where(x => x.ContentPageId == item.Id).Select(x => x.Tag).ToList();
                item.LinkReferences = listReferences.Where(x => x.LinkId1 == item.Link?.Id).ToList();
            }

            return new BaseSearchModel<List<ContentPage>>
            {
                Data = list,
                Limit = limit,
                Page = page,
                TotalRows = await query.CountAsync()
            };
        }
        public async Task<List<ContentPage>> SearchAdvanceAsync(CategoryType type, int skip = 0, int Take = 0, int? categoryId =null, int? tagId = null, Expression<Func<ContentPage, bool>> predicate = null, Func<IQueryable<ContentPage>, IOrderedQueryable<ContentPage>> orderBy = null, Expression<Func<ContentPage, ContentPage>> select = null)
        {
            IQueryable<ContentPage> query = _context.ContentPages.AsNoTracking().AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }
            if (tagId != null)
            {
                query = query.Where(x => _context.ContentPageTags.Any(m => m.ContentPageId == x.Id && m.TagId == tagId)).AsQueryable();
            }
            if (categoryId != null && type !=CategoryType.FAQ)
            {
                query = query.Where(x => _context.ContentPageCategorys.Any(m => m.ContentPageId == x.Id && m.CategoryId == categoryId)).AsQueryable();
            }
            else if(type == CategoryType.FAQ)
            {
                query = query.Where(x=>x.CategoryId==categoryId || categoryId==null);
            }
            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
          
            query = query
                .GroupJoin(_context.Links.Where(x => (x.Type == CategoryType.Blog || x.Type == CategoryType.FAQ || x.Type == CategoryType.Service || x.Type == CategoryType.Page || x.Type == CategoryType.PromotionInformation)).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new ContentPage
                {
                    Link = y,
                    Id = x.data.Id,
                    Author = x.data.Author,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    DatePosted = x.data.DatePosted,
                    Delete = x.data.Delete,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Price = x.data.Price,
                    Serice = x.data.Serice,
                    ServiceId = x.data.ServiceId,
                    Status = x.data.Status,
                    Summary = x.data.Summary,
                    Tags = x.data.Tags,
                    Type = x.data.Type,
                    StartDate = x.data.StartDate,
                    EndDate = x.data.EndDate,
                    CategoryId = x.data.CategoryId
                }).AsQueryable();
            if (Take > 0)
            {
                query = query.Skip(skip < 0 ? 0 : skip).Take(Take).AsQueryable();
            }
            return await query.ToListAsync();
        }

    }
}

