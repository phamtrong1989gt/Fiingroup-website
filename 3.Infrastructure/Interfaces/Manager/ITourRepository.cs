using PT.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PT.Infrastructure.Interfaces
{
    public interface ITourRepository : IGenericRepository<Tour>
    {
        Task RemoveCategory(int tourId);
        Task<List<Tour>> SearchAsync(int skip, int take, int? categoryId, Expression<Func<Tour, bool>> predicate = null, Func<IQueryable<Tour>, IOrderedQueryable<Tour>> orderBy = null, Expression<Func<Tour, Tour>> select = null);
        Task<BaseSearchModel<List<Tour>>> SearchPagedListAsync(
            int page,
            int limit,
            int[] categoryIds,
            string[] days,
            int[] tourTypes,
            Expression<Func<Tour, bool>> predicate = null,
            Func<IQueryable<Tour>, IOrderedQueryable<Tour>> orderBy = null,
            Expression<Func<Tour, Tour>> select = null);
        Task<BaseSearchModel<List<Tour>>> SearchPagedListAsync(int page, int limit, int? categoryId, Expression<Func<Tour, bool>> predicate = null, Func<IQueryable<Tour>, IOrderedQueryable<Tour>> orderBy = null, Expression<Func<Tour, Tour>> select = null, params Expression<Func<Tour, object>>[] includeProperties);
    }
}
