using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;

namespace PT.Domain.Model
{
    /// <summary>
    /// Entity đại diện cho thông tin tĩnh trong hệ thống, dùng cho lưu trữ dữ liệu ở database.
    /// </summary>
    public class StaticInformation : IAggregateRoot
    {
        /// <summary>
        /// Khóa chính của thông tin tĩnh
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Id cha (nếu có), dùng cho phân cấp thông tin tĩnh
        /// </summary>
        public int ParentId { get; set; }
        /// <summary>
        /// Tên thông tin tĩnh
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Mã định danh thông tin tĩnh
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Nội dung chi tiết của thông tin tĩnh
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Trạng thái sử dụng (true: đang sử dụng, false: không sử dụng)
        /// </summary>
        public bool Status { get; set; }
        /// <summary>
        /// Ngôn ngữ hiển thị (ví dụ: "vi", "en")
        /// </summary>
        [MaxLength(10)]
        public string Language { get; set; }
        /// <summary>
        /// Đánh dấu đã xóa (true: đã xóa, false: còn sử dụng)
        /// </summary>
        public bool Delete { get; set; }
        /// <summary>
        /// Dữ liệu mở rộng (nếu có)
        /// </summary>
        public string Data { get; set; }

        public int PortalId { get; set; }
        [NotMapped]
        public Portal Portal { get; set; }
    }

    /// <summary>
    /// Model dùng cho binding dữ liệu giữa giao diện và controller (ViewModel).
    /// </summary>
    public class StaticInformationModel
    {
        /// <summary>
        /// Khóa chính của thông tin tĩnh
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Tên thông tin tĩnh
        /// </summary>
        [Display(Name = "Tên")]
        public string Name { get; set; }
        /// <summary>
        /// Mã định danh thông tin tĩnh
        /// </summary>
        [Display(Name = "Mã")]
        public string Code { get; set; }
        /// <summary>
        /// Id cha (nếu có), dùng cho phân cấp thông tin tĩnh
        /// </summary>
        public int ParentId { get; set; }
        /// <summary>
        /// Nội dung chi tiết của thông tin tĩnh
        /// </summary>
        [Display(Name = "Nội dung")]
        public string Content { get; set; }
        /// <summary>
        /// Trạng thái sử dụng (true: đang sử dụng, false: không sử dụng)
        /// </summary>
        [Display(Name = "Sử dụng dữ liệu này")]
        public bool Status { get; set; }
        /// <summary>
        /// Ngôn ngữ hiển thị (ví dụ: "vi", "en")
        /// </summary>
        [MaxLength(10)]
        public string Language { get; set; }
        /// <summary>
        /// Đánh dấu đã xóa (true: đã xóa, false: còn sử dụng)
        /// </summary>
        public bool Delete { get; set; }

        [Display(Name = "Sử dụng dữ liệu này")]
        [Required(ErrorMessage = "Cổng")]
        public int PortalId { get; set; }

        public SelectList PortalSelectList { get; set; }
    }
}
