using Microsoft.AspNetCore.Mvc;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.UI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PT.Component
{
    // Hiển thị danh sách ContentPage
    [ViewComponent(Name = "ContentPage")]
    public class ContentPageComponent : ViewComponent
    {
        private readonly IContentPageRepository _contentPageRepository;

        public ContentPageComponent(IContentPageRepository contentPageRepository)
        {
            _contentPageRepository = contentPageRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(ContentPageComponentModel model)
        {
            try
            {
                var items = await _contentPageRepository.SearchAdvanceAsync(
                    model.Type, 0, model.Take, model.CategoryId, model.TagId,
                    m => m.Status 
                        && m.DatePosted <= DateTime.Now
                        && m.Language == model.Language
                        && m.Type == model.Type
                        && (m.Id != model.ContentPageId || model.ContentPageId == null),
                    m => m.OrderByDescending(x => x.DatePosted),
                    a => new ContentPage
                    {
                        Banner = a.Banner,
                        DatePosted = a.DatePosted,
                        Id = a.Id,
                        Name = a.Name,
                        Summary = a.Summary,
                        Language = a.Language,
                        Author = a.Author,
                        CategoryId = a.CategoryId,
                        Link = a.Link
                    });

                return View(model.View, new ComponentModel<ContentPage>
                {
                    Items = items,
                    CategoryId = model.CategoryId,
                    Language = model.Language,
                    Take = model.Take,
                    Title = model.Title,
                    Href = model.Href,
                    View = model.View
                });
            }
            catch
            {
                return Content("ERROR");
            }
        }
    }
}