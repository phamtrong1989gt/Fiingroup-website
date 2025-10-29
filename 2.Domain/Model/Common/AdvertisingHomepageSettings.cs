using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Domain.Model
{
    public class AdvertisingHomepageSettings
    {
        public string Id { get; set;}
        [Display(Name ="Hiển thị trong khoảng thời gian (giây)")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        public int DisplayTime { get; set; }

        [Display(Name = "Giãn cách thời gian hiển thị lại (Giờ)")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        public int TimeOut { get; set; }

        public string Banner { get; set; }

        [Display(Name = "Hiển thị từ")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Hiển thị đến")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Mẫu giao diện")]
        public string Template { get; set; }

        [Display(Name = "Hiển thị banner này")]
        public bool Status { get; set; }

        [Display(Name = "Chiều rộng")]
        public int Width { get; set; }

        [Display(Name = "Chiều cao")]
        public int Height { get; set; }

        [Display(Name = "Quảng cáo dạng ảnh")]
        public bool IsBannerImg { get; set; }

        [Display(Name = "Đường dẫn đến")]
        public string Href { get; set; }

        [Display(Name = "Cách mở trang")]
        public string Target { get; set; }

        [Display(Name = "Quan hệ (rel)")]
        public string Rel { get; set; }

        [Display(Name = "Tiêu đề 1")]
        public string HeaderText1 { get; set; }

        [Display(Name = "Tiêu đề 2")]
        public string HeaderText2 { get; set; }

        [Display(Name = "Nội dung")]
        public string ContentText { get; set; }

        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }
        public DateTime? UpdatedDate { get; set; }

    }
}
