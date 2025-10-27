using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PT.Domain.Model.Common;

namespace PT.Domain.Model
{
    public class PaypalSettings
    {
        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string Secret { get; set; }

    }

    public class BaseSettings
    {
        [Display(Name = "Ngôn ngữ mặc định")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public string DefaultLanguage { get; set; }

        [Display(Name = "Định dạng ảnh")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(100, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string ImagesType { get; set; }

        [Display(Name = "Kích thước ảnh cho phép")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public double ImagesMaxSize { get; set; }

        [Display(Name = "Kích thước tài liệu cho phép")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public double DocumentsMaxSize { get; set; }

        [Display(Name = "Định dạng tài liệu cho phép")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public string DocumentsType { get; set; }

        [Display(Name = "Kích thước video cho phép")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public double VideosMaxSize { get; set; }

        [Display(Name = "Định dạng video cho phép")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public string VideosType { get; set; }

        public bool MultipleLanguage { get; set; }
        [Display(Name = "Trạng thái capcha")]
        public bool IsCapCha { get; set; }
        [Display(Name = "Capcha Data Site Key")]
        public string CapChaDataSitekey { get; set; }
        [Display(Name = "Capcha Secret")]
        public string CapChaSecret { get; set; }

        [Display(Name = "Google map Key")]
        public string GoogleMapKey { get; set; }

        public string EmailManager { get; set; }

        public bool IsWebsiteHotel { get; set; }
        public bool IsWebsiteProduct { get; set; }

        [Display(Name = "Client Id FaceBook")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string ClientIdFaceBook { get; set; }

        [Display(Name = "Client Secret FaceBook")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string ClientSecretFaceBook { get; set; }

        [Display(Name = "Có thông tin liên hệ gửi email tới")]
        public string ToEmail { get; set; }

        public int TimeOutSendRequest { get; set; }
        public bool MultiDomain { get; set; }
        public string RootDomin { get; set; }
        public int ImageMaxWith { get; set; }
        public bool IsHttps { get; set; }
        public int PortalId { get; set; }
    }
}
