using Microsoft.AspNetCore.Mvc;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PT.Component
{
    // Hiển thị bài viết nâng cao
    [ViewComponent(Name = "ContentPageAdvance")]
    public class ContentPageAdvanceComponent : ViewComponent
    {
        private readonly IContentPageRepository _contentPageRepository;

        public ContentPageAdvanceComponent(IContentPageRepository contentPageRepository)
        {
            _contentPageRepository = contentPageRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            int take,
            string view,
            string title,
            int? categoryId,
            int? tagId,
            CategoryType type,
            Expression<Func<ContentPage, bool>> predicate = null,
            Func<IQueryable<ContentPage>, IOrderedQueryable<ContentPage>> orderBy = null,
            Expression<Func<ContentPage, ContentPage>> select = null)
        {
            try
            {
                var items = await _contentPageRepository.SearchAdvanceAsync(type, 0, take, categoryId, tagId, predicate, orderBy, select);
                return View(view, new ComponentModel<ContentPage>
                {
                    Items = items,
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