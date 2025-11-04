using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PT.Domain.Model;

namespace PT.Infrastructure.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<List<CategoryTreeModel>> GetAll(string language, int portalId);
       int  MaxOrder(Expression<Func<Category, bool>> predicate = null);
        Task<List<Category>> FindByLinkReference(int skip = 0, int Take = 0, Expression<Func<Category, bool>> predicate = null, Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null, Expression<Func<Category, Category>> select = null);
        Task<List<Category>> SearchAsync(
           bool asNoTracking = false,
           int skip = 0,
           int Take = 0,
           Expression<Func<Category, bool>> predicate = null,
           Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null,
           Expression<Func<Category, Category>> select = null,
           bool anyContent = false);
        List<PT.Domain.Model.CategoryTreeModel> GetCategoryAncestors(PT.Domain.Model.CategoryTreeModel category, List<PT.Domain.Model.CategoryTreeModel> allCategories);
        List<ECategoryType> GetCategoryPrefixedTypes();
        CategoryType CategoryTypeCategoryToContentPage(CategoryType categoryType);
    }
}
