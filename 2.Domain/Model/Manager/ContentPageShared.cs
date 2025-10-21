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

    public class ContentPageShared : IAggregateRoot
    {
        public int ParentContentPageId { get; set; }
        public int ParentPortalId { get; set; }
        public int SharedContentPageId { get; set; }
        public int SharedPortalId { get; set; }
    }
}
