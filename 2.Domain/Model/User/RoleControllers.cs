using PT.Domain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using PT.Domain.Seedwork;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PT.Domain.Model
{
    public class RoleController : IAggregateRoot
    {
        [Key]
        [MaxLength(100)]
        public string Id { get; set; }
        [ForeignKey("RoleArea")]
        public string AreaId { get; set; }
        [ForeignKey("RoleGroups")]
        public int GroupId { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        public int Order { get; set; }
        public bool Status { get; set; }
        public virtual RoleGroup RoleGroups { get; set; }
        public virtual RoleArea RoleArea { get; set; }

    }
    public class RoleControllerModel
    {
        [Display(Name = "Mã")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(100, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 1)]
        public string Id { get; set; }
        [Display(Name = "Quyền area")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public string AreaId { get; set; }
        [Display(Name = "Nhóm quyền")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public int GroupId { get; set; }

        [Display(Name = "Tên")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(500, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 1)]
        public string Name { get; set; }
        [Display(Name = "Thứ tự")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public int Order { get; set; }
        [Display(Name = "Trạng thái sử dụng")]
        public bool Status { get; set; }
        public virtual RoleGroup RoleGroups { get; set; }
        public virtual RoleArea RoleArea { get; set; }
        public SelectList RoleGroupSelectList { get; set; }
        public SelectList RoleAreaSelectList { get; set; }
    }
}
