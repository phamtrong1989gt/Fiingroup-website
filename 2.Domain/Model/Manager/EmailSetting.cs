using PT.Domain.Seedwork;
using System.ComponentModel.DataAnnotations;

namespace PT.Domain.Model
{
    public class EmailSetting : IAggregateRoot
    {
        public int Id { get; set; }

        public int PortalId { get; set; }

        [Display(Name = "Email server")]
        public string EmailServer { get; set; }

        [Display(Name = "Mật khẩu hoặc token")]
        public string Password { get; set; }

        [Display(Name = "Port")]
        public int Port { get; set; }

        [Display(Name = "Host")]
        public string Host { get; set; }

        [Display(Name = "From")]
        public string From { get; set; }

        [Display(Name = "CC")]
        public string CC { get; set; }
    }
}
