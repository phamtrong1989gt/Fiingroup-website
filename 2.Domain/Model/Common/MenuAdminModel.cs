using System;
using System.Collections.Generic;
using System.Text;

namespace PT.Domain.Model.Common
{
    public class MenuAdminModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IdParent { get; set; }
        public string Class { get; set; }
        public string Attribute { get; set; }
        public int Order { get; set; }
    }
}
