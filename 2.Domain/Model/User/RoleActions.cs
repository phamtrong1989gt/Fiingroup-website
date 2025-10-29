using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;

namespace PT.Domain.Model
{
    public class RoleAction :  IAggregateRoot
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("RoleController")]
        [MaxLength(100)]
        public string ControllerId { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string ActionName { get; set; }
        public int Order { get; set; }
        public bool Status { get; set; }
        public virtual RoleController RoleController { get; set; }
    }
    public class RoleActionModel
    {
        public int Id { get; set; }
        [Display(Name = "Area")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public string AreaId { get; set; }

        [Display(Name = "Controller")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public string ControllerId { get; set; }
        [Display(Name = "Tên mô tả")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(500, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 1)]
        public string ActionName { get; set; }
        [Display(Name = "Mã")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(100, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 1)]
        public string Name { get; set; }
        [Display(Name = "Thứ tự")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public int Order { get; set; }
        [Display(Name = "Trạng thái sử dụng")]
        public bool Status { get; set; }
        public SelectList RoleAreaSelectList { get; set; }
    }
    public class DataRoleActionModel
    {
       public string ControllerName { get; set; }
        public string AreaName { get; set; }
        public string ActionName { get; set; }
        public int Id { get; set; }
    }
}
