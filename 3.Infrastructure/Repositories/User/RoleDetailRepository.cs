using PT.Infrastructure.Interfaces;
using PT.Domain.Model;

namespace PT.Infrastructure.Repositories
{
    public class RoleDetailRepository : BaseRepository<RoleDetail>, IRoleDetailRepository
    {
        public RoleDetailRepository(ApplicationContext context) : base(context)
        {
        }

    }
}
