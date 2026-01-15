using System.ComponentModel.DataAnnotations;

namespace PT.Domain.Model.Common
{
    /// <summary>
    /// MISA CRM API Configuration Settings
    /// </summary>
    public class MisaSettings
    {
        [Display(Name = "Client ID")]
        [Required(ErrorMessage = "{0} is required")]
        public string ClientId { get; set; }

        [Display(Name = "Client Secret")]
        [Required(ErrorMessage = "{0} is required")]
        public string ClientSecret { get; set; }

        [Display(Name = "API Get Token URL")]
        [Required(ErrorMessage = "{0} is required")]
        public string APIGetTokenURL { get; set; }

        [Display(Name = "API Created Contact URL")]
        [Required(ErrorMessage = "{0} is required")]
        public string APICreatedContact { get; set; }
        [Display(Name = "Form Layout")]
        public string FormLayout { get; set; }
        public string Department { get; set; }
        public string TitleSolution { get; set; }
        public string DepartmentSolution { get; set; }
        public string FormLayoutSolution { get; set; }
    }
}
