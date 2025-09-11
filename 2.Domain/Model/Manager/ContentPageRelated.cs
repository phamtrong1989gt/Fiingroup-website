using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Domain.Model
{
    public class ContentPageRelated : IAggregateRoot
    {
        [Key]
        public int Id { get; set; }
        public int ParentId { get; set; }
        public int ContentPageId { get; set; }
    }
}
