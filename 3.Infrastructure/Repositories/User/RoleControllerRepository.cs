using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;

namespace PT.Infrastructure.Repositories
{
   
    public class RoleControllerRepository : BaseRepository<RoleController>, IRoleControllerRepository
    {
        private readonly ApplicationContext _context;
        public RoleControllerRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public virtual async Task UpdateRoleAsync(int roleId,List<int> ids)
        {
            var listRole =await _context.RoleDetails.Where(m => m.RoleId == roleId).ToListAsync();
            var listRoleRomove = listRole.Where(m => !ids.Any(x => x == m.ActionId)).ToList();
            _context.RoleDetails.RemoveRange(listRoleRomove);
            await _context.RoleDetails.AddRangeAsync(ids.Where(m => !listRole.Any(x => x.ActionId == m)).Select(m => new RoleDetail { ActionId = m, RoleId = roleId }));
            await _context.Users.Where(m => _context.UserRoles.Any(x => x.RoleId == roleId && x.UserId == m.Id && m.IsReLogin == false)).ForEachAsync(m => m.IsReLogin = true);
            await _context.SaveChangesAsync();
     
        }
        public virtual async Task<List<TreeRoleModel>> GetTreeRoleAsync(int roleId)
        {
            var listGroup = await _context.RoleGroups.Where(m => m.Status == true).ToListAsync();
            var listController = await _context.RoleControllers.Where(m=>m.Status==true).ToListAsync();
            var listAction = await _context.RoleActions.Where(m => m.Status == true).ToListAsync();
            var listRoleInGroup = await _context.RoleDetails.Where(m=>m.RoleId==roleId).ToListAsync();
            var newList = new List<TreeRoleModel>();
            foreach(var itemG in listGroup.OrderBy(m => m.Order))
            {
                newList.Add(new TreeRoleModel { Id=$"G{itemG.Id}",Text=itemG.Name,Icon= "/Content/Admin/plugins/jstree/themes/default/treegroup.png", Parent="#",State=new TreeRoleStateModel {Opened=true } });
                foreach(var itemC in listController.Where(m=>m.GroupId==itemG.Id).OrderBy(m => m.Order))
                {
                    newList.Add(new TreeRoleModel { Id = $"C{itemC.Id}", Text = itemC.Name, Icon = "/Content/Admin/plugins/jstree/themes/default/treecontroller.png", Parent = $"G{itemG.Id}", State = new TreeRoleStateModel { Opened = true } });
                    foreach (var itemA in listAction.Where(m => m.ControllerId == itemC.Id).OrderBy(m => m.Order))
                    {
                        newList.Add(new TreeRoleModel { Id = $"A{itemA.Id}", Text = itemA.ActionName, Icon = "/Content/Admin/plugins/jstree/themes/default/treeaction.png", Parent = $"C{itemC.Id}", State = new TreeRoleStateModel { Opened = true, Selected = listRoleInGroup.Any(x=>x.ActionId==itemA.Id) } });
                    }
                }
            }
            return newList;
        }
        public virtual async Task ReLoginUsersAsync(int roleId)
        {
            var listUser = new List<ApplicationUser>();
            listUser = await _context.Users.Where(m => _context.UserRoles.Any(x => x.RoleId == roleId && x.UserId == m.Id)).ToListAsync();
            foreach(var item in listUser)
            {
                if(item.IsReLogin==false)
                {
                    item.IsReLogin = true;
                }
            }
        }
       
    }
}