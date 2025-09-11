using PT.Domain.Seedwork;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PT.Domain.Model
{
    public class Product : IAggregateRoot
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }

        [MaxLength(1000)]
        public string Name { get; set; }

        public string Content { get; set; }

        public string Specification { get; set; }

        [MaxLength(1000)]
        public string Banner { get; set; }

        [MaxLength(10)]
        public string Language { get; set; }

        public CategoryType Type { get; set; } = CategoryType.CategoryProduct;

        public bool Status { get; set; }

        public bool Delete { get; set; }

        public double? Price { get; set; }

        [NotMapped]
        public Link Link { get; set; }

        [NotMapped]
        public List<Category> Categorys { get; set; }

        [NotMapped]
        public Category Category { get; set; }

        [NotMapped]
        public List<Category> ServiceCategorys { get; set; }

        public bool IsHome { get; set; }

        [NotMapped]
        public List<LinkReference> LinkReferences { get; set; }

        public string Images { get; set; }
      

    }
}
