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
    public class ContactRepository : BaseRepository<Contact>, IContactRepository
    {
        private readonly ApplicationContext _context;
        public ContactRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }
        public override async Task<BaseSearchModel<List<Contact>>> SearchPagedListAsync(int page, int limit, Expression<Func<Contact, bool>> predicate = null, Func<IQueryable<Contact>, IOrderedQueryable<Contact>> orderBy = null, Expression<Func<Contact, Contact>> select = null, params Expression<Func<Contact, object>>[] includeProperties)
        {
            IQueryable<Contact> query = _context.Contacts.AsQueryable();
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
                .GroupJoin(_context.Employees, x => x.EmployeeId, y => y.Id, (x, y) => new { data = x, employees = y })
                    .SelectMany(x => x.employees.DefaultIfEmpty(), (x, y) => new { x.data, employee = y })
                    .GroupJoin(_context.ContentPages, x => x.data.ServiceId, y => y.Id, (x, y) => new { x.data, x.employee, services = y })
                    .SelectMany(x => x.services.DefaultIfEmpty(), (x, y) => new { x.data, x.employee, service = y })
                    .GroupJoin(_context.Customers, x => x.data.CustomerId, y => y.Id, (x, y) => new { x.data, x.employee, x.service, customers = y })
                .SelectMany(x => x.customers.DefaultIfEmpty(), (x, y) => new Contact
                {
                   Id = x.data.Id,
                   CustomerId =x.data.CustomerId,
                   ServiceId = x.data.ServiceId,
                   Service= x.service,
                   Address =x.data.Address,
                   Age =x.data.Age,
                   AppointmentDate=x.data.AppointmentDate,
                   AppointmentStatus=x.data.AppointmentStatus,
                   Content=x.data.Content,
                   CreatedDate=x.data.CreatedDate,
                   Customer=y,
                   Delete=x.data.Delete,
                   Email=x.data.Email,
                   Employee= x.employee,
                   EmployeeId=x.data.EmployeeId,
                   FullName=x.data.FullName,
                   Language=x.data.Language,
                   Note=x.data.Note,
                   Phone=x.data.Phone,
                   Status=x.data.Status,
                   Type=x.data.Type,
                   AppointmentDateTo = x.data.AppointmentDateTo,
                   Rating =x.data.Rating,
                   IsHome = x.data.IsHome,
                   Avatar= x.data.Avatar,
                   CountryId = x.data.CountryId,
                   PhoneCode=x.data.PhoneCode
                }).AsQueryable();

            var list = await query.Skip((page - 1) * limit).Take(limit).AsNoTracking().ToListAsync();
            return new BaseSearchModel<List<Contact>>
            {
                Data = list,
                Limit = limit,
                Page = page,
                TotalRows = await query.CountAsync()
            };
        }
    }
}

