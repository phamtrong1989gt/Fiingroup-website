using Microsoft.AspNetCore.Mvc;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PT.Domain.Model.Contact;

namespace PT.Component
{
    // Hiển thị liên hệ hoặc đánh giá
    [ViewComponent(Name = "Contact")]
    public class ContactComponent : ViewComponent
    {
        private readonly IContactRepository _contactRepository;

        public ContactComponent(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            string language,
            int take,
            string view,
            string title,
            ContactType type)
        {
            try
            {
                IEnumerable<Contact> items;
                if (type == ContactType.Testimonial)
                {
                    items = await _contactRepository.SearchAsync(
                        true, 0, take,
                        m => !m.Delete && m.Type == type && m.Language == language && m.IsHome,
                        m => m.OrderByDescending(x => x.CreatedDate));
                }
                else
                {
                    items = await _contactRepository.SearchAsync(
                        true, 0, take,
                        m => !m.Delete && m.Type == type && m.Language == language,
                        m => m.OrderByDescending(x => x.CreatedDate));
                }

                return View(view, new ComponentModel<Contact>
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