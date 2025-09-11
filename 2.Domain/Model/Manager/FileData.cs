using PT.Domain.Seedwork;
using System;
using System.ComponentModel.DataAnnotations;

namespace PT.Domain.Model
{
    public class FileData : IAggregateRoot
    {
        [Key]
        public int Id { get; set; }
        public string Path { get; set; }
        [MaxLength(100)]
        public string AltId { get; set; }
        public int ObjectId { get; set; }
        public CategoryType Type { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
