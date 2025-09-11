using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Domain.Model
{
    public class EmailSettings
    {
        [Display(Name = "Google email SMTP")]
        [EmailAddress(ErrorMessage = "Không đúng định dạng email!")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public string Email { get; set; }

        [Display(Name = "Mật khẩu Email SMTP")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public string Password { get; set; }

        [Display(Name = "Cổng")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public int Port { get; set; }

        [Display(Name = "Host")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public string Host { get; set; }

        [Display(Name = "Gửi từ")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public string From { get; set; }

        //// Liên hệ 
        //[Display(Name = "Email gửi đến khi liên hệ thành công")]
        //[Required(ErrorMessage = "{0} không được để trống!")]
        //[StringLength(500, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 15)]
        //public string ToEmailContact { get; set; }
        //[Display(Name = "Tiêu đề Email")]
        //[Required(ErrorMessage = "{0} không được để trống!")]
        //[StringLength(500, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 15)]
        //public string SubjectContact { get; set; }
        //[Display(Name = "Nội dung Email")]
        //[Required(ErrorMessage = "{0} không được để trống!")]
        //[StringLength(500, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 20)]
        //public string BodyContact { get; set; }
        //[Display(Name = "Nội dung thông báo khi gửi liên hệ thành công")]
        //[Required(ErrorMessage = "{0} không được để trống!")]
        //public string TextSuccessContact { get; set; }
        //public bool IsContact { get; set; }
        //// Sản phẩm
        //[Display(Name = "Email gửi đến khi có email đặt hàng")]
        //[StringLength(500, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 15)]
        //public string ToEmailProduct { get; set; }
        //[Display(Name = "Tiêu đề Email")]
        //[StringLength(500, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 15)]
        //public string SubjectProduct { get; set; }
        //[Display(Name = "Nội dung Email")]
        //public string BodyProduct { get; set; }
        //[Display(Name = "Nội dung thông báo khi đặt hàng thành công")]
        //[StringLength(500, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 15)]
        //public string TextSuccessProduct { get; set; }
        //public bool IsSendProduct { get; set; }

    }
}
