using PT.Domain.Model;
using PT.Infrastructure.Interfaces;

namespace PT.Infrastructure.Repositories
{
    public class CountryRepository : BaseRepository<Country>, ICountryRepository
    {
        public CountryRepository(ApplicationContext context) : base(context)
        {
        }
    }
}

