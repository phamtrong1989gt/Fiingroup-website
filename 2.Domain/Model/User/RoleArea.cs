using System.ComponentModel.DataAnnotations;
using PT.Domain.Seedwork;

namespace PT.Domain.Model
{
    public class RoleArea : IAggregateRoot
    {
        [Key]
        [MaxLength(100)]
        public string Id { get; set; }
        [MaxLength(500)]
        public string Name { get; set; }
        public int Order { get; set; }
        public bool Status { get; set; }
    }
    public class RoleAreaModel
    {
        [Display(Name = "Mã")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(100, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 1)]
        public string Id { get; set; }
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
