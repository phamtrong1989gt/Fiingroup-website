using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PT.Domain.Model
{
    public class ResetPasswordViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage ="{0} không được để trống.")]
        [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$", ErrorMessage = "Mật khẩu phải từ 8 đến 15 ký tự, chữ cái đầu là viết hoa, và phải có ký tự đặc biệt và số.")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }
        public string Code { get; set; }
    }
}
