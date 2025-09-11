using System;
using System.Linq.Expressions;
using PT.Domain.Model;

namespace PT.Infrastructure.Interfaces
{
    public interface IBannerItemRepository : IGenericRepository<BannerItem>
    {
        int MaxOrder(Expression<Func<BannerItem, bool>> predicate = null);
    }
}
