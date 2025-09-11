using System;
using System.Collections.Generic;
using System.Text;

namespace PT.Domain.Model.Common
{
    public class DataItem
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class DataConfig
    {
        public static List<CategoryType> AllowCategoryTypes
        {
            get
            {
                return new List<CategoryType>
                {
                    CategoryType.CategoryBlog,
                    CategoryType.Tag,
                    CategoryType.Blog,
                    CategoryType.Page,
                    CategoryType.FAQ,
                    CategoryType.ImageGallery,
                    CategoryType.Static,
                    CategoryType.PromotionInformation,
                    CategoryType.CategoryTour,
                    CategoryType.Tour,
                    CategoryType.TourType
                };
            }    
        }
    }
}
