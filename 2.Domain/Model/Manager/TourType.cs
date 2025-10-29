using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PT.Domain.Seedwork;

namespace PT.Domain.Model
{
    public class TourType : IAggregateRoot
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Banner { get; set; }
        public string BannerHeader { get; set; }
        public string BannerFooter { get; set; }
        public string Content { get; set; }
        public bool Status { get; set; }
        [MaxLength(10)]
        public string Language { get; set; }
        public bool Delete { get; set; }
        [NotMapped]
        public virtual Link Link { get; set; }
        [NotMapped]
        public List<LinkReference> LinkReferences { get; set; }
    }

    public class TourTypeModel : SeoModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Banner { get; set; }
        public string BannerHeader { get; set; }
        public string BannerFooter { get; set; }
        public string Content { get; set; }
        [NotMapped]
        public List<FileDataModel> PhotoFileDatas { get; set; } = new List<FileDataModel>();
    }
}
