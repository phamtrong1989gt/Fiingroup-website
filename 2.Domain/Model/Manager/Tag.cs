using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;

namespace PT.Domain.Model
{
    public enum EPaymentTransactionStatus: byte
    {
        KhoiTao = 0,
        ThanhCong = 1,
        ThatBai = 2
    }

    public enum EPaymentTransactionType: byte
    {
        [Display(Name = "Thanh toán Paypal")]
        Paypal = 0,

        [Display(Name = "Đặt trước")]
        Booking = 1
    }

    public class PaymentTransaction: IAggregateRoot
    {
        public int Id { get; set; }

        public int TourId { get; set; }

        [MaxLength(100)]
        public string Code { get; set; }


        [MaxLength(100)]
        public string GuidId { get; set; }

        [MaxLength(100)]
        public string PicUp { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(2000)]
        public string Note { get; set; }

        public int Adult { get; set; }

        public int Children { get; set; }

        public int Elderly { get; set; }

        public int Infant { get; set; }

        [Display(Name = "Giá")]
        public decimal AdultPrice { get; set; }

        [Display(Name = "Giá trẻ em")]
        public decimal ChildrenPrice { get; set; }

        [Display(Name = "Giá người già 60+")]
        public decimal ElderlyPrice { get; set; }

        [Display(Name = "Giá trẻ em dười 2 tuổi")]
        public decimal InfantPrice { get; set; }

        public decimal Total { get; set; }

        public EPaymentTransactionStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public EPaymentTransactionType Type { get; set; }

        [NotMapped]
        public Tour Tour { get; set; }

        [MaxLength(100)]
        public string Phone { get; set; }
    }

    public class Tag : IAggregateRoot
    {
        public int Id { get; set; }
        public int PortalId { get; set; }
        public string Name { get; set; }
        public string Banner { get; set; }
        public string Content { get; set; }
        public bool Status { get; set; }


        [MaxLength(10)]
        public string Language { get; set; }
        [NotMapped]
        public virtual Link Link { get; set; }
        [NotMapped]
        public BaseSearchModel<List<ContentPage>> ContentPages { get; set; }
        [NotMapped]
        public List<LinkReference> LinkReferences { get; set; }
        [NotMapped]
        public Portal Portal { get; set; }

        [NotMapped]
        public string FullPath { get; set; }
    }
    public class TagModel:SeoModel
    {
        public int Id { get; set; }
        [Display(Name = "Tên tag")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 1)]
        public string Name { get; set; }
        public string Banner { get; set; }
        public CategoryType SlugType { get; set; } = CategoryType.Page;
        [Display(Name = "Nội dung")]
        public string Content { get; set; }
        public SelectList PortalSelectList { get; set; }
    }
    public class AddTagModel 
    {
        public int Id { get; set; }
        [Display(Name = "Tên tag")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        [StringLength(1000, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 1)]
        public string Name { get; set; }
        public string Language { get; set; }

    }
}
