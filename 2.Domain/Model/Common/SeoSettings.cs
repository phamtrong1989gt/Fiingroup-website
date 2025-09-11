using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Domain.Model
{
    public class SeoSettings
    {
        public string Id { get; set;}

        [Display(Name = "Tần số thay đổi(Changefreq)")]
        [StringLength(100, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Changefreq { get; set; }

        [Display(Name = "Sửa đổi gần nhất(Lastmod)")]
        [StringLength(100, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Lastmod { get; set; }

        [Display(Name = "Seo Title mặc định")]
        [StringLength(500, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Title { get; set; }
        [Display(Name = "Meta Description mặc định")]
        [StringLength(500, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Description { get; set; }

        [Display(Name = "Keywords mặc định")]
        [StringLength(500, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Keywords { get; set; }

        [Display(Name = "Script Google Analytics")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Analytics { get; set; }

        [Display(Name = "Script Facebook Pixel ID")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string FacebookPixelID { get; set; }

        [Display(Name = "Mã meta xác minh google")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string MetaGoogle { get; set; }

        [Display(Name = "Script ứng dụng chat hỗ trợ")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string AppSubport { get; set; }

        

        [Display(Name = "Nội dung file robots.txt")]
        [StringLength(4000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Robots { get; set; }
    }
}
