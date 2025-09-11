using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PT.Domain.Model
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "{0} không được để trống!")]
        [EmailAddress(ErrorMessage = "Không đúng định dạng email")]
        public string Email { get; set; }
        public string Capcha { get; set; }
    }
}
