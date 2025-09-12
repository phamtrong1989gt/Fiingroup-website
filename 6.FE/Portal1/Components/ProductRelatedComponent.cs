using Microsoft.AspNetCore.Mvc;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.UI.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PT.Component
{

    // Hiển thị sản phẩm liên quan
    [ViewComponent(Name = "ProductRelated")]
    public class ProductRelatedComponent : ViewComponent
    {
        private readonly IProductRepository _productRepository;

        public ProductRelatedComponent(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(ProductComponentModel model)
        {
            try
            {
                var listNew = await _productRepository.SearchPagedListAsync(
                    1, model.Take, null,
                    m => m.Status && !m.Delete && m.Language == model.Language && m.Id != model.CurrentId,
                    m => m.OrderByDescending(x => x.Name),
                    a => new Product
                    {
                        Banner = a.Banner,
                        Id = a.Id,
                        Name = a.Name,
                        Language = a.Language,
                        Link = a.Link,
                        Status = a.Status,
                        Delete = a.Delete
                    });

                return View(model.View, new ComponentModel<Product>
                {
                    Items = listNew.Data,
                    Language = model.Language,
                    Take = model.Take,
                    Title = model.Title,
                    View = model.View
                });
            }
            catch
            {
                return View();
            }
        }
    }
}