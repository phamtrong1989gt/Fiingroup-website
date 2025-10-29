using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PT.Domain.Model
{
    public class RedirectLink : IAggregateRoot
    {
        public enum RedirectLinkCode
        {
            Code301 = 301,
            Code302 = 302,
            Code307 = 307
        }
        [Key]
        public int Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public RedirectLinkCode Code { get; set; }
    }
}
