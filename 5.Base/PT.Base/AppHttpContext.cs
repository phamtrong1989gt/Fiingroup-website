using Microsoft.AspNetCore.Http;
using System;
namespace PT.Base
{
    public class CookieExtensions
    {
        public static void Set(string key, string value)
        {
            CookieOptions option = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(2),
                HttpOnly = true
            };
            AppHttpContext.Current.Response.Cookies.Append(key, value, option);
        }
        public static string Get(string key)
        {
            if (AppHttpContext.Current.Request.Cookies[key] == null)
            {
                return "";
            }
            return AppHttpContext.Current.Request.Cookies[key];
        }
    }

    public static class AppHttpContext
    {
        static IServiceProvider _services = null;
        public static IServiceProvider Services
        {
            get => _services;
            set
            {
                if (_services != null)
                    throw new Exception("Can't set once a value has already been set.");
                _services = value;
            }
        }
        public static HttpContext Current
        {
            get
            {
                IHttpContextAccessor httpContextAccessor =
                    _services.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
                return httpContextAccessor?.HttpContext;
            }
        }
    }
}
