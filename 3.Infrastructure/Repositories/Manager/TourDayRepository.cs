using PT.Domain.Model;
using PT.Infrastructure.Interfaces;

namespace PT.Infrastructure.Repositories
{
    public class TourDayRepository : BaseRepository<TourDay>, ITourDayRepository
    {
        private readonly ApplicationContext _context;
        public TourDayRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }
    }
}

