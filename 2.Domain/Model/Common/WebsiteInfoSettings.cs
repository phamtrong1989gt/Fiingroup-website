using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Domain.Model
{
    public class WebsiteInfoSettings
    {
        public string Id { get; set; }
        [Display(Name = "Tên đơn vị")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public string Name { get; set; }
        [Display(Name = "Địa chỉ đơn vị")]
        public string Address { get; set; }

        [Display(Name = "Địa chỉ đơn vị 2")]
        public string Address2 { get; set; }

        [Display(Name = "Địa chỉ đơn vị 3")]
        public string Address3 { get; set; }

        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Display(Name = "Số điện thoại 2")]
        public string Phone2 { get; set; }

        [Display(Name = "Số điện thoại 3")]
        public string Phone3 { get; set; }

        [Display(Name = "Đường dây nóng")]
        public string Hotline { get; set; }

        [Display(Name = "Đường dây nóng 2")]
        public string Hotline2 { get; set; }

        [Display(Name = "Đường dây nóng 3")]
        public string Hotline3 { get; set; }

        [Display(Name = "Số fax")]
        public string Fax { get; set; }
        [Display(Name = "Tên website")]
        public string Website { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Email 2")]
        public string Email2 { get; set; }
        [Display(Name = "Email 3")]
        public string Email3 { get; set; }

        [Display(Name = "Logo")]
        public string Logo { get; set; }
        [Display(Name = "Mã nhúng video")]
        public string Video { get; set; }
        [Display(Name = "Mã nhúng map")]
        public string Map { get; set; }
        [Display(Name = "Mô tả")]
        public string Note { get; set; }
    }
}
