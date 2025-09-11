using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
public enum AlertType
{
    danger,
    success,
    info,
    warning
}
public static class StaticExtensions
{
    public static string GenAlert(AlertType type, string text)
    {
        return $"<div class=\"alert alert-{type.ToString()}\"><a style='font-size: 20px; top:-5px;position: relative;right:-2px;' href=\"#\" class=\"close\" data-dismiss=\"alert\" aria-label=\"close\" title=\"close\">×</a>{text}</div>";
    }

    private static readonly string[] VietNamChar = new string[]
    {
            "aAeEoOuUiIdDyY",
            "áàạảãâấầậẩẫăắằặẳẵ",
            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
            "éèẹẻẽêếềệểễ",
            "ÉÈẸẺẼÊẾỀỆỂỄ",
            "óòọỏõôốồộổỗơớờợởỡ",
            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
            "úùụủũưứừựửữ",
            "ÚÙỤỦŨƯỨỪỰỬỮ",
            "íìịỉĩ",
            "ÍÌỊỈĨ",
            "đ",
            "Đ",
            "ýỳỵỷỹ",
            "ÝỲỴỶỸ"
    };
    public static string ToSlug(this string str)
    {
        string newChar = "";
        string[] mang = str.Split(' ', '/', ',', '*', '-');
        for (int i = 0; i < mang.Count(); i++)
        {
            if (i == mang.Count() - 1)
            {
                newChar += mang[i];
            }
            else
            {
                newChar += mang[i] + "-";
            }

        }
        for (int i = 1; i < VietNamChar.Length; i++)
        {
            for (int j = 0; j < VietNamChar[i].Length; j++)
                newChar = newChar.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]);
        }
        return newChar.ToLower();
    }
    public static string ToStringDateTime(this DateTime dateTime, string languege = "vi")
    {
        return dateTime.ToString("dd/MM/yyyy HH:mm:ss");
    }
    
    public static DateTime GetLastDayOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
    }
    public static DateTime GetFirstDayOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }
    public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
    {
        foreach (var i in ie)
        {
            action(i);
        }
    }
    public static string GetDisplayName(this Enum value)
    {
        try
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
            return outString;
        }
        catch
        {
            return value.ToString();
        }
    }
    public static string DateToString(this DateTime? value, string type = "")
    {
        if (value == null) return "";
        else
        {
            return value?.ToString("dd/MM/yyyy");
        }
    }
    public static string DateToString(this DateTime value, string type = "")
    {
        return value.ToString("dd/MM/yyyy");

    }
    public static int Value(this Enum value)
    {
        return Convert.ToInt32(value);
    }

    public static double ConvertToDouble(this string input, string type = "en-US")
    {
        try
        {
            return Convert.ToDouble(input, CultureInfo.GetCultureInfo(type).NumberFormat);
        }
        catch
        {
            return 0;
        }
    }
    public static string ConvertToString(this double input, string type = "en-US")
    {
        try
        {
            return input.ToString(CultureInfo.CreateSpecificCulture(type));
        }
        catch
        {
            return "0";
        }
    }
}
