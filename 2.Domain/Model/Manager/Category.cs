using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Domain.Model
{
    public class Category : IAggregateRoot
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public CategoryType Type { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string Banner { get; set; }
        public string Banner2 { get; set; }
        public string LinkData { get; set; }
        public int Order { get; set; }
        [MaxLength(10)]
        public string Language { get; set; }
        public bool Status { get; set; }
        [NotMapped]
        public virtual Link Link { get; set; }
        [NotMapped]
        public  Category Parent { get; set; }
        [NotMapped]
        public BaseSearchModel<List<ContentPage>> PageBlog { get; set; }
        [NotMapped]
        public BaseSearchModel<List<Tour>> Tours { get; set; }
        [NotMapped]
        public List<Category> ChildrentCategorys { get; set; }
        [NotMapped]
        public List<ContentPage> ContentPageCategory { get; set; } = new List<ContentPage>();
        public bool IsHome { get; set; }
        [NotMapped]
        public List<LinkReference> LinkReferences { get; set; }

        [NotMapped]
        public BaseSearchModel<List<Product>> Products { get; set; }
        public int PortalId { get; set; }

        [NotMapped]
        public string FullPath { get; set; }
    }
    public class CategoryModel:SeoModel
    {
        public int Id { get; set; }
        [Display(Name = "Tên danh mục")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 1)]
        public string Name { get; set; }
        [Display(Name = "Tóm tắt")]
        public string Summary { get; set; }
        [Display(Name = "Nội dung")]
        public string Content { get; set; }
        [Display(Name = "Ảnh đại diện")]
        public string Banner { get; set; }
        public int Order { get; set; }
        public int ParentId { get; set; }
        public CategoryType SlugType { get; set; } = CategoryType.CategoryBlog;
        [Display(Name = "Đặt làm dịch vụ tiêu biểu")]
        public bool IsHome { get; set; }
        [Display(Name = "Ảnh đại diện dịch vụ tiêu biểu")]
        public string Banner2 { get; set; }
        public SelectList PortalSelectList { get; set; }
    }
}
