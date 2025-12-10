using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PT.UI.Models
{
    public class ContactHomeModel
    {
        [Display(Name = nameof(Id))]
        public int Id { get; set; }

        [Display(Name = nameof(FullName))]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.")]
        public string FullName { get; set; }

        [Display(Name = nameof(Phone))]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.")]
        public string Phone { get; set; }

        [Display(Name = nameof(Email))]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.")]
        public string Email { get; set; }

        [Display(Name = nameof(ConpanyName))]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.")]
        public string ConpanyName { get; set; }

        [Display(Name = nameof(Position))]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.")]
        public string Position { get; set; }

        [Display(Name = nameof(Content))]
        [StringLength(500, MinimumLength = 0, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.")]
        public string Content { get; set; }

        [Display(Name = nameof(Language))]
        public string Language { get; set; }

        [Display(Name = nameof(Capcha))]
        public string Capcha { get; set; }

        [Display(Name = nameof(ServiceId))]
        [Required(ErrorMessage = "{0} không được để trống")]
        public int? ServiceId { get; set; }

        [Display(Name = nameof(Products))]
        [StringLength(100, MinimumLength = 0, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.")]
        public string Products { get; set; }

        [Display(Name = nameof(CountrySelectlist))]
        public SelectList CountrySelectlist { get; set; }
    }

    public class ContactSolotionModel
    {
        [Display(Name = nameof(Id))]
        public int Id { get; set; }

        [Display(Name = nameof(FullName))]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.")]
        public string FullName { get; set; }

        [Display(Name = nameof(Phone))]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.")]
        public string Phone { get; set; }

        [Display(Name = nameof(Email))]
        [Required(ErrorMessage = "{0}  không được để trống")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.")]
        public string Email { get; set; }

        [Display(Name = nameof(Language))]
        public string Language { get; set; }

        [Display(Name = nameof(Capcha))]
        public string Capcha { get; set; }

        [Display(Name = nameof(Products))]
        [StringLength(100, MinimumLength = 0, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.")]
        public string Products { get; set; }
    }

}
