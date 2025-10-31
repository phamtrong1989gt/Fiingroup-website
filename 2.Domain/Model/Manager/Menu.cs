using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PT.Domain.Model
{
    public class Menu : IAggregateRoot
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Template { get; set; }
        public string Template1 { get; set; }
        public string Template2 { get; set; }
        public string Template3 { get; set; }
        public string HasChildrentClass1 { get; set; }
        public string HasChildrentClass2 { get; set; }
        public string HasChildrentClass3 { get; set; }
        public string Content { get; set; }
        public bool Status { get; set; }
        public bool Delete { get; set; }
        public string Language { get; set; }
        public int PortalId { get; set; } = 0;
        [NotMapped]
        public Portal Portal { get; set; }
        [NotMapped]
        public string PortalName { get; set; }
    }
    public class MenuModel
    {
        [Display(Name = "Mã")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        public string Code { get; set; }

        public int Id { get; set; }
        [Display(Name = "Tên")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        public string Name { get; set; }

        [Display(Name="Mẫu giao diện chính")]
        public string Template { get; set; }
        [Display(Name = "Mẫu giao diện con 1")]
        public string Template1 { get; set; }
        [Display(Name = "Mẫu giao diện con 2")]
        public string Template2 { get; set; }
        [Display(Name = "Mẫu giao diện con 3")]
        public string Template3 { get; set; }
        [Display(Name= "Tên lớp nếu level 1 chứa con")]
        public string HasChildrentClass1 { get; set; }
        [Display(Name = "Tên lớp nếu level 2 chứa con")]
        public string HasChildrentClass2 { get; set; }
        [Display(Name = "Tên lớp nếu level 3 chứa con")]
        public string HasChildrentClass3 { get; set; }
        [Display(Name = "Sử dụng dữ liệu này")]
        public bool Status { get; set; }
        public bool Delete { get; set; }
        public string Language { get; set; }
        [Display(Name = "Sử dụng dữ liệu này")]
        [Required(ErrorMessage = "Cổng")]
        public int PortalId { get; set; }

        public SelectList PortalSelectList { get; set; }
        public string PortalName { get; set; }
    }
}
