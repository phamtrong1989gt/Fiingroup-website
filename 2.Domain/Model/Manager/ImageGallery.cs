using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PT.Domain.Model
{
    public class ImageGallery : IAggregateRoot
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        public bool Delete { get; set; }
        public string Language { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
        [NotMapped]
        public  Link Link { get; set; }
        [NotMapped]
        public BaseSearchModel<List<Image>> ListImage { get; set; }
        [NotMapped]
        public List<Category> Categorys { get; set; }
        [NotMapped]
        public List<LinkReference> LinkReferences { get; set; }
    }
    public class ImageGalleryModel:SeoModel
    {
        public int Id { get; set; }
        [Display(Name = "Tên")]
        [Required(ErrorMessage = "{0} không được để rỗng!")]
        public string Name { get; set; }
        public virtual Link Link { get; set; }
        public CategoryType SlugType { get; set; } = CategoryType.Employee;
        public List<SelectListItem> CategorySelectList { get; set; }
    }
}
