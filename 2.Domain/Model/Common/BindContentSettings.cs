using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Domain.Model
{
    public class BindContentSettings
    {
        [Display(Name = "Chèn nội dung vào head")]
        [StringLength(10000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Head { get; set; }
        [Display(Name = "Chèn nội dung vào body")]
        [StringLength(10000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Body { get; set; }
        [Display(Name = "Chèn nội dung vào footer")]
        [StringLength(10000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Footer { get; set; }
    }
}
