using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PT.Domain.Model
{
    public class LoginViewModel
    {
        [Display(Name ="Tài khoản")]
        [Required(ErrorMessage ="{0} không được để trống!")]
        public string Username { get; set; }
        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập?")]
        public bool RememberMe { get; set; }
        public string Capcha { get; set; }
    }
}
