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

    public class NewAPISettings
    {

        [Display(Name = "Token Endpoint")]
        public string TokenEndpoint { get; set; }

        [Display(Name = "News Endpoint VI")]
        public string NewsEndpointVI { get; set; }

        [Display(Name = "News Endpoint EN")]
        public string NewsEndpointEN { get; set; }

        [Display(Name = "Category ID VI")]
        public List<int> CategoryIdVI { get; set; }

        [Display(Name = "Category ID EN")]
        public List<int> CategoryIdEN { get; set; }

        [Display(Name = "Grant Type")]
        public string GrantType { get; set; }

        [Display(Name = "Client ID")]
        public string ClientId { get; set; }

        [Display(Name = "Client Secret")]
        public string ClientSecret { get; set; }

        [Display(Name = "Scope")]
        public string Scope { get; set; }

        [Display(Name = "Username")]
        public string Username { get; set; }

        [Display(Name = "Password")]
        public string Password { get; set; }

        // ============ NEW RATING API ENDPOINTS ============
        [Display(Name = "Report Scores Endpoint")]
        public string ReportScoresEndpoint { get; set; }

        [Display(Name = "Report Industries Endpoint")]
        public string ReportIndustriesEndpoint { get; set; }

        [Display(Name = "Report Outlooks Endpoint")]
        public string ReportOutlooksEndpoint { get; set; }

        [Display(Name = "Sustainable Finance Endpoint")]
        public string SustainableFinanceEndpoint { get; set; }

        [Display(Name = "Rating Results Endpoint")]
        public string RatingResultsEndpoint { get; set; }

        // ============ NEW SUSTAINABLE FINANCE API ENDPOINTS ============
        [Display(Name = "Sustainable Industries Endpoint")]
        public string SustainableIndustriesEndpoint { get; set; }

        [Display(Name = "Sustainable Standards Endpoint")]
        public string SustainableStandardsEndpoint { get; set; }
        public string IssuerTypeEndpoint { get; set; }
        public string OpinionTypesEndpoint { get; set; }
        public string IssuerOrgansEndpoint { get; set; }
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
        public int TimeCache { get; set; }
        public string DataPath { get; set; }
        public string BackendDomain { get; set; }

        public NewAPISettings NewAPI { get; set; }
        public NewAPISettings RatingAPI { get; set; }
        public string DataPathFR { get; set; }
    }

    public class AsyncNewsSettings
    {
        [Display(Name = "Token Endpoint")]
        public string TokenEndpoint { get; set; }

        [Display(Name = "News Endpoint VI")]
        public string NewsEndpointVI { get; set; }

        [Display(Name = "News Endpoint EN")]
        public string NewsEndpointEN { get; set; }

        [Display(Name = "Category ID VI")]
        public List<int> CategoryIdVI { get; set; }

        [Display(Name = "Category ID EN")]
        public List<int> CategoryIdEN { get; set; }

        [Display(Name = "Grant Type")]
        public string GrantType { get; set; }

        [Display(Name = "Client ID")]
        public string ClientId { get; set; }

        [Display(Name = "Client Secret")]
        public string ClientSecret { get; set; }

        [Display(Name = "Scope")]
        public string Scope { get; set; }

        [Display(Name = "Username")]
        public string Username { get; set; }

        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Created Endpoint VI")]
        public string CreatedEndPointVI { get; set; }

        [Display(Name = "Edit Endpoint VI")]
        public string EditEndPointVI { get; set; }

        [Display(Name = "Delete Endpoint VI")]
        public string DeleteEndPointVI { get; set; }

        [Display(Name = "Get Endpoint VI")]
        public string GetEndPointVI { get; set; }

        [Display(Name = "Created Endpoint EN")]
        public string CreatedEndPointEN { get; set; }

        [Display(Name = "Edit Endpoint EN")]
        public string EditEndPointEN { get; set; }

        [Display(Name = "Delete Endpoint EN")]
        public string DeleteEndPointEN { get; set; }

        [Display(Name = "Get Endpoint EN")]
        public string GetEndPointEN { get; set; }

        [Display(Name = "GetCategorys")]
        public string GetCategorys { get; set; }
    }
}
