using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PT.Domain.Model
{
    public enum RoleManagerType
    {
        [Display(Name = "Mặc định")]
        Default = 0
    }
    public class RoleManagerModel
    {
        public int Id { get; set; } = 0;
        [Display(Name = "Tên nhóm quyền")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(100, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 5)]
        public string Name { get; set; }
        [Display(Name = "Mô tả")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Description { get; set; }
        public string TreeData { get; set; }
        [Display(Name = "Loại")]
        public RoleManagerType Type { get; set; }
        public SelectList TypeSelectList { get; set; }
    }
    public class RoleTypeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public RoleManagerType Type { get; set; }
        [Display(Name = "Chọn quyền với đơn vị")]
        public List<int> TransportCompanyIds { get; set; }
        public MultiSelectList TransportCompanySelectList { get; set; }
        [Display(Name = "Chọn quyền với trường")]
        public List<int> SchoolIds { get; set; }
        public MultiSelectList SchoolSelectList { get; set; }
        [Display(Name = "Mã học sinh")]
        public string StudentCode { get; set; } 
    }
}
