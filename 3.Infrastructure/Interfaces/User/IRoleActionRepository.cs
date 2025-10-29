using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PT.Infrastructure;
using PT.Domain.Model;

namespace PT.Infrastructure.Interfaces
{
    public interface IRoleActionRepository : IGenericRepository<RoleAction>
    {
        Task<BaseSearchModel<List<RoleAction>>> SearchPagedListAdvance(int page, int limit, Expression<Func<RoleAction, bool>> predicate = null, Func<IQueryable<RoleAction>, IOrderedQueryable<RoleAction>> orderBy = null, Expression<Func<RoleAction, RoleAction>> select = null);
    }
}

