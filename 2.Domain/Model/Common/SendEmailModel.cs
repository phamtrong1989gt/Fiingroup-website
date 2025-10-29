using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PT.Domain.Model
{
    public class SendEmailModel
    {
        public int ConfirmationTripsId { get; set; }
        [Display(Name ="Gửi từ")]
        public string From { get; set; }
        [Display(Name = "Gửi đến")]
        public string To { get; set; }
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }
        [Display(Name = "Nội dung")]
        [StringLength(1000, ErrorMessage = "{0} ít nhất phải là {2} và tối đa {1} ký tự.", MinimumLength = 0)]
        public string Content { get; set; }
    }
}
