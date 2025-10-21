using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PT.Domain.Model
{
    // Sẽ chứa link tất cả các phiên bản của 1 nội dung ví dụ 1 row chính nó, 1 row của những phiên bản còn lại
    public class LinkDataModel
    {
        public string Link { get; set; }
        public string Language { get; set; }

    }
    public class SeoModel: LinkReferenceBaseModel
    {
       
        [Display(Name = "Seo Title")]
        [StringLength(500, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Title { get; set; }
        [Display(Name = "Meta Description")]
        [StringLength(500, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Description { get; set; }
        [Display(Name = "Forcus Keywords")]
        [StringLength(500, ErrorMessage = "{0} từ {2} đến {1} ký tự!", MinimumLength = 0)]
        public string Keywords { get; set; }
        [Display(Name = "Liên kết thân thiện/Permalink")]
        [Required(ErrorMessage = "{0} không được để rỗng")]
        [RegularExpression(@"^.*[a-z0-9-/]$", ErrorMessage = "{0} chỉ bao gồm ký tự (a-z), (0-9), (-,/)")]
        [Remote("IsSlug", "Functions", "Base", ErrorMessage = "{0} đã tồn tại, vui lòng thay đổi hoặc thêm một số ký tự khác bao gồm (a-z), (0-9), (-,/)", AdditionalFields = "Id,Language,PortalId")]
        public string Slug { get; set; }
        public double? Price { get; set; }
        public bool ChangeSlug { get; set; }
        [Display(Name = "Sitemap Lastmod")]
        public DateTime? Lastmod { get; set; } = DateTime.Now;
        [Display(Name = "Sitemap Changefreq")]
        public string Changefreq { get; set; } = "monthly";
        [Display(Name = "Sitemap Priority")]
        public string Priority { get; set; } = "0.8";
        [Display(Name = "Sử dụng dữ liệu này")]
        public bool Status { get; set; } = true;
        public bool Delete { get; set; } = false;
        [Display(Name = "Forcus Keywords")]
        public string FocusKeywords { get; set; }
        [Display(Name = "Meta Robots Index")]
        public string MetaRobotsIndex { get; set; }
        [Display(Name = "Meta Robots Follow")]
        public string MetaRobotsFollow { get; set; } = "follow";
        [Display(Name = "Meta Robots Advance")]
        public string MetaRobotsAdvance { get; set; }
        [Display(Name = "Hiển thị trong sitemap")]
        public bool IncludeSitemap { get; set; } = true;
        [Display(Name = "Redirect 301")]
        public string Redirect301 { get; set; }
        [Display(Name = "Facebook Description")]
        public string FacebookDescription { get; set; }
        [Display(Name = "Facebook Banner")]
        public string FacebookBanner { get; set; }
        [Display(Name = "Google+ Description")]
        public string GooglePlusDescription { get; set; }

        public bool IsStatic { get; set; }

        [Display(Name = "Cổng")]
        [Required(ErrorMessage = "{0} không được để rỗng")]
        public int? PortalId { get; set; }
        public string PortalName { get; set; }

    }
}
