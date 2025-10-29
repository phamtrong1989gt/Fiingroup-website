using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using PT.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace PT.Base
{
    public class CultureHelper
    {
        //public static LanguageModel GetImplementedCulture(string id)
        //{
        //    var cul = ListData.ListLanguage.FirstOrDefault(m => m.Id == id || m.Id2 == id);
        //    if (id == null || cul == null)
        //    {
        //        return GetDefaultCulture;
        //    }
        //    return cul;
        //}
        //public static LanguageModel GetDefaultCulture
        //{
        //    get
        //    {
        //        return ListData.ListLanguage.FirstOrDefault(m => m.Id == "en"); // return Default culture
        //    }
        //}
        public static LanguageModel GetCurrentCulture
        {
            get
            {
                string a = Thread.CurrentThread.CurrentCulture.Name;
                return ListData.ListLanguage.FirstOrDefault(m => m.Id == a);
            }
        }
        public static void AppendLanguage(string language, bool isRedirect = true)
        {
            //try
            //{
            //    language = language ?? "vi";
            //    if (GetCurrentLanguage != language)
            //    {

            //         AppHttpContext.Current.Response.Cookies.Append(
            //         CookieRequestCultureProvider.DefaultCookieName,
            //         CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(language)),
            //         new CookieOptions
            //         {
            //             Expires = DateTimeOffset.UtcNow.AddDays(1),
            //             IsEssential = true,
            //             Path = "/",
            //             HttpOnly = true,
            //         }
            //        );
            //        if (isRedirect)
            //        {
            //            AppHttpContext.Current.Response.Redirect(AppHttpContext.Current.Request.Path);
            //        }
            //    }
            //}
            //catch 
            //{
            //}
        }
        public static string GetCurrentLanguage
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture.Name;
            }
        }
    }
}
