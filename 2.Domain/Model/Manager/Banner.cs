using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PT.Domain.Model
{

    public enum BannerType
    {
        Slide,
        Advertising
    }

    public class Banner : IAggregateRoot
    {
        public int Id { get; set; }
        public string Name { get; set; }
        // PortalId dùng để phân biệt dữ liệu giữa các website/portal
        public int PortalId { get; set; }
        // NotMapped nav property để hiển thị thông tin Portal trên giao diện quản trị
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public Portal Portal { get; set; }
        // Content lưu lại HTML đã được xử lý để tránh phải sinh lại mỗi lần
        public string Content { get; set; }
        public string ClassActive { get; set; }
        public string Template { get; set; }
        public bool Status { get; set; }
        public bool Delete { get; set; }
        public string Language { get; set; }
        public string Code { get; set; }

        //public int? DisplayTime { get; set; }
        //public int? TimeOut { get; set; }
        //public string BannerUrl { get; set; }
        //public DateTime? StartDate { get; set; }
        //public DateTime? EndDate { get; set; }

        public BannerType Type { get; set; } = BannerType.Slide;
    }
    public class BannerModel
    {
        public int Id { get; set; }
        [Display(Name = "Tên")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        public string Name { get; set; }
        [Display(Name = "Class Action")]
        public string ClassActive { get; set; }
        [Display(Name = "Mẫu")]
        public string Template { get; set; }
        [Display(Name = "Sử dụng dữ liệu này")]
        public bool Status { get; set; }
        public bool Delete { get; set; }
        public string Language { get; set; }

        [Display(Name = "Mã")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        public string Code { get; set; }
        [Display(Name = "Cổng (Portal)")]
        public int PortalId { get; set; }
        public SelectList PortalSelectList { get; set; }
    }
}
