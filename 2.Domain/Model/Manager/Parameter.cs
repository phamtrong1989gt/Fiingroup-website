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
    public class Parameter : IAggregateRoot
    {
        public string Id { get; set; }
        public int PortalId { get; set; }
        public string Language { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string LinkName { get; set; }
    }
}
