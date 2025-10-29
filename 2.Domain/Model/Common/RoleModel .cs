using System;
using System.Collections.Generic;
using System.Text;

namespace PT.Domain.Model
{
    public class RoleModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public  string ConcurrencyStamp { get; set; }
        public string Description { get; set; }
        public string NormalizedName { get; set; }
        public int NumberOfUsers { get; set; }
    }
}
