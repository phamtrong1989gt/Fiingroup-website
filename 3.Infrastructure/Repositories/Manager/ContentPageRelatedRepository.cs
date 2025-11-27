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
        public async Task<List<ContentPage>> GetContentPageAsync(
             int contentPageId,
             int skip = 0,
             int Take = 0,
             Expression<Func<ContentPage, bool>> predicate = null,
             Func<IQueryable<ContentPage>, IOrderedQueryable<ContentPage>> orderBy = null,
             Expression<Func<ContentPage, ContentPage>> select = null)
        {
            // ✅ QUERY MỚI: SELECT ContentPages → JOIN ContentPageRelateds → JOIN Links
            var query = (from cp in _context.ContentPages.AsNoTracking()
                             // ✅ Điều kiện: Tồn tại trong ContentPageRelateds với ParentId
                         where _context.ContentPageRelateds.Any(cpr =>
                             cpr.ContentPageId == cp.Id &&
                             cpr.ParentId == contentPageId)
                         // ✅ Join với ContentPageRelateds để lấy Order
                         join cpr in _context.ContentPageRelateds.AsNoTracking()
                             on new { Id = cp.Id, ParentId = contentPageId }
                             equals new { Id = cpr.ContentPageId, ParentId = cpr.ParentId }
                             // ✅ Join với Links để lấy Link
                         join link in _context.Links.AsNoTracking()
                                 .Where(l => l.Type == ESlugType.ContentPage && !l.Delete)
                             on cp.Id equals link.ObjectId into linkGroup
                         from link in linkGroup.DefaultIfEmpty()
                             // ✅ Sắp xếp theo Order từ ContentPageRelateds
                         orderby cpr.Order ?? 0
                         select new
                         {
                             ContentPage = cp,
                             RelatedOrder = cpr.Order ?? 0,
                             RelatedNote = cpr.Note,
                             Link = link
                         }).AsQueryable();

            // ✅ Apply pagination
            if (Take > 0)
            {
                query = query.Skip(skip < 0 ? 0 : skip).Take(Take);
            }

            // ✅ Map sang ContentPage với Order từ ContentPageRelateds
            var mappedQuery = query.Select(x => new ContentPage
            {
                // Primary properties
                Id = x.ContentPage.Id,
                Name = x.ContentPage.Name,
                Banner = x.ContentPage.Banner,
                Content = x.ContentPage.Content,
                Summary = x.ContentPage.Summary,
                Author = x.ContentPage.Author,
                DatePosted = x.ContentPage.DatePosted,
                Language = x.ContentPage.Language,
                Status = x.ContentPage.Status,
                Type = x.ContentPage.Type,

                // Foreign keys
                CategoryId = x.ContentPage.CategoryId,
                ServiceId = x.ContentPage.ServiceId,
                PortalId = x.ContentPage.PortalId,

                // Enum properties
                CategoryType = x.ContentPage.CategoryType,
                SlugType = x.ContentPage.SlugType,

                // Additional string properties
                Tags = x.ContentPage.Tags,
                Address = x.ContentPage.Address,
                Pages = x.ContentPage.Pages,
                Topic = x.ContentPage.Topic,
                Extentions = x.ContentPage.Extentions,
                FilePath = x.ContentPage.FilePath,
                TimeFromTo = x.ContentPage.TimeFromTo,

                // Decimal properties
                Price = x.ContentPage.Price,

                // DateTime properties
                StartDate = x.ContentPage.StartDate,
                EndDate = x.ContentPage.EndDate,
                DeliveryTime = x.ContentPage.DeliveryTime,

                // Boolean properties
                IsHome = x.ContentPage.IsHome,
                IsShowHeaderContent = x.ContentPage.IsShowHeaderContent,

                // Input fields (Input1 - Input12)
                Input1 = x.ContentPage.Input1,
                Input2 = x.ContentPage.Input2,
                Input3 = x.ContentPage.Input3,
                Input4 = x.ContentPage.Input4,
                Input5 = x.ContentPage.Input5,
                Input6 = x.ContentPage.Input6,
                Input7 = x.ContentPage.Input7,
                Input8 = x.ContentPage.Input8,
                Input9 = x.ContentPage.Input9,
                Input10 = x.ContentPage.Input10,
                Input11 = x.ContentPage.Input11,
                Input12 = x.ContentPage.Input12,

                // ✅ QUAN TRỌNG: Order từ ContentPageRelateds (GHI ĐÈ Order của ContentPage)
                Order = x.RelatedOrder,

                // ✅ Navigation property: Link đã join sẵn
                Link = x.Link,
                RelatedNote = x.RelatedNote
            });

            // ✅ Apply select projection (nếu có)
            if (select != null)
            {
                mappedQuery = mappedQuery.Select(select);
            }

            // ✅ Execute query và trả về List<ContentPage>
            return await mappedQuery.ToListAsync();
        }
    }
}

