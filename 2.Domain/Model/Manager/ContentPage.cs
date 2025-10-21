using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Domain.Model
{

    public class ContentPage : IAggregateRoot
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int CategoryId { get; set; }
        [MaxLength(1000)]
        public string Name { get; set; }

        [MaxLength(2000)]
        public string Summary { get; set; }
        public string Content { get; set; }
        [MaxLength(1000)]
        public string Banner { get; set; }
        [MaxLength(10)]
        public string Language { get; set; }
        public CategoryType Type { get; set; } = CategoryType.Blog;
        public string Author { get; set; }
        public DateTime DatePosted { get; set; }
        public bool Status { get; set; }
        public bool Delete { get; set; }
        public int PortalId { get; set; } = 1;
        public double? Price { get; set; }
        [NotMapped]
        public  Link Link { get; set; }
        [NotMapped]
        public  List<Category> Categorys { get; set; }
        [NotMapped]
        public  List<Tag> Tags { get; set; }
        [NotMapped]
        public  ContentPage Serice { get; set; }
        [NotMapped]
        public Category Category { get; set; }
        [NotMapped]
        public List<Category> ServiceCategorys { get; set; }
        public bool IsHome { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [NotMapped]
        public List<LinkReference> LinkReferences { get; set; }

        [NotMapped]
        public Portal Portal { get; set; }

    }
    public class BlogModel :SeoModel
    {
        public int Id { get; set; }
        [Display(Name = "Danh mục tin")]
        public int CategoryId { get; set; }

        [Display(Name="Tên bài viết")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Name { get; set; }

        [Display(Name = "Summary")]
        public string Summary { get; set; }


        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [Display(Name = "Hình đại diện")]
        public string Banner { get; set; }

        public string Folder { get; set; }

        [Display(Name = "Thời gian đăng")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public DateTime DatePosted { get; set; }

        public virtual Category Category { get; set; }

        [Display(Name = "Tags")]
        public List<int> TagIds { get; set; }

        [Display(Name = "Tác giả")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string Author { get; set; }

        public string ContentPageRelatedIds { get; set; }
       
        public string RelatedString { get; set; }

        public MultiSelectList TagSelectList { get; set; }

        public string CategoryIds { get; set; }

        public string ReferenceString { get; set; }

        public string ContentPageReferenceIds { get; set; }
        public SelectList PortalSelectList { get; set; }

        public List<PortalSharedModel> PortalShareds { get; set; }

        public List<int> SharedPortalIds { get; set; }
    }
    public class PortalSharedModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
    }

    public class ServiceModel:SeoModel
    {
        public int Id { get; set; }
        [Display(Name = "Tên dịch vụ")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Name { get; set; }
        [Display(Name = "Tóm tắt")]
        [StringLength(2000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Summary { get; set; }
        [Display(Name = "Nội dung")]
        public string Content { get; set; }
        [Display(Name = "Hình đại diện")]
        public string Banner { get; set; }
     
        public string ContentPageRelatedIds { get; set; }

        public string Folder { get; set; }
        public virtual Category Category { get; set; }
        [Display(Name = "Tags")]
        public List<int> TagIds { get; set; }
        public CategoryType SlugType { get; set; } = CategoryType.Service;
        public MultiSelectList TagSelectList { get; set; }
        public string CategoryIds { get; set; }
        public string ReferenceString { get; set; }
        public string ContentPageReferenceIds { get; set; }
        public string RelatedString { get; set; }
    }

    public class PageModel:SeoModel
    {
        public int Id { get; set; }
        [Display(Name = "Tên trang")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Name { get; set; }
        [Display(Name = "Tóm tắt")]
        [StringLength(2000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Summary { get; set; }
        [Display(Name = "Nội dung")]
        public string Content { get; set; }
        [Display(Name = "Hình đại diện")]
        public string Banner { get; set; }
        public string Folder { get; set; }
        [Display(Name = "Tags")]
        public List<int> TagIds { get; set; }
        public MultiSelectList TagSelectList { get; set; }
        public SelectList PortalSelectList { get; set; }
        public CategoryType SlugType { get; set; } = CategoryType.Page;
    }
  
    public class FAQModel:SeoModel
    {
        public int Id { get; set; }
        [Display(Name = "Tên câu hỏi")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Name { get; set; }
        [Display(Name = "Tóm tắt")]
        [StringLength(2000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Summary { get; set; }
        [Display(Name = "Nội dung")]
        public string Content { get; set; }
        [Display(Name = "Câu hỏi của danh mục dịch vụ")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "{0} không được để trống")]
        public int CategoryId { get; set; }
        [Display(Name = "Hình đại diện")]
        public string Banner { get; set; }
        [Display(Name = "Tags")]
        public List<int> TagIds { get; set; }
        public CategoryType SlugType { get; set; } = CategoryType.FAQ;
        public MultiSelectList TagSelectList { get; set; }
        [Display(Name = "Thời gian đăng")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public DateTime DatePosted { get; set; }
        [Display(Name = "Tác giả")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string Author { get; set; }
        public SelectList ServiceSelectList { get; set; }
        public List<SelectListItem> CategorySelectList { get; set; }

        [Display(Name = "Đặt ngoài trang chủ")]
        public bool IsHome { get; set; }
    }

    public class PromotionInformationModel : SeoModel
    {
        public int Id { get; set; }
        [Display(Name = "Tên trang")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Name { get; set; }
        [Display(Name = "Tóm tắt")]
        [StringLength(2000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Summary { get; set; }
        [Display(Name = "Nội dung")]
        public string Content { get; set; }
        [Display(Name = "Hình đại diện")]
        public string Banner { get; set; }
        public string Folder { get; set; }
        [Display(Name = "Tags")]
        public List<int> TagIds { get; set; }
        public MultiSelectList TagSelectList { get; set; }
        public CategoryType SlugType { get; set; } = CategoryType.PromotionInformation;

        [Display(Name = "Khuyến mãi hiệu lực từ")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public DateTime? StartDate { get; set; }
        [Display(Name = "Khuyến mãi hiệu lực đến")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Thời gian đăng")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public DateTime DatePosted { get; set; }
    }

    public class ProductModel : SeoModel
    {
        public int Id { get; set; }

        [Display(Name = "Danh mục tin")]
        public int CategoryId { get; set; }

        [Display(Name = "Tên bài viết")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Name { get; set; }

        [Display(Name = "Specification")]
        public string Specification { get; set; }

        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [Display(Name = "Hình đại diện")]
        public string Banner { get; set; }

        public string Folder { get; set; }

        public virtual Category Category { get; set; }

        public string CategoryIds { get; set; }

        [NotMapped]
        public List<FileDataModel> PhotoFileDatas { get; set; } = new List<FileDataModel>();

        public string Images { get; set; }
    }
}
