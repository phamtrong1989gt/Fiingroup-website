using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PT.Domain.Model
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage ="{0} không được để trống!")]
        [Display(Name = "Mật khẩu hiện tại")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(100, ErrorMessage = "{0} có độ dài từ {2} đến {1} ký tự!", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$", ErrorMessage = "Mật khẩu phải từ 8 đến 15 ký tự, chữ cái đàu là viết hoa, và phải có ký tự đặc biệt và số.")]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [Display(Name = "Nhập lại mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và mật khẩu xác nhận không khớp!")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public string ConfirmPassword { get; set; }

        public string StatusMessage { get; set; }
    }
}
