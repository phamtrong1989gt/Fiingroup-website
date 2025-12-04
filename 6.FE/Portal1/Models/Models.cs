using PT.Domain.Model;

namespace PT.UI.Models
{
    public class ProductComponentModel
    {
       public  int CurrentId { get; set; }
        public string Language { get; set; }
        public int Take { get; set; }
        public string View { get; set; }
        public string Title { get; set; }
    }
    public class ContentPageComponentModel
    {
        public string Language { get; set; }
        public int? ContentPageId { get; set; }
        public int? CategoryId { get; set; }
        public int? TagId { get; set; }
        public int Take { get; set; }
        public string View { get; set; }
        public string Title { get; set; }
        public string Href { get; set; }
        public CategoryType Type { get; set; }
    }

}
