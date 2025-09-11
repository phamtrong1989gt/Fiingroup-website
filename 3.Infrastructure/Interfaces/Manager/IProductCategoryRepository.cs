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
    public interface IProductCategoryRepository : IGenericRepository<ProductCategory>
    {
    }

    public class ProductCategoryRepository : BaseRepository<ProductCategory>, IProductCategoryRepository
    {
        private readonly ApplicationContext _context;
        public ProductCategoryRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }
    }

}
