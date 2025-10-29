using System;
using System.Collections.Generic;
using System.Text;

namespace PT.Domain.Model.Common
{
    public class RoleActionModel
    {
        public int Id { get; set; }
        public int ControllerId { get; set; }
        public string Name { get; set; }
        public string ControllerName { get; set; }
        public string AreaName { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public bool IsShow { get; set; }
    }
}
