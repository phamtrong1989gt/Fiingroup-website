using System.ComponentModel.DataAnnotations;

namespace PT.Domain.Model
{
    public enum CategoryType
    {
        [Display(Name ="Tin bài")]
        CategoryBlog = 1,

        [Display(Name = "Dịch vụ")]
        CategoryService = 8,

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
    }
}
