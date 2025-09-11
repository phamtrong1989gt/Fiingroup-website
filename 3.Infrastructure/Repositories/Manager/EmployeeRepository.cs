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
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        private readonly ApplicationContext _context;
        public EmployeeRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }
        public override async Task<BaseSearchModel<List<Employee>>> SearchPagedListAsync(int page, int limit, Expression<Func<Employee, bool>> predicate = null, Func<IQueryable<Employee>, IOrderedQueryable<Employee>> orderBy = null, Expression<Func<Employee, Employee>> select = null, params Expression<Func<Employee, object>>[] includeProperties)
        {
            IQueryable<Employee> query = _context.Employees.AsNoTracking().AsQueryable();
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
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }
            query = query
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.Employee), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Employee
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Delete = x.data.Delete,
                    Status = x.data.Status,
                    Email = x.data.Email,
                    FullName = x.data.FullName,
                    Gender = x.data.Gender,
                    Phone = x.data.Phone,
                    Endodontics =x.data.Endodontics,
                    Degrees=x.data.Degrees,
                    Facebook=x.data.Facebook,
                    Job=x.data.Job,
                    Language=x.data.Language,
                    Office=x.data.Office,
                    EmployeeMappingId = x.data.EmployeeMappingId,
                    GeneralDentistry = x.data.GeneralDentistry,
                    OralMedicine =x.data.OralMedicine,
                    OralSurgery=x.data.OralSurgery,
                    Orthodontics=x.data.Orthodontics,
                    Periodontics=x.data.Periodontics,
                    Prosthodontics =x.data.Prosthodontics
            }).AsQueryable();
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
            var list = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();
            return new BaseSearchModel<List<Employee>>
            {
                Data = list,
                Limit = limit,
                Page = page,
                TotalRows = await query.CountAsync()
            };
        }
        public override async Task<List<Employee>> SearchAsync(bool asNoTracking = false, int skip = 0, int Take = 0, Expression<Func<Employee, bool>> predicate = null, Func<IQueryable<Employee>, IOrderedQueryable<Employee>> orderBy = null, Expression<Func<Employee, Employee>> select = null, params Expression<Func<Employee, object>>[] includeProperties)
        {
            IQueryable<Employee> query = _context.Employees.AsNoTracking().AsQueryable();
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
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }
            query = query
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.Employee), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Employee
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Delete = x.data.Delete,
                    Status = x.data.Status,
                    Email = x.data.Email,
                    FullName = x.data.FullName,
                    Gender = x.data.Gender,
                    Phone = x.data.Phone,
                    Endodontics = x.data.Endodontics,
                    Degrees = x.data.Degrees,
                    Facebook = x.data.Facebook,
                    Job = x.data.Job,
                    Language = x.data.Language,
                    Office = x.data.Office,
                    EmployeeMappingId = x.data.EmployeeMappingId,
                    GeneralDentistry = x.data.GeneralDentistry,
                    OralMedicine = x.data.OralMedicine,
                    OralSurgery = x.data.OralSurgery,
                    Orthodontics = x.data.Orthodontics,
                    Periodontics = x.data.Periodontics,
                    Prosthodontics = x.data.Prosthodontics,
                    Summary =x.data.Summary
                }).AsQueryable();

            if (Take > 0)
            {
                query = query.Skip(skip < 0 ? 0 : skip).Take(Take).AsQueryable();
            }

            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
            return await query.ToListAsync();
        }
    }
}

