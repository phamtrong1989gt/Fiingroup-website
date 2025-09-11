using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;
using static PT.Domain.Model.Employee;

namespace PT.Domain.Model
{
    public class Employee : IAggregateRoot
    {
        public enum EmployeeJob
        {
            [Display(Name ="Bác sỹ nha khoa")]
            Job1,
            [Display(Name = "Bác sỹ nha khoa tập sự")]
            Job2
        }
        public enum EmployeeGender
        {
            [Display(Name = "Không xác định")]
            Unknown,
            [Display(Name = "Nam")]
            Male,
            [Display(Name = "Nữ")]
            Female
        }
        public int Id { get; set; }

        public string FullName { get; set; }
        public string Summary { get; set; }
        public EmployeeGender Gender { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Facebook { get; set; }
        public EmployeeJob Job { get; set; }
        public string Degrees { get; set; }
      
        public string Office { get; set; }
        public string Banner { get; set; }
        public string Content { get; set; }
        public bool Status { get; set; }
        [MaxLength(10)]
        public string Language { get; set; }
        public int EmployeeMappingId { get; set; }
        public bool Delete { get; set; }
        [NotMapped]
        public virtual Link Link { get; set; }
        // Chuyên môn
        public bool Endodontics { get; set; }
        public bool GeneralDentistry { get; set; }
        public bool OralMedicine { get; set; }
        public bool OralSurgery { get; set; }
        public bool Orthodontics { get; set; }
        public bool Periodontics { get; set; }
        public bool Prosthodontics { get; set; }
        [NotMapped]
        public List<Employee> Employees { get; set; }
    }
    public class EmployeeModel
    {
        public int Id { get; set; }
        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        public string FullName { get; set; }
        [Display(Name = "Tóm lược")]
        public string Summary { get; set; }
        [Display(Name = "Giới tính")]
        public EmployeeGender Gender { get; set; }
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Facebook")]
        public string Facebook { get; set; }
        [Display(Name = "Vị trí làm việc")]
        public EmployeeJob Job { get; set; }
        [Display(Name = "Bằng cấp")]
        public string Degrees { get; set; }

        [Display(Name = "Kinh nghiệm làm việc")]
        public string Office { get; set; }
        public string Banner { get; set; }
        public string Content { get; set; }
        public CategoryType SlugType { get; set; } = CategoryType.Employee;
        [Display(Name = "Ánh xạ với nhân viên")]
        public int EmployeeMappingId { get; set; }
        public SelectList EmployeeMappingSelectList { get; set; }

        [Display(Name = "Nội nha")]
        public bool Endodontics { get; set; }
        [Display(Name = "Nha khoa tổng quát")]
        public bool GeneralDentistry { get; set; }
        [Display(Name = "Thuốc uống")]
        public bool OralMedicine { get; set; }
        [Display(Name = "Phẫu thuật miệng")]
        public bool OralSurgery { get; set; }
        [Display(Name = "Chỉnh nha")]
        public bool Orthodontics { get; set; }
        [Display(Name = "Nha chu")]
        public bool Periodontics { get; set; }
        [Display(Name = "Phục hình")]
        public bool Prosthodontics { get; set; }
   
        [Display(Name = "Sử dụng dữ liệu này")]
        public bool Status { get; set; } = true;
        public bool Delete { get; set; } = false;
        public string Language { get; set; }
    }
}
