using System.Collections.Generic;
using System.Threading.Tasks;
using PT.Infrastructure.Interfaces;
using PT.Domain.Model;

namespace PT.Infrastructure.Interfaces
{
    public interface IRoleControllerRepository : IGenericRepository<RoleController>
    {
        Task<List<TreeRoleModel>> GetTreeRoleAsync(int roleId);
        Task UpdateRoleAsync(int roleId, List<int> ids);
        Task ReLoginUsersAsync(int roleId);
    }
}
