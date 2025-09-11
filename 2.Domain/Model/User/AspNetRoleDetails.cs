using PT.Domain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using PT.Domain.Seedwork;

namespace PT.Domain.Model
{
    public class RoleDetail :  IAggregateRoot
    {
        [Key, ForeignKey("Role"), Column(Order = 1)]
        public int RoleId { get; set; }
        [Key, ForeignKey("RoleAction"), Column(Order = 2)]
        public int ActionId { get; set; }
        public virtual ApplicationRole Role { get; set; }
        public virtual RoleAction RoleAction { get; set; }

    }
}
