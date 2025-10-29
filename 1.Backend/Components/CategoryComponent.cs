using Microsoft.AspNetCore.Mvc;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PT.Component
{
    // Hiển thị danh mục
    [ViewComponent(Name = "Category")]
    public class CategoryComponent : ViewComponent
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryComponent(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            string language,
            int take,
            string view,
            string title,
            Expression<Func<Category, bool>> predicate = null,
            Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null,
            Expression<Func<Category, Category>> select = null)
        {
            try
            {
                var items = await _categoryRepository.SearchAsync(true, 0, take, predicate, orderBy, select);
                return View(view, new ComponentModel<Category>
                {
                    Items = items,
                    Language = language,
                    Take = take,
                    Title = title,
                    View = view
                });
            }
            catch
            {
                return View();
            }
        }
    }
}