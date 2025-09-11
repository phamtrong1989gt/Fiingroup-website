using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PT.Domain.Model;

namespace PT.Infrastructure.Interfaces
{
    public interface IUserRepository : IGenericRepository<ApplicationUser>
    {
        //List<int> SchoolRoles(int userId);
        Task<List<DataRoleActionModel>> RoleActionsByUserAsync(int userId);
        Task UpdateRolesAsync(int userId, List<int> roles);
        Task<List<int>> GetRolesAsync(int userId);
        Task<int> StatusUserAsync(int userId);
        void DeleteRoles(int userId);
        Task<ApplicationRole> GetRoleByUser(int userId);
        Task<BaseSearchModel<List<ApplicationUser>>> SearchPagedList(int page, int limit, int? roleId, Expression<Func<ApplicationUser, bool>> predicate = null, Func<IQueryable<ApplicationUser>, IOrderedQueryable<ApplicationUser>> orderBy = null, Expression<Func<ApplicationUser, ApplicationUser>> select = null, params Expression<Func<ApplicationUser, object>>[] includeProperties);
        Task<CapchaResponse> VeryfyCapcha(string url, string secret, string response);
        Task<bool> CheckLastExpirationResetPassword(int id, int maxMinute);
        Task UpdateExpirationResetPassword(int id, int maxMinute);
    }
}
