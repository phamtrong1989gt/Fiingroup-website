using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PT.Domain.Model
{
    public class LinkHomeModel
    {
        public string Controller { get; set; }
        public string Acction { get; set; }
        public Link Link { get; set; }
    }

    public class CategoryTreeModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ParentId { get; set; }
        public CategoryType Type { get; set; }
        public string SlugLink { get; set; }
        public string Language { get; set; }
        public string Path { get; set; }
    }

    public class ContentPageCategoryTreeModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ParentId { get; set; }
        public CategoryType Type { get; set; }
        public string SlugLink { get; set; }
        public string Language { get; set; }
        public string Path { get; set; }
    }
}
