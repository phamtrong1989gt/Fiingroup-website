using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PT.Domain.Model
{
    public class ContentPageReference : IAggregateRoot
    {
        [Key]
        public int Id { get; set; }
        public int ContentPageId { get; set; }
        public string Name { get; set; }
        public string Href { get; set; }
        public string Target { get; set; }
        public string Rel { get; set; }
    }

    public class ContentPageReferenceModel
    {
        public int Id { get; set; }
        public int ContentPageId { get; set; }
        public string Name { get; set; }
        public string Href { get; set; }
        public string Target { get; set; }
        [Display(Name = "Quan hệ (rel)")]
        public string Rel { get; set; }
        public int Stt { get; set; }
        public int Type { get; set; }
    }
}
