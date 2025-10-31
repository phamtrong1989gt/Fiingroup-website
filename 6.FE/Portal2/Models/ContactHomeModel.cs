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
        [Required(ErrorMessage = "The {0} field is required")]
        [StringLength(50, MinimumLength =1)]
        public string FullName { get; set; }

        [Display(Name = "CountryId")]
       // [Required(ErrorMessage = "The {0} field is required")]
        public int? CountryId { get; set; }

        [Display(Name = "PhoneCode")]
        public string PhoneCode { get; set; }

        [Display(Name = "Phone")]
        [StringLength(20, MinimumLength = 0)]
        public string Phone { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "The {0} field is required")]
        [StringLength(100, MinimumLength = 10)]
        public string Email { get; set; }

        [Display(Name = "Content")]
        [Required(ErrorMessage = "The {0} field is required")]
        [StringLength(200, MinimumLength = 10)]
        public string Content { get; set; }

        public string Language { get; set; }
        public string Capcha { get; set; }
        public SelectList CountrySelectlist { get;  set; }
    }


}
