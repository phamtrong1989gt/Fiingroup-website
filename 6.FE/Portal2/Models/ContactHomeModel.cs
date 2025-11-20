using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PT.UI.Models
{
    public class ContactHomeModel
    {
        public int Id { get; set; }

        [Display(Name = "FullName")]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(50, MinimumLength =10)]
        public string FullName { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(20, MinimumLength = 10)]
        public string Phone { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(100, MinimumLength = 10)]
        public string Email { get; set; }

        [Display(Name = "Tên công ty")]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(100, MinimumLength = 1)]
        public string ConpanyName { get; set; }

        [Display(Name = "Chức danh công việc")]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(100, MinimumLength = 1)]
        public string Position { get; set; }

        [Display(Name = "Mô tả chi tiết")]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(100, MinimumLength = 10)]
        public string Content { get; set; }
        public string Language { get; set; }
        public string Capcha { get; set; }

        [Display(Name = "Nhóm ngành")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public int? ServiceId { get; set; }

        [Display(Name = "Sản phẩm & dịch vụ quan tâm")]
        public string Products { get; set; }

        [Display(Name = "Loại liên hệ")]
        public int Type { get; set; }
        public SelectList CountrySelectlist { get;  set; }
    }

    public class ContactSolotionModel
    {
        public int Id { get; set; }
        [Display(Name = "FullName")]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(50, MinimumLength = 10)]
        public string FullName { get; set; }
        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(20, MinimumLength = 10)]
        public string Phone { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(100, MinimumLength = 10)]
        public string Email { get; set; }
        public string Language { get; set; }
        public string Capcha { get; set; }
        [Display(Name = "Nhận tin tức liên quan đến lĩnh vực")]
        public string Products { get; set; }
        [Display(Name = "Loại liên hệ")]
        public int Type { get; set; }
    }

}
