using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using PT.Infrastructure.Interfaces;
using PT.Domain.Model;
namespace  PT.Base
{
    public class RoleType
    {
        public string Area { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }

    }
    public class AuthorizePermissionAttribute : TypeFilterAttribute
    {
        public AuthorizePermissionAttribute(string action = null, string controller = null, string area = null)
            : base(typeof(PermissionFilter))
        {
            Arguments = new object[] { new RoleType() {Action=action,Area=area,Controller=controller } };
        }
    }
    public class IsSupperAdminAuthorizePermissionAttribute : TypeFilterAttribute
    {
        public IsSupperAdminAuthorizePermissionAttribute()
            : base(typeof(IsSupperAdminPermissionFilter))
        {
            Arguments = new object[] {  };
        }
    }
    public class PermissionFilter : Attribute, IAsyncAuthorizationFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        readonly RoleType _roleType;

        public PermissionFilter(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleType roleType)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleType = roleType;
        }
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            //if ("vi" != CultureHelper.GetCurrentCulture.Id)
            //{
            //    CultureHelper.AppendLanguage("vi",false);
            //}

            List<string> listAction = new List<string>();
            string controller, action, area = "Null", returnUrl;
            int userId;
            ApplicationUser user;
            List<DataRoleActionModel> listRoleUser;
            controller = _roleType.Controller ?? context.RouteData.Values["controller"].ToString();
            action = _roleType.Action ?? context.RouteData.Values["action"].ToString();
            if (_roleType.Area != null)
            {
                area = _roleType.Area;
            }
            else
            {
                if (context.RouteData.Values.TryGetValue("area", out object areaObj))
                {
                    area = _roleType.Area ?? areaObj.ToString();
                }
            }
            listAction = action.Split("|").ToList();
            userId = Convert.ToInt32(context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            user = await _userManager.FindByIdAsync(userId.ToString());
            // Không tồn tại
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                returnUrl = "/Login" + "?ReturnUrl=" + System.Text.Encodings.Web.UrlEncoder.Default.Encode(context.HttpContext.Request.Path + context.HttpContext.Request.QueryString);
                context.Result = new RedirectResult(returnUrl);
                return;
            }
            // Tài khoản bị khóa
            if (user.IsLock && !user.IsSuperAdmin)
            {
                await _signInManager.SignOutAsync();
                returnUrl = "/Login" + "?ReturnUrl=" + System.Text.Encodings.Web.UrlEncoder.Default.Encode(context.HttpContext.Request.Path + context.HttpContext.Request.QueryString);
                context.Result = new RedirectResult(returnUrl);
                return;
            }
            // Yêu cầu đăng nhập lại
            if (user.IsReLogin)
            {
                await _signInManager.RefreshSignInAsync(user);
                user.IsReLogin = false;
                await _userManager.UpdateAsync(user);
            }
            if (!user.IsSuperAdmin)
            {
                //Check quyền
                listRoleUser = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataRoleActionModel>>(context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "RoleActions")?.Value);
                if (listRoleUser.Any(m => listAction.Contains(m.ActionName) && m.AreaName == area && m.ControllerName == controller))
                {
                    return;
                }
                context.Result = new ForbidResult();
            }
            return;
        }
    }
    public class IsSupperAdminPermissionFilter : Attribute, IAsyncAuthorizationFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public IsSupperAdminPermissionFilter(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if ("vi" != CultureHelper.GetCurrentCulture.Id)
            {
                CultureHelper.AppendLanguage("vi", false);
            }

            string returnUrl;
            int userId;
            ApplicationUser user;
            userId = Convert.ToInt32(context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            user = await _userManager.FindByIdAsync(userId.ToString());
            // Không tồn tại
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                returnUrl = "/Login" + "?ReturnUrl=" + System.Text.Encodings.Web.UrlEncoder.Default.Encode(context.HttpContext.Request.Path + context.HttpContext.Request.QueryString);
                context.Result = new RedirectResult(returnUrl);
                return;
            }
            // Tài khoản bị khóa
            if (user.IsLock && !user.IsSuperAdmin)
            {
                await _signInManager.SignOutAsync();
                returnUrl = "/Login" + "?ReturnUrl=" + System.Text.Encodings.Web.UrlEncoder.Default.Encode(context.HttpContext.Request.Path + context.HttpContext.Request.QueryString);
                context.Result = new RedirectResult(returnUrl);
                return;
            }
            // Yêu cầu đăng nhập lại
            if (user.IsReLogin)
            {
                await _signInManager.RefreshSignInAsync(user);
                user.IsReLogin = false;
                await _userManager.UpdateAsync(user);
            }
            if (!user.IsSuperAdmin)
            {
                context.Result = new ForbidResult();
            }
            return;
        }
    }
}
