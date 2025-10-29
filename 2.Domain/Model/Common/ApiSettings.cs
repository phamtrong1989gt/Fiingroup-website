using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Domain.Model
{
    public class ApiSettings
    {
        public string TokenIssuer { get; set; }
        public string TokenAudience { get; set; }
        public string TokenKey { get; set; }
    }
}
