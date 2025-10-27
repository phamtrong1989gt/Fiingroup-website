using Microsoft.AspNetCore.Mvc;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PT.Component
{
    // Hiển thị bài viết liên quan
    [ViewComponent(Name = "ContentPageRelated")]
    public class ContentPageRelatedComponent : ViewComponent
    {
        private readonly IContentPageRelatedRepository _contentPageRelatedRepository;

        public ContentPageRelatedComponent(IContentPageRelatedRepository contentPageRelatedRepository)
        {
            _contentPageRelatedRepository = contentPageRelatedRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            int contentPageId,
            string language,
            int take,
            string view,
            string title)
        {
            try
            {
                var items = await _contentPageRelatedRepository.GetContentPageAsync(
                    contentPageId, 0, take,
                    m => m.Status && m.DatePosted <= DateTime.Now && m.Language == language,
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
                        Link = a.Link
                    });

                return View(view, new ComponentModel<ContentPage>
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