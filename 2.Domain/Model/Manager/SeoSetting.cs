using PT.Domain.Seedwork;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PT.Domain.Model
{
    public class SeoSetting : IAggregateRoot
    {
        [Key]
        public int Id { get; set; }
        public string Language { get; set; }
        public int PortalId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
        public string MetaGoogle { get; set; }
        public string Robots { get; set; }
        [NotMapped]
        public List<Portal> Portals { get; set; }
    }
}
