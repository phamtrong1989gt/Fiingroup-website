using System;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using PT.Domain.Model;
using Microsoft.AspNetCore.Http;
using static PT.Base.AppHttpContext;
namespace PT.Base
{
    public static class DataUserInfo 
    {
        public static int UserId
        {
            get
            {
                return Int32.Parse(Current.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            }
        }
        public static bool IsLogin
        {
            get
            {
                var a = Current.User.FindFirst(ClaimTypes.NameIdentifier);
                return a != null;
            }
        }
        public static bool IsSuperAdmin
        {
            get
            {
                return Convert.ToBoolean(Current.User.Claims.FirstOrDefault(c => c.Type == "IsSuperAdmin").Value);
            }
        }
        public static string Email
        {
            get
            {
                return Current.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            }
        }
        public static string UserName
        {
            get
            {
                return Current.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            }
        }
        public static List<RoleActionModel> RoleActions
        {
            get
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<RoleActionModel>>(Current.User.Claims.FirstOrDefault(c => c.Type == "RoleActions")?.Value);
            }
        }
        public static string StringRoleActions
        {
            get
            {
                return Current.User.Claims.FirstOrDefault(c => c.Type == "RoleIds")?.Value;
            }
        }
        public static string PhoneNumber
        {
            get
            {
                return Current.User.Claims.FirstOrDefault(c => c.Type == "PhoneNumber")?.Value;
            }
        }
        public static string Avatar
        {
            get
            {
                return Current.User.Claims.FirstOrDefault(c => c.Type == "Avatar")?.Value;
            }
        }
        public static string DisplayName
        {
            get
            {
                return Current.User.Claims.FirstOrDefault(c => c.Type == "DisplayName")?.Value;
            }
        }
       
        public static List<int> RoleTransportCompany
        {
            get
            {
                var data = Current.User.Claims.FirstOrDefault(c => c.Type == "RoleTransportCompany")?.Value;
                if(data==null|| data=="")
                {
                    return new List<int>();
                }
                return data.Split(',').Select(x=> int.Parse(x)).ToList();
            }
        }
        public static List<int> RoleSchool
        {
            get
            {
                var data = Current.User.Claims.FirstOrDefault(c => c.Type == "RoleSchool")?.Value;
                if (data == null || data == "")
                {
                    return new List<int>();
                }
                return data.Split(',').Select(x => int.Parse(x)).ToList();
            }
        }
        public static List<string> RoleParents
        {
            get
            {
                var data = Current.User.Claims.FirstOrDefault(c => c.Type == "RoleParents")?.Value;
                if (data == null || data == "")
                {
                    return new List<string>();
                }
                return data.Split(',').Select(x => x).ToList();
            }
        }
        public static RoleManagerType GetRoleManagerType
        {
            get
            {
                var data = Current.User.Claims.FirstOrDefault(c => c.Type == "RoleManagerType")?.Value;
                if (data == null)
                {
                    return RoleManagerType.Default;
                }
                RoleManagerType m = (RoleManagerType)Enum.Parse(typeof(RoleManagerType), data, true);
                return m;
            }
        }
        #region [Extention]

        //public static bool ValidateRoleCenter()
        //{
        //    if (IsSuperAdmin)
        //    {
        //        return true;
        //    }
        //    else if (GetRoleManagerType == RoleManagerType.Center)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    };
        //}
        //public static bool ValidateTransportCompany(int transportCompany)
        //{
        //    if (IsSuperAdmin)
        //    {
        //        return true;
        //    }
        //    else if (GetRoleManagerType == RoleManagerType.TransportCompany)
        //    {
        //        if(RoleTransportCompany.Any(x=>x== transportCompany))
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    };
        //}
        //public static bool IsCenter
        //{
        //    get
        //    {
        //        return IsSuperAdmin ? false : (GetRoleManagerType == RoleManagerType.Center);
        //    }
        //}
        //public static bool IsTransportCompany
        //{
        //    get
        //    {
        //        return IsSuperAdmin ? false : GetRoleManagerType == (RoleManagerType.TransportCompany);
        //    }
        //}
        //public static List<TransportCompany> CheckRoleTransportCompany(List<TransportCompany> list)
        //{
        //    if(IsCenter || IsSuperAdmin)
        //    {
        //        return list;
        //    }
        //    else if(IsTransportCompany)
        //    {
        //        return list.Where(x => RoleTransportCompany.Any(e => e == x.Id)).ToList();
        //    }
        //    else
        //    {
        //        return new List<TransportCompany>();
        //    }
        //}
        //public static bool IsRoleRoleTransportCompany(int id)
        //{
        //    return (RoleTransportCompany.Contains<int>(id) || IsCenter || IsSuperAdmin);
        //}

        #endregion

    }
}