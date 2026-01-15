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
        public int Id { get; set; }

        [Display(Name = "FullName")]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "ValidateStringLength")]
        public string FullName { get; set; }

        [Display(Name = "Phone")]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "ValidateStringLength")]
        public string Phone { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "ValidateStringLength")]
        public string Email { get; set; }

        [Display(Name = "ConpanyName")]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "ValidateStringLength")]
        public string ConpanyName { get; set; }

        [Display(Name = "Position")]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "ValidateStringLength")]
        public string Position { get; set; }

        [Display(Name = "Content")]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "ValidateStringLength")]
        public string Content { get; set; }
        public string Language { get; set; }
        public string Capcha { get; set; }

        [Display(Name = "ServiceId")]
        [Required(ErrorMessage = "ValidateRequired")]
        public int? ServiceId { get; set; }

        [Display(Name = "Products")]
        public string Products { get; set; }

        [Display(Name = "Type")]
        public int Type { get; set; }
        public SelectList CountrySelectlist { get;  set; }
    }

    public class ContactSolotionModel
    {
        public int Id { get; set; }
        [Display(Name = "FullName")]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "ValidateStringLength")]
        public string FullName { get; set; }
        [Display(Name = "Phone")]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "ValidateStringLength")]
        public string Phone { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "ValidateRequired")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "ValidateStringLength")]
        public string Email { get; set; }
        public string Language { get; set; }
        public string Capcha { get; set; }
        [Display(Name = "Products")]
        public string Products { get; set; }
        [Display(Name = "Type")]
        public int Type { get; set; }

        [Display(Name = "ConpanyName")]
        public string ConpanyName { get; set; }
        [Display(Name = "Position")]
        public string Position { get; set; }
    }

    public class ListRatingsModel
    {
        public string Language { get; set; }
        public int PortalId { get; set; }
    }

}
