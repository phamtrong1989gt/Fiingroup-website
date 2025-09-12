using PT.Domain.Model;
using PT.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;

namespace PT.Infrastructure.Interfaces
{
    public interface IPortalRepository : IGenericRepository<Portal>
    {
    }
    public class PortalRepository : BaseRepository<Portal>, IPortalRepository
    {
        private readonly ApplicationContext _context;
        public PortalRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }
    }
}
