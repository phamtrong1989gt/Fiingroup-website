using System.ComponentModel.DataAnnotations;

namespace PT.Domain.Model
{
    // Quy định các bảng để biết type nào query qua bảng nào
    public enum ESlugType: int
    {
        [Display(Name ="Danh mục")]
        Category = 1,
        [Display(Name = "Tag")]
        Tag = 2,
        [Display(Name = "Trang nội dung")]
        ContentPage = 3,
        [Display(Name = "Trang thủ công")]
        Static = 10,
        //[Display(Name = "Trang nhân viên")]
        //Employee = 11,
        //// Trang thư viện hình ảnh
        //[Display(Name = "Trang hình ảnh")]
        //ImageGallery = 12,
        //Tour = 13,
        //TourType = 14,
        //FAQ = 15
    }
    // Qiy định type của category để khi vào các category biết được là loại gì để hiển thị các input tương ứng
    public enum ECategoryType: int
    {
        [Display(Name = "Tin bài")]
        ContentPage_Blog = 100,
        [Display(Name = "Dịch vụ")]
        ContentPage_Service = 101,
        [Display(Name = "Giải pháp")]
        ContentPage_Solution = 102,
        [Display(Name = "Sự kiện")]
        ContentPage_Event = 103,
        [Display(Name = "Ấn phẩm")]
        ContentPage_Publications = 104,
        [Display(Name = "Báo cáo")]
        ContentPage_Report = 105,
        [Display(Name = "Sản phẩm")]
        ContentPage_Product = 106,
        [Display(Name = "Trang nội dung")]
        ContentPage_Page = 107,
        [Display(Name = "Câu hỏi thường gặp")]
        ContentPage_FAQ = 108,
        [Display(Name = "Trang hỗ trợ dòng chảy")]
        ContentPage_Flow = 109,
        [Display(Name = "Trang sản phẩm dòng chảy")]
        ContentPage_FlowItems = 110,
    }
    public enum CategoryType
    {
        [Display(Name ="Tin bài")]
        CategoryBlog = 1,

        [Display(Name = "Dịch vụ")]
        CategoryService = 8,

        [Display(Name = "Hỗ trợ dòng chảy")]
        CategoryFlowSupportService = 100,

        [Display(Name = "Sự kiện")]
        CategoryEvent = 110,

        [Display(Name = "Ấn phẩm")]
        CategoryPublications = 120,

        [Display(Name = "Báo cáo")]
        CategoryReport = 130,

        [Display(Name = "Sản phẩm")]
        CategoryProduct = 16,

        [Display(Name = "Tag")]
        Tag = 2,

        [Display(Name = "Trang tin")]
        ContentPageBlog = 3,
       
        [Display(Name = "Trang nội dung")]
        ContentPagePage = 5,

        [Display(Name = "Trang cố định")]
        Static = 10
    }
}
