using System;
using System.Collections.Generic;
using System.Text;

namespace PT.Domain.Model
{
    public class TreeRoleModel
    {
        public string Id { get; set; }
        public string Parent { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }
        public TreeRoleStateModel State { get; set; }
    }
    public class TreeRoleStateModel
    {
        public bool Opened { get; set; }
        public bool Disabled { get; set; }
        public bool Selected { get; set; }

    }
}
