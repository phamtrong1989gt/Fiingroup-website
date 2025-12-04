
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static PT.Domain.Model.Contact;

namespace PT.UI.Models
{

    public class TestimonialHomeModel
    {
        public int Id { get; set; }
        public ContactType Type { get; set; } = ContactType.Testimonial;

        [Display(Name = "FullName")]
        [Required(ErrorMessage = "The {0} field is required")]
        [StringLength(50, MinimumLength = 1)]
        public string FullName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Content")]
        [Required(ErrorMessage = "The {0} field is required")]
        [StringLength(500, MinimumLength = 1)]
        public string Content { get; set; }

        [Display(Name = "Rating")]
        [Required(ErrorMessage = "The {0} field is required")]
        [Range(1, 5, ErrorMessage = "{0} from 1 to 5")]
        public double? Rating { get; set; }

        public string Language { get; set; }

        [Display(Name = "Check chọn thì nội dung này sẽ ưu tiên hiển thị ngoài trang index")]
        public bool IsHome { get; set; }
        public string Avatar { get; set; }
        public string Capcha { get; set; }
    }
}

