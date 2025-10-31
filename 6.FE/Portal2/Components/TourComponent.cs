using Microsoft.AspNetCore.Mvc;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PT.Component
{
    // Hiển thị danh sách tour
    [ViewComponent(Name = "Tour")]
    public class TourComponent : ViewComponent
    {
        private readonly ITourRepository _tourRepository;

        public TourComponent(ITourRepository tourRepository)
        {
            _tourRepository = tourRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            string language,
            int take,
            string view,
            Expression<Func<Tour, bool>> predicate = null,
            Func<IQueryable<Tour>, IOrderedQueryable<Tour>> orderBy = null,
            Expression<Func<Tour, Tour>> select = null,
            string title = null)
        {
            try
            {
                var items = await _tourRepository.SearchAsync(0, take, null, predicate, orderBy, select);
                return View(view, new ComponentModel<Tour>
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