using System.ComponentModel.DataAnnotations;

namespace PT.Domain.Model
{
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

        [Display(Name = "Tag")]
        Tag = 2,

        [Display(Name = "Tin tức")]
        Blog = 3,

        [Display(Name = "Dịch vụ")]
        Service = 4,

        [Display(Name = "Trang nội dung")]
        Page = 5,

        [Display(Name = "Câu hỏi thường gặp")]
        FAQ = 6,

        [Display(Name = "Nhân viên")]
        Employee = 7,

        [Display(Name = "Thư viên ảnh")]
        ImageGallery = 9,

        [Display(Name = "Trang cố định")]
        Static = 10,

        [Display(Name = "Thông tin khuyến mãi")]
        PromotionInformation = 12,

        [Display(Name = "Danh mục Tour")]
        CategoryTour = 11,

        [Display(Name = "Tour")]
        Tour = 13,

        [Display(Name = "Tour type")]
        TourType = 14,

        [Display(Name = "Tour style")]
        TourStyle = 15,

        [Display(Name = "Sản phẩm")]
        CategoryProduct = 16,

        [Display(Name = "Sản phẩm")]
        Product = 17,

        [Display(Name = "Hỗ trợ dòng chảy")]
        FlowSupportService = 101,

        [Display(Name = "Sự kiện")]
        Event = 111,

        [Display(Name = "Ấn phẩm")]
        Publications = 121,

        [Display(Name = "Báo cáo")]
        Report = 131,
    }
}
