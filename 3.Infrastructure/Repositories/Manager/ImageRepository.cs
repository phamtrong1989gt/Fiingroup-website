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
    public class ImageRepository : BaseRepository<Image>, IImageRepository
    {
        private readonly ApplicationContext _context;
        public ImageRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }
      
        public override async Task<BaseSearchModel<List<Image>>> SearchPagedListAsync(int page, int limit, Expression<Func<Image, bool>> predicate = null, Func<IQueryable<Image>, IOrderedQueryable<Image>> orderBy = null, Expression<Func<Image, Image>> select = null, params Expression<Func<Image, object>>[] includeProperties)
        {
            IQueryable<Image> query = _context.Images.AsQueryable();
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

            query = query
                .GroupJoin(_context.Categorys.Where(x => (x.Type == CategoryType.CategoryService)).AsQueryable(), x => x.CategoryId, y => y.Id, (x, y) => new { data = x, categorys = y })
                .SelectMany(x => x.categorys.DefaultIfEmpty(), (x, y) => new Image
                {
                    Id = x.data.Id,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    CategoryId = x.data.CategoryId,
                    Category = y,
                    Image1 = x.data.Image1,
                    Image2=x.data.Image2,
                    ImageGalleryId = x.data.ImageGalleryId
                }).AsQueryable();

            return new BaseSearchModel<List<Image>>
            {
                Data = await query.Skip((page - 1) * limit).Take(limit).AsNoTracking().ToListAsync(),
                Limit = limit,
                Page = page,
                TotalRows = await query.CountAsync()
            };
        }
    }
}

