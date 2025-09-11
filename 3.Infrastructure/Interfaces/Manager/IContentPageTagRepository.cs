using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PT.Domain.Model;

namespace PT.Infrastructure.Interfaces
{
    public interface IContentPageTagRepository : IGenericRepository<ContentPageTag>
    {
        Task<List<Tag>> GetTag(int skip = 0, int Take = 0, int pageContentId = 0, Expression<Func<Tag, bool>> predicate = null, Func<IQueryable<Tag>, IOrderedQueryable<Tag>> orderBy = null);
    }
}
