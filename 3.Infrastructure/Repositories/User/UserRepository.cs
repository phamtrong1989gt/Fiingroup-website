using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using System.Net.Http;
using Newtonsoft.Json;

namespace PT.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<ApplicationUser>, IUserRepository
    {
        private readonly ApplicationContext _db;
        public UserRepository(ApplicationContext context) : base(context)
        {
            _db = context;
        }
        public async Task UpdateExpirationResetPassword(int id, int maxMinute)
        {
            var dl = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (dl != null)
            {
                dl.ExpirationResetPassword = DateTime.Now.AddMinutes(maxMinute);
                _db.Users.Update(dl);
                await _db.SaveChangesAsync();
            }
        }
        public async Task<bool> CheckLastExpirationResetPassword(int id, int maxMinute)
        {
            return await _db.Users.AnyAsync(x => x.Id == id && (x.ExpirationResetPassword <= DateTime.Now || x.ExpirationResetPassword == null));
        }
        public async Task<CapchaResponse> VeryfyCapcha(string url, string secret, string response)
        {
            using (var httpClient = new HttpClient())
            {
                url = $"{url}?secret={secret}&response={response}";
                var stringContent = new StringContent("", UnicodeEncoding.UTF8, "application/json");
                var t = await httpClient.PostAsync(url, stringContent);
                if (t.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<CapchaResponse>(await t.Content.ReadAsStringAsync());
                }
            }
            return null;
        }
        public async Task<List<DataRoleActionModel>> RoleActionsByUserAsync(int userId)
        {
          
              var listCa = await _db.RoleActions.Where(m =>
                 _db.UserRoles.Any(x => x.UserId == userId && _db.RoleDetails.Any(r => r.RoleId == x.RoleId && r.ActionId == m.Id))
                ).GroupBy(m=>m.Id).Select(m=>m.FirstOrDefault()).Select(x => new DataRoleActionModel
                {
                    ControllerName = x.ControllerId,
                    AreaName = x.RoleController.AreaId,
                    ActionName = x.Name,
                    Id = x.Id
                }).ToListAsync();

            return listCa;
        }
        public async Task<ApplicationRole> GetRoleByUser(int userId)
        {
            return await _db.Roles.FirstOrDefaultAsync(x => _db.UserRoles.Any(m => m.UserId == userId && m.RoleId == x.Id));
        }
        public async Task<int> StatusUserAsync(int userId)
        {
            var dl= await _db.Users
                .Where(m=>m.Id==userId)
                .Select(
                    x=>new ApplicationUser {
                        Id = x.Id,
                        IsLock =x.IsLock,
                        IsReLogin =x.IsReLogin
                    })
                .FirstOrDefaultAsync();
            if (dl == null) return 0;
            else if (dl.IsReLogin) return 3;
            else if (dl.IsLock) return 2;
            return 1;
        }
        public async Task UpdateRolesAsync(int userId,List<int> roles)
        {
            var listFirst =await _db.UserRoles.Where(m => m.UserId == userId).ToListAsync();
            _db.UserRoles.RemoveRange(listFirst);
            await _db.SaveChangesAsync();
            await _db.UserRoles.AddRangeAsync(roles.Select(m => new IdentityUserRole<int> { RoleId = m, UserId = userId }));
            await _db.SaveChangesAsync();
        }
        public async Task<List<int>> GetRolesAsync(int userId)
        {
            return await _db.UserRoles.Where(m => m.UserId == userId).Select(x => x.RoleId).ToListAsync();
        }

        public  async Task<BaseSearchModel<List<ApplicationUser>>> SearchPagedList(int page, int limit, int? roleId, Expression<Func<ApplicationUser, bool>> predicate = null, Func<IQueryable<ApplicationUser>, IOrderedQueryable<ApplicationUser>> orderBy = null, Expression<Func<ApplicationUser, ApplicationUser>> select = null, params Expression<Func<ApplicationUser, object>>[] includeProperties)
        {
            IQueryable<ApplicationUser> query = _db.Set<ApplicationUser>();
            if (predicate != null)
            {
                query = _db.Users.Where(predicate).AsQueryable();
            }
            if(roleId != null)
            {
                query= query.Where(m => _db.UserRoles.Any(x => x.UserId == m.Id && x.RoleId == roleId)).AsQueryable();
            }
            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
            
            var currentData = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();
            var userIds = currentData.Select(x => x.Id);
            var roles = await _db.Roles.ToListAsync();
            var roleUser = await _db.UserRoles.Where(x => userIds.Contains(x.UserId)).ToListAsync();
           

            foreach (var item in currentData)
            {
                item.Roles = roles.Where(x => roleUser.Any(m => m.UserId == item.Id && x.Id == m.RoleId)).ToList();
            }
            return new BaseSearchModel<List<ApplicationUser>>
            {
                Data = currentData,
                Limit = limit,
                Page = page,
                ReturnUrl = "",
                TotalRows = await query.Select(m => 1).CountAsync()
            };
        }
        public  void DeleteRoles(int userId)
        {
              _db.UserRoles.RemoveRange(_db.UserRoles.Where(m => m.UserId == userId));
        }
    }
}
