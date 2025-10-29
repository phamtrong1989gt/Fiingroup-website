using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PT.Domain.Model
{
    public class Image : IAggregateRoot
    {
        public int Id { get; set; }
        public int ImageGalleryId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public bool Status { get; set; }
        public string Language { get; set; }
        [NotMapped]
        public Category Category { get; set; }
    }
    public class ImageModel
    {
        public int Id { get; set; }
        [Display(Name = "Danh mục dịch vụ")]
        public int CategoryId { get; set; }
        [Display(Name = "Tên")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        public string Name { get; set; }
        [Display(Name = "Sử dụng dữ liệu này")]
        public bool Status { get; set; }
        [Display(Name = "Hình ảnh 1")]
        public string Image1 { get; set; }
        [Display(Name = "Hình ảnh 2")]
        public string Image2 { get; set; }
        public bool Delete { get; set; }
        public string Language { get; set; }
        public int ImageGalleryId { get; set; }
        public List<SelectListItem> CategorySelectList { get; set; }
    }
}
