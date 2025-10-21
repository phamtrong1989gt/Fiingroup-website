using PT.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PT.Infrastructure.Interfaces
{
    public interface IContentPageRepository : IGenericRepository<ContentPage>
    {

        Task ContentPageSharedRefeshContent(int contentPageId);
        Task ContentPageSharedAdds(int parentContentPageId, int parrentPortalId, List<int> sharedIds);
        Task<List<ContentPageShared>> ContentPageSharedGets(int contentPageId);
        Task<BaseSearchModel<List<ContentPage>>> FAQSearchPagedListAsync(int page, int limit, int? categoryId, int? tagId, Expression<Func<ContentPage, bool>> predicate = null, Func<IQueryable<ContentPage>, IOrderedQueryable<ContentPage>> orderBy = null, Expression<Func<ContentPage, ContentPage>> select = null, params Expression<Func<ContentPage, object>>[] includeProperties);
        Task<List<ContentPage>> SearchAdvanceAsync(CategoryType type, int page, int limit, int? categoryId, int? tagId, Expression<Func<ContentPage, bool>> predicate = null, Func<IQueryable<ContentPage>, IOrderedQueryable<ContentPage>> orderBy = null, Expression<Func<ContentPage, ContentPage>> select = null);
        Task<BaseSearchModel<List<ContentPage>>> SearchPagedListAsync(int page, int limit, int? categoryId, int? tagId, Expression<Func<ContentPage, bool>> predicate = null, Func<IQueryable<ContentPage>, IOrderedQueryable<ContentPage>> orderBy = null, Expression<Func<ContentPage, ContentPage>> select = null, params Expression<Func<ContentPage, object>>[] includeProperties);
    }
}
