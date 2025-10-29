using PT.Domain.Seedwork;
using System.ComponentModel.DataAnnotations;

namespace PT.Domain.Model
{
    public class TourGallery : IAggregateRoot
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string Name { get; set; }
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public bool Status { get; set; }
        public string Language { get; set; }
    }

    public class TourGalleryeModel
    {
        public int Id { get; set; }

        [Display(Name = "Tên")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        public string Name { get; set; }
        [Display(Name = "Sử dụng dữ liệu này")]
        public bool Status { get; set; }

        [Display(Name = "Hình ảnh 1")]
        public string Image1 { get; set; }

        [Display(Name = "Hình ảnh 2")]
        public string Image2 { get; set; }

        public bool Delete { get; set; }

        public string Language { get; set; }

        public int TourId { get; set; }
    }
}
