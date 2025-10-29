using PT.Domain.Model;
using PT.Infrastructure;
using System.Threading.Tasks;
using PT.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace PT.Infrastructure.Repositories
{
    public class RoleActionRepository : BaseRepository<RoleAction>, IRoleActionRepository
    {
        private readonly ApplicationContext db;
        public RoleActionRepository(ApplicationContext context) : base(context)
        {
            db = context;
        }
        public async Task<BaseSearchModel<List<RoleAction>>> SearchPagedListAdvance(int page, int limit, Expression<Func<RoleAction, bool>> predicate = null, Func<IQueryable<RoleAction>, IOrderedQueryable<RoleAction>> orderBy = null, Expression<Func<RoleAction, RoleAction>> select = null)
        {
            IQueryable<RoleAction> query = db.RoleActions.AsQueryable();
            if (predicate != null)
            {
                query = db.RoleActions.Where(predicate).AsQueryable();
            }
            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }
            query = query.Include(x => x.RoleController).ThenInclude(x => x.RoleArea);
            query = query.Include(x => x.RoleController).ThenInclude(x => x.RoleGroups);
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }

            return new BaseSearchModel<List<RoleAction>>
            {
                Data = await query.Skip((page - 1) * limit).Take(limit).ToListAsync(),
                Limit = limit,
                Page = page,
                ReturnUrl = "",
                TotalRows = await query.Select(m => 1).CountAsync()
            };
        }
    }
}

