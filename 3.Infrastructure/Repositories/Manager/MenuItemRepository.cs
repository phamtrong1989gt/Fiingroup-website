using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace PT.Infrastructure.Repositories
{
    public class MenuItemRepository : BaseRepository<MenuItem>, IMenuItemRepository
    {
        private readonly ApplicationContext _db;
        public MenuItemRepository(ApplicationContext context) : base(context)
        {
            _db = context;
        }
        public int MaxOrder(Expression<Func<MenuItem, bool>> predicate = null)
        {
            var querry = _db.MenuItems.AsQueryable();
            if (querry != null)
            {
                querry = querry.Where(predicate).AsQueryable();
            }
            if (querry.Any())
            {
                return querry.Max(x => x.Order);
            }
            else
            {
                return 0;
            }
        }
    }
}

