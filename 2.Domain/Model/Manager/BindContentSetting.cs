using PT.Domain.Seedwork;
using System.ComponentModel.DataAnnotations;

namespace PT.Domain.Model
{
    public class BindContentSetting : IAggregateRoot
    {
        public int Id { get; set; }
        public int PortalId { get; set; }
        [Display(Name = "Chèn nội dung vào head")]
        public string Head { get; set; }
        [Display(Name = "Chèn nội dung vào body")]
        public string Body { get; set; }
        [Display(Name = "Chèn nội dung vào footer")]
        public string Footer { get; set; }
    }
}
