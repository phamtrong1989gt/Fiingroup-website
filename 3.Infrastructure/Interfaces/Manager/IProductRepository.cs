using Microsoft.EntityFrameworkCore;
using PT.Domain.Model;
using PT.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PT.Infrastructure.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<BaseSearchModel<List<Product>>> SearchPagedListAsync(int page, int limit, int? categoryId, Expression<Func<Product, bool>> predicate = null, Func<IQueryable<Product>, IOrderedQueryable<Product>> orderBy = null, Expression<Func<Product, Product>> select = null, params Expression<Func<Product, object>>[] includeProperties);
    }

    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        private readonly ApplicationContext _context;
        public ProductRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<BaseSearchModel<List<Product>>> SearchPagedListAsync(
            int page, int limit, int? categoryId,
            Expression<Func<Product, bool>> predicate = null,
            Func<IQueryable<Product>, IOrderedQueryable<Product>> orderBy = null,
            Expression<Func<Product, Product>> select = null,
            params Expression<Func<Product, object>>[] includeProperties)
        {
            var query = _context.Products.AsNoTracking();

            if (predicate != null)
                query = query.Where(predicate);

            if (categoryId.HasValue)
            {
                var productIds = _context.ProductCategorys
                    .Where(pc => pc.CategoryId == categoryId.Value)
                    .Select(pc => pc.ProductId);
                query = query.Where(p => productIds.Contains(p.Id));
            }

            if (orderBy != null)
                query = orderBy(query);

            if (select != null)
                query = query.Select(select);

            var totalRows = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            // Lấy danh sách ProductCategory và Category liên quan
            var productIdsPage = products.Select(x => x.Id).ToList();

            var productCategories = await _context.ProductCategorys
                .Where(pc => productIdsPage.Contains(pc.ProductId))
                .ToListAsync();

            var categoryIds = productCategories.Select(pc => pc.CategoryId).Distinct().ToList();
            var categories = await _context.Categorys
                .Where(c => categoryIds.Contains(c.Id))
                .ToListAsync();


            var links = await _context.Links
                .Where(l => productIdsPage.Contains(l.ObjectId) && l.Type == CategoryType.Product)
                .ToListAsync();

            // Lấy LinkReferences nếu cần (giữ nguyên logic cũ)
            var linkIds = links.Select(x=>x.Id).ToList();
            var linkReferences = await _context.LinkReferences
                .Where(lr => linkIds.Contains(lr.LinkId1))
                .ToListAsync();

            foreach (var product in products)
            {
                product.Link = links.FirstOrDefault(l => l.ObjectId == product.Id);

                product.LinkReferences = linkReferences
                    .Where(lr => lr.LinkId1 == product.Link?.Id)
                    .ToList();

                var cats = productCategories
                 .Where(pc => pc.ProductId == product.Id)
                 .Select(pc => categories.FirstOrDefault(c => c.Id == pc.CategoryId))
                 .Where(c => c != null)
                 .ToList();
                product.Categorys = cats;
            }

            return new BaseSearchModel<List<Product>>
            {
                Data = products,
                Limit = limit,
                Page = page,
                TotalRows = totalRows
            };
        }
    }

}
