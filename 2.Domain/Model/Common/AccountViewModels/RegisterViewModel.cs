using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PT.Domain.Model
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$", ErrorMessage = "Mật khẩu phải từ 8 đến 15 ký tự, chữ cái đàu là viết hoa, và phải có ký tự đặc biệt và số.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
    public class ManagerRegisterModel
    {
        public int Id { get; set; }
        [Display(Name = "Tài khoản")]
        [StringLength(30, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 6)]
        [Required(ErrorMessage = "{0} không được để trống.")]
        public string Username { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage ="Không đúng định dạng email.")]
        [StringLength(200, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 10)]
        [Required(ErrorMessage = "{0} không được để trống.")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [StringLength(100, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 0)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Tên hiển thị")]
        [StringLength(100, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 6)]
        public string DisplayName { get; set; }

        [Display(Name = "Mô tả lý do khóa")]
        [StringLength(1000, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 0)]
        public string NoteLock { get; set; }

        [Display(Name = "Tích chọn để khóa tài khoản")]
        public bool IsLock { get; set; }

        [Display(Name = "Mô tả")]
        public string Note { get; set; }

        [Display(Name = "Chọn quyền tài khoản")]
        public int? RoleId { get; set; }
        public IEnumerable<SelectListItem> RoleSelectListItem { get; set; }

        public List<int> TransportCompanyIds { get; set; }
        public List<int> SchoolIds { get; set; }
        public string StudentCode { get; set; }
        public string Avatar { get; set; }
    }
}
