using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PT.Domain.Model;

namespace PT.Infrastructure.Interfaces
{
    public interface IServicePriceRepository : IGenericRepository<ServicePrice>
    {
        //Task<BaseSearchModel<List<ServicePrice>>> SearchPagedListAsync(int page, int limit, int? categoryId, Expression<Func<ServicePrice, bool>> predicate = null, Func<IQueryable<ServicePrice>, IOrderedQueryable<ServicePrice>> orderBy = null, Expression<Func<ServicePrice, ServicePrice>> select = null, params Expression<Func<ServicePrice, object>>[] includeProperties);
    }
}
