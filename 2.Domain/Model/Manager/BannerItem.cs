using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PT.Domain.Model
{
    public class BannerItem : IAggregateRoot
    {
        public int Id { get; set; }
        public int BannerId { get; set; }
        public string Name { get; set; }
        public string Href { get; set; }
        public string Target { get; set; }
        public string Content { get; set; }
        public string Template { get; set; }
        public string Banner { get; set; }
        public int Order { get; set; }
        public bool Status { get; set; }
    }
    public class BannerItemModel
    {
        public int Id { get; set; }
        public int BannerId { get; set; }
        [Display(Name = "Tên")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        public string Name { get; set; }
        [Display(Name = "Đường dẫn đến")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        public string Href { get; set; }
        [Display(Name = "Nội dung")]
        public string Content { get; set; }
        [Display(Name = "Cách mở trang")]
        public string Target { get; set; }
        [Display(Name = "Mẫu")]
        public string Template { get; set; }
        [Display(Name = "Chọn hình ảnh")]
        public string Banner { get; set; }
        public int Order { get; set; }
        [Display(Name = "Sử dụng dữ liệu này")]
        public bool Status { get; set; }
    }
}
