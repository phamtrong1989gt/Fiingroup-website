using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;

namespace PT.Infrastructure.Repositories
{
    public class ServicePriceRepository : BaseRepository<ServicePrice>, IServicePriceRepository
    {
        private readonly ApplicationContext _context;
        public ServicePriceRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }
        //public  async Task<BaseSearchModel<List<ServicePrice>>> SearchPagedListAsync(int page, int limit,int? categoryId, Expression<Func<ServicePrice, bool>> predicate = null, Func<IQueryable<ServicePrice>, IOrderedQueryable<ServicePrice>> orderBy = null, Expression<Func<ServicePrice, ServicePrice>> select = null, params Expression<Func<ServicePrice, object>>[] includeProperties)
        //{
        //    IQueryable<ServicePrice> query = _context.ServicePrices.AsQueryable();
        //    if (predicate != null)
        //    {
        //        query = query.Where(predicate).AsQueryable();
        //    }
        //    if (categoryId != null)
        //    {
        //        query = query.Where(x => x.CategoryId==categoryId || _context.Categorys.Any(m=>m.ParentId==categoryId)).AsQueryable();
        //    }
        //    if (orderBy != null)
        //    {
        //        query = orderBy(query).AsQueryable();
        //    }
        //    if (select != null)
        //    {
        //        query = query.Select(select).AsQueryable();
        //    }
            
        //    query = query
        //        .GroupJoin(_context.Categorys.Where(x => (x.Type == CategoryType.CategoryService)).AsQueryable(), x => x.CategoryId, y => y.Id, (x, y) => new { data = x, categorys = y })
        //        .SelectMany(x => x.categorys.DefaultIfEmpty(), (x, y) => new ServicePrice
        //        {
        //            Id = x.data.Id,
        //            Delete = x.data.Delete,
        //            Name = x.data.Name,
        //            Language = x.data.Language,
        //            Status = x.data.Status,
        //            CategoryId = x.data.CategoryId,
        //            FromPrice =x.data.FromPrice,
        //            Order =x.data.Order,
        //            ToPrice =x.data.ToPrice,
        //            Unit =x.data.Unit,
        //            Visits =x.data.Visits,
        //            Category = y,
        //            Type = x.data.Type
        //        }).AsQueryable();

        //    return new BaseSearchModel<List<ServicePrice>>
        //    {
        //        Data = await query.Skip((page - 1) * limit).Take(limit).AsNoTracking().ToListAsync(),
        //        Limit = limit,
        //        Page = page,
        //        TotalRows = await query.CountAsync()
        //    };
        //}
    }
}

