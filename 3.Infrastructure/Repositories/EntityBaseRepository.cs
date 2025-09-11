using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PT.Domain.Model;
using PT.Domain.Seedwork;
using PT.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PT.Infrastructure.Repositories
{
	public class BaseRepository<T> : IGenericRepository<T>
			where T : class, IAggregateRoot, new()
	{
		private readonly ApplicationContext _context;

		public IUnitOfWork UnitOfWork
		{
			get
			{
				return _context;
			}
		}

		#region Properties
		public BaseRepository(ApplicationContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}
		#endregion

		public virtual async Task<BaseSearchModel<List<T>>> SearchPagedListAsync(int page, int limit, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Expression<Func<T, T>> select = null, params Expression<Func<T, object>>[] includeProperties)
		{
			IQueryable<T> query = _context.Set<T>().AsNoTracking();
			
			
            if (predicate != null)
            {
                query = _context.Set<T>().Where(predicate).AsQueryable();
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
            }
            var testLisst = query.ToList();
            var list = await query.Skip((page - 1) * limit).Take(limit).AsNoTracking().ToListAsync();

            var count = await query.CountAsync();

            return new BaseSearchModel<List<T>>
			{
				Data = list,
				Limit = limit,
				Page = page,
				TotalRows = count
            };
		}
		
        public virtual async Task<List<T>> SearchAsync(bool asNoTracking= false, int skip = 0, int Take = 0, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Expression<Func<T, T>> select = null, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _context.Set<T>();

            if (predicate != null)
            {
                query = _context.Set<T>().Where(predicate).AsQueryable();
            }
            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            if(Take>0)
            {
                query = query.Skip(skip < 0 ? 0 : skip).Take(Take).AsQueryable();
            }

            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
            if(asNoTracking)
            {
                query = query.AsNoTracking();
            }
            return await query.ToListAsync();
        }

		public virtual async Task<T> SingleOrDefaultAsync(bool asNoTracking = false, Expression<Func<T, bool>> predicate = null,  params Expression<Func<T, object>>[] includeProperties)
		{
            IQueryable<T> query = _context.Set<T>();
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
            return await query.FirstOrDefaultAsync(predicate);
        }
		
		public T Add(T entity)
		{
			return _context.Set<T>().Add(entity).Entity;
		}

		public virtual async Task AddAsync(T entity)
		{
			EntityEntry dbEntityEntry = _context.Entry<T>(entity);
			await _context.Set<T>().AddAsync(entity);
		}

		public virtual async Task AddRangeAsync(List<T> entity)
		{
			EntityEntry dbEntityEntry = _context.Entry<List<T>>(entity);
			await _context.Set<T>().AddRangeAsync(entity);
		}

		public virtual void Update(T entity)
		{
			_context.Entry(entity).State = EntityState.Modified;
		}

		public virtual void Delete(T entity)
		{
			_context.Entry(entity).State = EntityState.Deleted;
		}
        public virtual DatabaseFacade Database()
        {
            return _context.Database;
        }

        public virtual void DeleteWhere(Expression<Func<T, bool>> predicate)
		{
			IEnumerable<T> entities = _context.Set<T>().Where(predicate);
            foreach(var item in entities)
            {
                _context.Entry<T>(item).State = EntityState.Deleted;
            }
		}
        public virtual async Task BeginTransaction()
        {
            await _context.Database.BeginTransactionAsync();
        }
        public virtual async Task CommitTransaction()
        {
            await _context.Database.CommitTransactionAsync();
        }
        public virtual async Task CommitAsync()
		{
			await UnitOfWork.SaveEntitiesAsync();
		}
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            IQueryable<T> query = _context.Set<T>();
            return await query.AnyAsync(predicate);
        }
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            IQueryable<T> query = _context.Set<T>();
            if (predicate != null)
            {
                query = _context.Set<T>().Where(predicate).AsQueryable();
            }
            return await query.CountAsync();
        }
        
    }
}
