using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;
using static PT.Domain.Model.Customer;
using static PT.Domain.Model.Employee;

namespace PT.Domain.Model
{
    public class Customer : IAggregateRoot
    {
        public enum CustomerGender
        {
            [Display(Name = "Không xác định")]
            Unknown,
            [Display(Name = "Nam")]
            Male,
            [Display(Name = "Nữ")]
            Female
        }
        public int Id { get; set; }
        public CustomerGender Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string Banner { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Content { get; set; }
        public string Country { get; set; }
        public bool Status { get; set; }
        public bool Delete { get; set; }
        [NotMapped]
        public int ContactCount { get; set; }
        [NotMapped]
        public Link Link { get; set; }
    }
    public class CustomerModel
    {
        public int Id { get; set; }
        [Display(Name = "Giới tính")]
        public CustomerGender Gender { get; set; }
        [Display(Name = "Ngày sinh")]
        public DateTime? Birthday { get; set; }
        public string Banner { get; set; }
        [Display(Name = "Tên khách hàng")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string FullName { get; set; }
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }
        [Display(Name = "Email")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [EmailAddress(ErrorMessage ="Không đúng định dạng email")]
        public string Email { get; set; }
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }
        [Display(Name = "Nội dung")]
        public string Content { get; set; }
        [Display(Name = "Quốc tịch")]
        public string Country { get; set; }
        [Display(Name = "Sử dụng dữ liệu này")]
        public bool Status { get; set; }
        public bool Delete { get; set; }
        [NotMapped]
        public int ContactCount { get; set; }
    }
}
