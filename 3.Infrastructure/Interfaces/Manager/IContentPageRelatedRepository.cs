using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PT.Domain.Model;

namespace PT.Infrastructure.Interfaces
{
    public interface IContentPageRelatedRepository : IGenericRepository<ContentPageRelated>
    {
        Task<List<ContentPage>> GetContentPageAsync(int contentPageId, int skip = 0, int Take = 0, Expression<Func<ContentPage, bool>> predicate = null, Func<IQueryable<ContentPage>, IOrderedQueryable<ContentPage>> orderBy = null, Expression<Func<ContentPage, ContentPage>> select = null);
    }
}
