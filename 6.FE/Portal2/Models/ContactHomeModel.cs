using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace PT.UI.Models
{
    public class ContactHomeModel
    {
        public int Id { get; set; }

        [Display(Name = "FullName")]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "ValidateStringLength")]
        public string FullName { get; set; }

        [Display(Name = nameof(Phone))]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "ValidateStringLength")]
        public string Phone { get; set; }

        [Display(Name = nameof(Email))]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "ValidateStringLength")]
        public string Email { get; set; }

        [Display(Name = nameof(ConpanyName))]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "ValidateStringLength")]
        public string ConpanyName { get; set; }

        [Display(Name = nameof(Position))]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "ValidateStringLength")]
        public string Position { get; set; }

        [Display(Name = nameof(Content))]
        [StringLength(500, MinimumLength = 0, ErrorMessage = "ValidateStringLength")]
        public string Content { get; set; }

        [Display(Name = nameof(Language))]
        public string Language { get; set; }

        [Display(Name = nameof(Capcha))]
        public string Capcha { get; set; }

        [Display(Name = nameof(ServiceId))]
        [Required(ErrorMessage = "ValidateRequired")]
        public int? ServiceId { get; set; }

        [Display(Name = nameof(Products))]
        [StringLength(100, MinimumLength = 0, ErrorMessage = "ValidateStringLength")]
        public string Products { get; set; }

        [Display(Name = nameof(CountrySelectlist))]
        public SelectList CountrySelectlist { get; set; }
    }

    public class ContactSolotionModel
    {
        public int Id { get; set; }

        [Display(Name = nameof(FullName))]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "ValidateStringLength")]
        public string FullName { get; set; }

        [Display(Name = nameof(Phone))]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "ValidateStringLength")]
        public string Phone { get; set; }

        [Display(Name = nameof(Email))]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "ValidateStringLength")]
        public string Email { get; set; }

        [Display(Name = nameof(Language))]
        public string Language { get; set; }

        [Display(Name = nameof(Capcha))]
        public string Capcha { get; set; }

        [Display(Name = nameof(Products))]
        [StringLength(100, MinimumLength = 0, ErrorMessage = "ValidateStringLength")]
        public string Products { get; set; }

        [Display(Name = nameof(ConpanyName))]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "ValidateStringLength")]
        public string ConpanyName { get; set; }

        [Display(Name = nameof(Position))]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "ValidateStringLength")]
        public string Position { get; set; }
    }

}
