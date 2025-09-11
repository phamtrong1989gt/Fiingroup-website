using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using PT.Domain.Seedwork;

namespace PT.Domain.Model
{
    public class RoleGroup : IAggregateRoot
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(500)]
        public string Name { get; set; }
        public int Order { get; set; }
        public bool Status { get; set; }
    }
    public class RoleGroupModel
    {
        public int Id { get; set; }
        [Display(Name = "Tên")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(500, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 1)]
        public string Name { get; set; }
        [Display(Name = "Thứ tự")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public int Order { get; set; }
        [Display(Name = "Trạng thái sử dụng")]
        public bool Status { get; set; }
    }
}
