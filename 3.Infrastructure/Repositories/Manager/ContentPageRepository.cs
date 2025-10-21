using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.WebSockets;
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

        public async Task<List<ContentPageShared>> ContentPageSharedGets(int contentPageId)
        {
            var checkShared = await _context.ContentPageShareds.AsNoTracking().FirstOrDefaultAsync(x => (x.ParentContentPageId == contentPageId || x.SharedContentPageId == contentPageId));
            if(checkShared==null)
            {
                return new List<ContentPageShared>();
            }
            else
            {
                // Là con
                if(checkShared.SharedContentPageId == contentPageId)
                {
                     return await _context.ContentPageShareds
                       .AsNoTracking()
                       .Where(x => x.ParentContentPageId == checkShared.ParentContentPageId)
                       .ToListAsync();
                }
                // là cha
                else
                {
                    return await _context.ContentPageShareds
                           .AsNoTracking()
                           .Where(x => x.ParentContentPageId == contentPageId)
                           .ToListAsync();
                }     
            }    
    
        }

        public async Task ContentPageSharedRefeshContent(int contentPageId)
        {
            var sourceContent = await _context.ContentPages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == contentPageId);
            if(sourceContent != null)
            {
                var sharedContents = await ContentPageSharedGets(contentPageId);

                foreach (var shared in sharedContents)
                {
                    var targetContent = await _context.ContentPages.FirstOrDefaultAsync(x => (x.Id == shared.SharedContentPageId || x.Id == shared.ParentContentPageId) && x.Id != contentPageId);
                    if (targetContent != null)
                    {
                        targetContent.Author = sourceContent.Author;
                        targetContent.Banner = sourceContent.Banner;
                        targetContent.Content = sourceContent.Content;
                        targetContent.DatePosted = sourceContent.DatePosted;
                        targetContent.Delete = sourceContent.Delete;
                        targetContent.Name = sourceContent.Name;
                        targetContent.Price = sourceContent.Price;
                        targetContent.ServiceId = sourceContent.ServiceId;
                        targetContent.Status = sourceContent.Status;
                        targetContent.Summary = sourceContent.Summary;
                        targetContent.StartDate = sourceContent.StartDate;
                        targetContent.EndDate = sourceContent.EndDate;
                        targetContent.CategoryId = sourceContent.CategoryId;
                        _context.ContentPages.Update(targetContent);
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task ContentPageSharedAdds(int parentContentPageId, int parrentPortalId, List<int> sharedIds)
        {
            if (sharedIds == null || sharedIds.Count == 0)
                return;
            // Trường hợp là con trong bảng shared thì khoogn xử lý
            var checkChildrent = await _context.ContentPageShareds.AsNoTracking().AnyAsync(x => x.SharedContentPageId == parentContentPageId && x.SharedPortalId == parrentPortalId);
            if(checkChildrent)
            {
                return;
            }    
            // Load parent once
            var parentContentPage = await _context.ContentPages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == parentContentPageId);
            if (parentContentPage == null)
                return;

            foreach (var sharedPortalId in sharedIds)
            {
                // Skip if already shared to this portal
                var alreadyShared = await _context.ContentPageShareds.AnyAsync(x => x.ParentContentPageId == parentContentPageId && x.ParentPortalId == parrentPortalId && x.SharedPortalId == sharedPortalId);
                if (alreadyShared)
                    continue;
     
                // Duplicate content page for target portal
                var newContent = new ContentPage
                {
                    Author = parentContentPage.Author,
                    Banner = parentContentPage.Banner,
                    Content = parentContentPage.Content,
                    DatePosted = parentContentPage.DatePosted,
                    Delete = parentContentPage.Delete,
                    Name = parentContentPage.Name,
                    Language = parentContentPage.Language,
                    Price = parentContentPage.Price,
                    ServiceId = parentContentPage.ServiceId,
                    Status = parentContentPage.Status,
                    Summary = parentContentPage.Summary,
                    Type = parentContentPage.Type,
                    IsHome = parentContentPage.IsHome,
                    StartDate = parentContentPage.StartDate,
                    EndDate = parentContentPage.EndDate,
                    CategoryId = parentContentPage.CategoryId,
                    PortalId = sharedPortalId
                };

                await _context.ContentPages.AddAsync(newContent);
                await _context.SaveChangesAsync(); // get newContent.Id
                // Try to get the parent's Link (on parent portal)
                var parentLink = await _context.Links.AsNoTracking().FirstOrDefaultAsync(x => x.ObjectId == parentContentPageId && x.PortalId == parrentPortalId && x.Type == parentContentPage.Type);
                // Clone Link from parent (if exists) to the new content on target portal
                if (parentLink != null)
                {
                    var originalSlug = parentLink.Slug;
                    // Kiểm tra với portal này slug đã tồn tại chưa
                    var slugExists = await _context.Links.AnyAsync(x => x.Slug == parentLink.Slug && x.PortalId == sharedPortalId && x.Language == parentLink.Language);
                    if (slugExists)
                    {
                        // Thêm hậu tố để tránh trùng lặp
                        originalSlug = $"{originalSlug}-{Guid.NewGuid().ToString().Substring(0, 8)}";
                    }
                    var newLink = new Link
                    {
                        Slug = originalSlug,
                        Name = parentLink.Name,
                        ObjectId = newContent.Id,
                        Language = parentLink.Language,
                        Lastmod = parentLink.Lastmod,
                        Changefreq = parentLink.Changefreq,
                        Priority = parentLink.Priority,
                        IsStatic = parentLink.IsStatic,
                        Type = parentLink.Type,
                        Title = parentLink.Title,
                        Description = parentLink.Description,
                        Keywords = parentLink.Keywords,
                        Status = parentLink.Status,
                        FocusKeywords = parentLink.FocusKeywords,
                        MetaRobotsIndex = parentLink.MetaRobotsIndex,
                        MetaRobotsFollow = parentLink.MetaRobotsFollow,
                        MetaRobotsAdvance = parentLink.MetaRobotsAdvance,
                        IncludeSitemap = parentLink.IncludeSitemap,
                        Redirect301 = parentLink.Redirect301,
                        FacebookDescription = parentLink.FacebookDescription,
                        FacebookBanner = parentLink.FacebookBanner,
                        GooglePlusDescription = parentLink.GooglePlusDescription,
                        Area = parentLink.Area,
                        Controller = parentLink.Controller,
                        Acction = parentLink.Acction,
                        Parrams = parentLink.Parrams,
                        PortalId = sharedPortalId,
                        Delete = false
                    };

                    await _context.Links.AddAsync(newLink);
                    await _context.SaveChangesAsync(); // get newLink.Id
                }

                // Create shared link record
                await _context.ContentPageShareds.AddAsync(new ContentPageShared
                {
                    ParentContentPageId = parentContentPageId,
                    ParentPortalId = parrentPortalId,
                    SharedContentPageId = newContent.Id,
                    SharedPortalId = sharedPortalId
                });

                await _context.SaveChangesAsync();
            }
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
                    CategoryId=x.data.CategoryId,
                    PortalId = x.data.PortalId
                    
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

