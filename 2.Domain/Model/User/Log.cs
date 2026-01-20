using System;
using System.ComponentModel.DataAnnotations;
using PT.Domain.Seedwork;

namespace PT.Domain.Model
{
    public enum LogType
    {
        [Display(Name = "Không xác định")]
        None = -1,
        [Display(Name = "Tạo mới dữ liệu")]
        Create,
        [Display(Name = "Cập nhật dữ liệu")]
        Edit,
        [Display(Name = "Xóa dữ liệu")]
        Delete,
        [Display(Name = "Điều khiển")]
        Control,
        
        // ============ NEW API LOG TYPES ============
        [Display(Name = "API - Lấy Token")]
        API_GetToken = 100,
        [Display(Name = "API - Tin tức (danh sách)")]
        API_GetNews = 101,
        [Display(Name = "API - Tin tức (chi tiết)")]
        API_GetNewsDetail = 102,
        [Display(Name = "API - Điểm xếp hạng")]
        API_GetReportScores = 103,
        [Display(Name = "API - Ngành nghề")]
        API_GetIndustries = 104,
        [Display(Name = "API - Triển vọng")]
        API_GetOutlooks = 105,
        [Display(Name = "API - Tài chính bền vững")]
        API_GetSustainableFinance = 106,
        [Display(Name = "API - Kết quả xếp hạng")]
        API_GetRatingResults = 107,
        [Display(Name = "API - Lỗi 401")]
        API_Error_Unauthorized = 200,
        [Display(Name = "API - Lỗi khác")]
        API_Error_Other = 201,
        [Display(Name = "API - Category")]
        API_Category = 202,
        [Display(Name = "Error")]
        Error = 203,
        API_GetIssuerTypes = 204,
        API_GetOpinionTypes = 205,
    }
  
    public class Log : IAggregateRoot
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int ObjectId { get; set; }
        [MaxLength(50)]
        public string Object { get; set; }
        [MaxLength(50)]
        public string ObjectType { get; set; }
        public DateTime ActionTime { get; set; }
        public string AcctionUser { get; set; }
        public LogType Type { get; set; }
    }
   
    public class WriteLogModel
    {
        public string Name { get; set; }
        public int ObjectId { get; set; }
    }
    
    public class LogModel
    {
        public string Name { get; set; }
        public int ObjectId { get; set; }
        public DateTime ActionTime { get; set; }
        public LogType Type { get; set; }
    }
    
    public class LogUserModel
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
    
    // ============ NEW API LOG MODEL ============
    public class APILogModel
    {
        public LogType Type { get; set; }
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public long DurationMs { get; set; }
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string Language { get; set; }
        public string RequestParams { get; set; }
        public int PortalId { get; set; }
    }
}
