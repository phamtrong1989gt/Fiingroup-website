using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PT.Domain.Model
{
    public class MenuItem : IAggregateRoot
    {
        [Key]
        public int Id { get; set; }
        public int MenuId { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public string Href { get; set; }
        public string Icon { get; set; }
        public string Target { get; set; }
        public string Class { get; set; }
        public int Order { get; set; }
        public bool IsLinkLocal { get; set; }
        public bool Status { get; set; }
        public int LinkId { get; set; }
        public CategoryType CategoryType { get; set; } = CategoryType.Blog;
        public string Language { get; set; }
    }
    public class MenuItemModel
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public int ParentId { get; set; }
        [Display(Name = "Tên")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        public string Name { get; set; }
        [Display(Name = "Đường dẫn đến")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        public string Href { get; set; }
        [Display(Name = "Icon")]
        public string Icon { get; set; }
        [Display(Name = "Cách mở trang")]
        public string Target { get; set; }
        [Display(Name = "Lớp tùy biến")]
        public string Class { get; set; }
        public int Order { get; set; }
        [Display(Name="Sử dụng dữ liệu này")]
        public bool Status { get; set; }
        [Display(Name = "Sử dụng link nội bộ")]
        public bool IsLinkLocal { get; set; }
        [Display(Name = "Link nội bộ")]
        public int LinkId { get; set; }
        [Display(Name = "Loại link nội bộ")]
        public CategoryType? CategoryType { get; set; }
        [Display(Name = "Ngôn ngữ")]
        public string Language { get; set; }
        public SelectList LinkSelectList { get; set; }
    }
}
