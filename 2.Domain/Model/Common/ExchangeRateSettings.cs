using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Domain.Model
{
    public class ExchangeRateSettings
    {
        [Display(Name ="Tỉ giá USD 1$ = ? VNĐ")]
        public int DollarToVND { get; set; }
        [Display(Name = "Tỉ giá 1£ = ? VNĐ")]
        public int PoundToVND { get; set; }
        [Display(Name = "Tỉ giá 1A$ = ? AUD")]
        public int AUDToVND { get; set; }
    }
}
