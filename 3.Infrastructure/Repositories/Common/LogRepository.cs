using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using PT.Domain.Seedwork;
using PT.Domain.Model;
using System.Threading.Tasks;
using PT.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Linq.Expressions;

namespace PT.Infrastructure.Repositories
{
    public class LogRepository : BaseRepository<Log>, ILogRepository
    {
        private readonly ApplicationContext _db;
        public LogRepository(ApplicationContext context) : base(context)
        {
            _db = context;
        }
        //public async override Task<BaseSearchModel<List<Log>>> SearchPagedList(int page, int limit, Expression<Func<Log, bool>> predicate = null, Func<IQueryable<Log>, IOrderedQueryable<Log>> orderBy = null, Expression<Func<Log, Log>> select = null, params Expression<Func<Log, object>>[] includeProperties)
        //{
        //    IQueryable<Log> query = _context.Logs.AsQueryable();
        //    if (predicate != null)
        //    {
        //        query = _context.Set<Log>().Where(predicate).AsQueryable();
        //    }
        //    if (orderBy != null)
        //    {
        //        query = orderBy(query).AsQueryable();
        //    }
        //    if (includeProperties != null)
        //    {
        //        foreach (var includeProperty in includeProperties)
        //        {
        //            query = query.Include(includeProperty);
        //        }
        //    }
        //    if (select != null)
        //    {
        //        query = query.Select(select).AsQueryable();
        //    }
        //    var newQuerry = (from q in query select q).Take(page * (limit + 1)).AsQueryable();
        //    return new BaseSearchModel<List<Log>>
        //    {
        //        Data = await newQuerry.Skip((page - 1) * limit).Take(limit).AsNoTracking().ToListAsync(),
        //        Limit = limit,
        //        Page = page,
        //        ReturnUrl = "",
        //        TotalRows = await newQuerry.AsNoTracking().CountAsync()
        //    };
        //}
    }
}
