using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static PT.Domain.Model.ServicePrice;

namespace PT.Domain.Model
{
    public class ServicePrice : IAggregateRoot
    {
        public enum ServicePriceUnitType
        {
            [Display(Name = "U&L")]
            UAndL,
            [Display(Name = "Lượt")]
            Visit,
            [Display(Name = "Số lượng")]
            Quarter
        }
        [Key]
        public int Id { get; set; }
        public int ParentId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public ServicePriceUnitType Unit { get; set; } = ServicePriceUnitType.Quarter;
        public string Type { get; set; }
        public long FromPrice { get; set; }
        public long? ToPrice { get; set; }
        public int Visits { get; set; }
        public int Order { get; set; }
        public bool Status { get; set; }
        public bool Delete { get; set; }
        public bool? SubParent { get; set; }
        public bool? ContactPrice { get; set; }
        public string Language { get; set; }
        [NotMapped]
        public Category Category { get; set; }
    }
    public class ServicePriceModel
    {
        public int Id { get; set; }
        [Display(Name = "Danh mục dịch vụ")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public int CategoryId { get; set; }

        [Display(Name = "Tên dịch vụ")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string Name { get; set; }

        [Display(Name = "Loại đơn vị")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string Type { get; set; }

        [Display(Name = "Giá từ")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [Range(0,int.MaxValue, ErrorMessage = "{0} không được để trống")]
        public long FromPrice { get; set; }
        [Display(Name = "Giá đến")]
        public long? ToPrice { get; set; }
       
        [Display(Name = "Thứ tự hiển thị")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [Range(0, int.MaxValue, ErrorMessage = "{0} không được để trống")]
        public int Order { get; set; }

        [Display(Name = "Sử dụng dữ liệu này")]
        public bool Status { get; set; }

        [Display(Name = "Đặt làm nhãn mô tả")]
        public bool SubParent { get; set; }

        [Display(Name = "Giá liên hệ")]
        public bool ContactPrice { get; set; }
        public bool Delete { get; set; }
        public string Language { get; set; }
        public int ParentId { get; set; }
        public List<SelectListItem> CategorySelectList { get; set; }
    }
    public class CategoryServicePriceModel
    {
        public int Id { get; set; }
        [Display(Name = "Giá dịch vụ")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string Name { get; set; }

        [Display(Name = "Thứ tự hiển thị")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [Range(0, int.MaxValue, ErrorMessage = "{0} không được để trống")]
        public int Order { get; set; }

        [Display(Name = "Sử dụng dữ liệu này")]
        public bool Status { get; set; }
        public string Language { get; set; }
    }
}
