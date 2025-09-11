using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PT.Domain.Model;
using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PT.Infrastructure
{
    public interface IGenericRepository<T>: IRepository<T> where T : class, IAggregateRoot, new ()
    {
        Task<BaseSearchModel<List<T>>> SearchPagedListAsync(int page, int limit, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Expression<Func<T, T>> select = null, params Expression<Func<T, object>>[] includeProperties);
        Task<List<T>> SearchAsync(bool asNoTracking = false, int skip = 0, int Take = 0, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Expression<Func<T, T>> select = null, params Expression<Func<T, object>>[] includeProperties);
        Task<T> SingleOrDefaultAsync(bool asNoTracking = false, Expression < Func<T, bool>> predicate =null, params Expression<Func<T, object>>[] includeProperties);
        T Add(T entity);
        Task AddAsync(T entity);
        Task AddRangeAsync(List<T> entity);
        void Update(T entity);
        void Delete(T entity);
        void DeleteWhere(Expression<Func<T, bool>> predicate);
        Task CommitAsync();
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
        DatabaseFacade Database();
        Task BeginTransaction();
        Task CommitTransaction();
    }
}
