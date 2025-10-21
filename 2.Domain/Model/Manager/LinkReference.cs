using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PT.Domain.Model
{
    public class LinkReference : IAggregateRoot
    {
        public int Id { get; set; }
        public int LinkId1 { get; set; }
        public int LinkId2 { get; set; }
        public string Language { get; set; }
        [NotMapped]
        public Link Link2 { get; set; }
    }
    public class LinkReferenceBaseModel
    {
        public int LinkId { get; set; }
        public int? ReferenceLinkIdVI { get; set; }
        public int? ReferenceLinkIdEN { get; set; }
        public int? ReferenceLinkIdIT { get; set; }

        [Display(Name = "Loại danh mục")]
        [Required]
        public CategoryType Type { get; set; }

        public string Language { get; set; }

        [Display(Name ="Tự động ánh xạ với trang này")]
        public bool IsAutoVI { get; set; } = true;

        [Display(Name = "Tự động ánh xạ với trang này")]
        public bool IsAutoEn { get; set; } = true;

        [Display(Name = "Tự động ánh xạ với trang này")]
        public bool IsAutoIt { get; set; } = true;

        public int PortalId { get; set; }
    }
}
