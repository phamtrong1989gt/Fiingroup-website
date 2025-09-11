using Microsoft.EntityFrameworkCore;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PT.Infrastructure.Repositories
{
    public class TourCategoryRepository : BaseRepository<TourCategory>, ITourCategoryRepository
    {
        private readonly ApplicationContext _context;
        public TourCategoryRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }
    }
}

