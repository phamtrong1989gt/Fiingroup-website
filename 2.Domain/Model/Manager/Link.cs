using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class Link : IAggregateRoot
    {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public int ObjectId { get; set; }
        [MaxLength(10)]
        public string Language { get; set; }
        public DateTime Lastmod { get; set; } = DateTime.Now;
        [MaxLength(10)]
        public string Changefreq { get; set; } = "monthly";
        public double Priority { get; set; } = 0.5;
        public bool IsStatic { get; set; } = false;
        public CategoryType Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
        public bool Status { get; set; } = true;
        public bool Delete { get; set; } = false;
        public string FocusKeywords { get; set; }

        public string MetaRobotsIndex { get; set; }
        public string MetaRobotsFollow { get; set; }
        public string MetaRobotsAdvance { get; set; }
        public bool IncludeSitemap { get; set; }
        public string Redirect301 { get; set; }

        public string FacebookDescription { get; set; }
        public string FacebookBanner { get; set; }
        public string GooglePlusDescription { get; set; }

        [StringLength(20)]
        public string Area { get; set; }

        [StringLength(50)]
        public string Controller { get; set; }

        [StringLength(20)]
        public string Acction { get; set; }

        public string Parrams { get; set; }

        public int PortalId { get; set; }

        [NotMapped]
        public List<LinkReference> LinkReferences { get; set; }
    }
}
