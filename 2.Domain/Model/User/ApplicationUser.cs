using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;

namespace PT.Domain.Model
{

    public class ApplicationUser : IdentityUser<int>, IAggregateRoot
    {
       
        public string Avatar { get; set; }
        [MaxLength(100)]
        public string DisplayName { get; set; }
        public bool IsLock { get; set; }
        [MaxLength(1000)]
        public string NoteLock { get; set; }
        public bool IsReLogin { get; set; }
        public bool IsSuperAdmin { get; set; } = false;
        public string Note { get; set; }
        public int? CreatedUserId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UpdatedUserId { get; set; }
        public DateTime? UpdatedDate { get; set; }
    
        public int? NumberWrongPasswords { get; set; }
        public DateTime? ExpirationWrongPassword { get; set; }
        public DateTime? ExpirationResetPassword { get; set; }

        [NotMapped]
        public virtual List<ApplicationRole> Roles { get; set; }
    }

    public class UserEditManagerModel
    {
        public int Id { get; set; }
        [Display(Name = "Ảnh đại diện")]
        public string Avatar { get; set; }

        [Display(Name = "Tên tài khoản")]
        public string Username { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(100, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 6)]
        [EmailAddress(ErrorMessage ="Không đúng định dạng email.")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [StringLength(100, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 0)]
        public string PhoneNumber { get; set; }
        [Display(Name = "Tên hiển thị")]
        [StringLength(100, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 0)]
        public string DisplayName { get; set; }
        [Display(Name = "Mô tả lý do khóa")]
        [StringLength(1000, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 0)]
        public string NoteLock { get; set; }
        [Display(Name = "Tích chọn để khóa tài khoản")]
        public bool IsLock { get; set; }
        [Display(Name = "Mô tả")]
        public string Note { get; set; }
        [Display(Name = "Chọn quyền tài khoản")]
        //public List<int> Roles { get; set; }
        public int? RoleId { get; set; }
        //public IEnumerable<SelectListItem> RoleSelectListItem { get; set; }
        public SelectList RoleSelectListItem { get; set; }


    }
    public class UserProfileModel
    {
        public int Id { get; set; }
        [Display(Name = "Ảnh đại diện")]
        public string Avatar { get; set; }
        [Display(Name = "Tên tài khoản")]
        public string UserName { get; set; }
        [Display(Name = "Email")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(100, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 6)]
        [EmailAddress(ErrorMessage = "Không đúng định dạng email.")]
        public string Email { get; set; }
        [Display(Name = "Số điện thoại")]
        [StringLength(100, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 0)]
        public string PhoneNumber { get; set; }
        [Display(Name = "Tên hiển thị")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(100, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 6)]
        public string DisplayName { get; set; }
        [Display(Name = "Mô tả")]
        public string Note { get; set; }
    }
    public class ChangePasswordAdminModel
    {
        public int Id { get; set; }
        public int NewPassword { get; set; }
    }
}
