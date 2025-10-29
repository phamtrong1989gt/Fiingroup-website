using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PT.Domain.Model
{
    public enum TourStyle
    {
        [Display(Name = "Tour")]
        Tour = 0,
        [Display(Name = "Book car")]
        Car = 1,
        [Display(Name = "Book room")]
        Hotel = 2
    }

    public enum ETourType
    {
        [Display(Name = "Culture and history")]
        CultureAndHistory = 0,
        [Display(Name = "Family holiday")]
        FamilyHoliday,
        [Display(Name = "Cruises and sea")]
        CruisesAndSea,
        [Display(Name = "Romance and honeymoon")]
        RomanceAndHhoneyMoon,
        [Display(Name = "Luxury and beach escapes")]
        LuxuryAndBeachEscapes,
        [Display(Name = "Cycling and trekking")]
        CyclingAndTrekking,
        [Display(Name = "Nature and wildlife")]
        NatureAndWildLife,
        [Display(Name = "Yoga and meditation")]
        YogaAndMeditation,
        [Display(Name = "Education and charity")]
        EducationAndCharity,
        [Display(Name = "Photo and culinary")]
        PhotoAndCulinary,
        [Display(Name = "Special tour")]
        SpecialTour,
    }

    public class Tour : IAggregateRoot
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Summary { get; set; }
        public string Banner { get; set; }
        public string BannerHeader { get; set; }
        public string BannerFooter { get; set; }
        public int Days { get; set; }
        public int Nights { get; set; }
        public TourStyle Style { get; set; }
        public int TourTypeId { get; set; }
        public string Overview { get; set; }
        public string Trips { get; set; }
        public string DetailDifference { get; set; }
        public string DetailServicesInclusion { get; set; }
        public string DetailServicesExclusion { get; set; }
        public string DetailNote { get; set; }
        public string Photos { get; set; }
        public bool IsTop { get; set; }
        [MaxLength(10)]
        public string Language { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedUser { get; set; }

        [NotMapped]
        public Link Link { get; set; }
        [NotMapped]
        public List<Category> Categorys { get; set; }
        [NotMapped]
        public List<LinkReference> LinkReferences { get; set; }
        [NotMapped]
        public List<TourDay> TourDays { get; set; }

        public decimal? AdultPrice { get; set; }
        public decimal? ElderlyPrice { get; set; }
        public decimal? ChildrenPrice { get; set; }
        public decimal? InfantPrice { get; set; }

        [MaxLength(100)]
        public string From { get; set; }

        [MaxLength(100)]
        public string To { get; set; }

        [MaxLength(50)]
        public string PickUp { get; set; }

        [MaxLength(50)]
        public string PickOut { get; set; }

        public int? Order { get; set; } = 0;
    }

    public class TourModel : SeoModel
    {
        public int Id { get; set; }

        [Display(Name = "Tên tour")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 1)]
        public string Name { get; set; }
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }
        public string Summary { get; set; }

        public string Banner { get; set; }
        public string BannerHeader { get; set; }
        public string BannerFooter { get; set; }

        [Display(Name = "Số ngày")]
        public int Days { get; set; }

        [Display(Name = "Số đêm")]
        public int Nights { get; set; }

        [Display(Name = "Phân nhóm")]
        public TourStyle Style { get; set; }

        [Display(Name = "Overview")]
        public string Overview { get; set; }

        [Display(Name = "The stages of your trip")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 1)]
        public string Trips { get; set; }

        [Display(Name = "Why choose us")]
        public string DetailDifference { get; set; }

        [Display(Name = "Included/Excluded")]
        public string DetailServicesInclusion { get; set; }

        [Display(Name = "SERVICES EXCLUSION")]
        public string DetailServicesExclusion { get; set; }

        [Display(Name = "Package details")]
        public string DetailNote { get; set; }
        [Display(Name = "Phận loại tour")]
        public int TourTypeId { get; set; }

        public string Photos { get; set; }
        public bool IsTop { get; set; }
        public int CreatedDate { get; set; }
        public int CreatedUser { get; set; }
        public string CategoryIds { get; set; }

        [NotMapped]
        public List<FileDataModel> PhotoFileDatas { get; set; } = new List<FileDataModel>();
        public SelectList TourTypeSelectListItems { get; set; }
        public List<SelectListItem> TourStyleSelectListItems { get; set; }

        [Display(Name = "Giá")]
        public decimal? AdultPrice { get; set; }

        [Display(Name = "Giá trẻ em")]
        public decimal? ChildrenPrice { get; set; }

        [Display(Name = "Giá người già 60+")]
        public decimal? ElderlyPrice { get; set; }

        [Display(Name = "Giá trẻ em dười 2 tuổi")]
        public decimal? InfantPrice { get; set; }

        [Display(Name = "Điểm đi")]
        public string From { get; set; }

        [Display(Name = "Điểm đến")]
        public string To { get; set; }

        [Display(Name = "Thời gian khởi hành")]
        public string PickUp { get; set; }

        [Display(Name = "Thời gian đến nơi")]
        public string PickOut { get; set; }

        [Display(Name = "Thứ tự")]
        public int? Order { get; set; } = 0;
    }
}
