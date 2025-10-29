using System.ComponentModel.DataAnnotations;
using PT.Domain.Seedwork;

namespace PT.Domain.Model
{
    public class StaticInformation : IAggregateRoot
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Content { get; set; }
        public bool Status { get; set; }
        [MaxLength(10)]
        public string Language { get; set; }
        public bool Delete { get; set; }
    }
    public class StaticInformationModel
    {
        public int Id { get; set; }
        [Display(Name = "Tên")]
        public string Name { get; set; }

        [Display(Name = "Mã")]
        public string Code { get; set; }
        public int ParentId { get; set; }
        [Display(Name = "Nội dung")]
        public string Content { get; set; }
        [Display(Name = "Sử dụng dữ liệu này")]
        public bool Status { get; set; }
        [MaxLength(10)]
        public string Language { get; set; }
        public bool Delete { get; set; }
    }
}
