using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace PT.Infrastructure.Repositories
{
    public class BannerItemRepository : BaseRepository<BannerItem>, IBannerItemRepository
    {
        private readonly ApplicationContext _db;
        public BannerItemRepository(ApplicationContext context) : base(context)
        {
            _db = context;
        }
        public int MaxOrder(Expression<Func<BannerItem, bool>> predicate = null)
        {
            var querry = _db.BannerItems.AsQueryable();
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

