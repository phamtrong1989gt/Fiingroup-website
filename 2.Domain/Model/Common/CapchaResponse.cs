using System;
using System.Collections.Generic;
using System.Text;

namespace PT.Domain.Model
{
    public class CapchaResponse
    {
        public bool Success { get; set; }
        public DateTime Challenge_ts { get; set; }
        public string Hostname { get; set; }
        public object Error_codes { get; set; }
    }
}
