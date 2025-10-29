using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using PT.Domain.Seedwork;

namespace PT.Domain.Model
{
    public class ApplicationRole : IdentityRole<int>, IAggregateRoot
    {
        [Display(Name= "Description")]
        public string Description { get; set; }
        public RoleManagerType Type { get; set; }
    }

}
