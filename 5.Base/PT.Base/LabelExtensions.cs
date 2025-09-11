using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Html;
namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static class LabelExtensions
    {
        public static IHtmlContent StatusDisplay(this IHtmlHelper helper, bool status)
        {
            if (status)
            {
                return new HtmlString(string.Format("{0}", "<span class='label label-success'>Sử dụng</span>"));
            }
            else
            {
                return new HtmlString(string.Format("{0}", "<span class='label label-default'>Không sử dụng</span>"));
            }
        }
        public static IHtmlContent EnumGetDisplayName(this IHtmlHelper helper, Enum value)
        {
            Type enumType = value.GetType();
            var enumValue = Enum.GetName(enumType, value);
            MemberInfo member = enumType.GetMember(enumValue)[0];
            var attrs = member.GetCustomAttributes(typeof(DisplayAttribute), false);
            var outString = ((DisplayAttribute)attrs[0]).Name;

            if (((DisplayAttribute)attrs[0]).ResourceType != null)
            {
                outString = ((DisplayAttribute)attrs[0]).GetName();
            }
            return new HtmlString(string.Format("{0}", outString));
        }
        public static IHtmlContent DateToString(this IHtmlHelper helper, DateTime? value, string type = "")
        {
            if (value == null) return new HtmlString("");
            else
            {
                return new HtmlString(value?.ToString("dd/MM/yyyy"));
            }
        }
        public static IHtmlContent GetOrder(this IHtmlHelper helper, int i, int page)
        {
            return new HtmlString(((page - 1) + i).ToString());
        }
        public static IHtmlContent DisplayStatus(this IHtmlHelper helper, int i, int page)
        {
            return new HtmlString(((page - 1) + i).ToString());
        }
    }
}
