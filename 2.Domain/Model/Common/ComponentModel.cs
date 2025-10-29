using System;
using System.Collections.Generic;
using System.Text;

namespace PT.Domain.Model
{
    public class ComponentModel<T> where T : class
    {
        public string Language { get; set; }
        public int? CategoryId { get; set; }
        public int? TagId { get; set; }

        public int Take { get; set; }
        public string View { get; set; }
        public string Title { get; set; }
        public string Href { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}
