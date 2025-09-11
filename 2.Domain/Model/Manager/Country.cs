using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PT.Domain.Model
{
    public class Country : IAggregateRoot
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneCode { get; set; }
    }
}
