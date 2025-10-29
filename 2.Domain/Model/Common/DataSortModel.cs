using System;
using System.Collections.Generic;
using System.Text;

namespace PT.Domain.Model
{
    public class DataSortModel
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public int Order { get; set; }
        public List<DataSortModel> Children { get; set; } = new List<DataSortModel>();
    }
}
