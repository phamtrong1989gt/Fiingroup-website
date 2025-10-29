using PT.Domain.Seedwork;
using System.ComponentModel.DataAnnotations.Schema;

namespace PT.Domain.Model
{
    public class TourCategory : IAggregateRoot
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public int CategoryId { get; set; }
        [NotMapped]
        public Category Category { get; set; }
        [NotMapped]
        public Link Link { get; set; }
    }
}
