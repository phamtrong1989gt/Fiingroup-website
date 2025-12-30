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
    public interface IParameterRepository : IGenericRepository<Parameter>
    {
    }

    public class ParameterRepository : BaseRepository<Parameter>, IParameterRepository  
    {
        private readonly ApplicationContext _context;
        public ParameterRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }
    }

}
