using System;
using System.Collections.Generic;
using System.Text;

namespace PT.Domain.Model
{
    public  class HomeModel
    {
        public SeoSettings SeoSettings { get; set; }
        public WebsiteInfoSettings WebsiteInfoSettings { get; set; }
        public string Domain { get; set; }
    }
}
