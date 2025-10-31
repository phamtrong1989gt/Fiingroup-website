using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static PT.Domain.Model.Contact;

namespace PT.BE.Models
{
    public class HomeContactAppointmentModel
    {

        [Display(Name = "ServiceId")]
        [Required(ErrorMessage = "The {0} field is required")]
        public int? ServiceId { get; set; }

        [Display(Name = "Age")]
        [Required(ErrorMessage = "The {0} field is required")]
        public int Age { get; set; }

        [Display(Name = "CountryId")]
        [Required(ErrorMessage = "The {0} field is required")]
        public int? CountryId { get; set; }
        [Display(Name = "PhoneCode")]
        public string PhoneCode { get; set; }

        [Display(Name = "FullName")]
        [Required(ErrorMessage = "The {0} field is required")]
        public string FullName { get; set; }

        [Display(Name = "Phone")]
        [Required(ErrorMessage = "The {0} field is required")]
        public string Phone { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Content")]
        public string Content { get; set; }

        [Display(Name = "AppointmentDate")]
        [Required(ErrorMessage = "The {0} field is required")]
        public DateTime? AppointmentDate { get; set; }

        [Display(Name = "AppointmentTime")]
        [Required(ErrorMessage = "The {0} field is required")]
        public string AppointmentTime { get; set; }

        public SelectList ServiceSelectList { get; set; }
        public SelectList CountrySelectlist { get;  set; }
        public string Capcha { get; set; }
    }
}
