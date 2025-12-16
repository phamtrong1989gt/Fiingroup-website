using System;
using System.Collections.Generic;

namespace PT.Domain.Model
{
    public class NewsDTOTagDTO
    {
        public string Name { get; set; }
    }
    public class NewsDTO
    {
        public int NewsId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ShortContent { get; set; }
        public DateTime? PublicDate { get; set; }
        public string FriendlyTitle { get; set; }
        public string ImageUrl { get; set; }
        public string SourceUrl { get; set; }
        public string Author { get; set; }
        public string UpdateBy { get; set; }
        public int RecordStatusId { get; set; }
        public List<RefItem> Categories { get; set; }
        public List<int> TypeIds { get; set; }
        public List<int> SourceIds { get; set; }
        public List<RefItem> Entities { get; set; }
        public List<NewsDTOTagDTO> Tags { get; set; }
        public List<RefItem> ICBs { get; set; }
        public List<RefItem> VSICs { get; set; }
    }
    public class NewsCMD
    {
        public int NewsId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ShortContent { get; set; }
        public DateTime? PublicDate { get; set; }
        public string FriendlyTitle { get; set; }
        public string ImageUrl { get; set; }
        public string SourceUrl { get; set; }
        public string Author { get; set; }
        public string UpdateBy { get; set; }
        //
        public int StatusId { get; set; }
        public List<RefItem> Categories { get; set; }
        public List<int> TypeIds { get; set; }
        public List<int> SourceIds { get; set; }
        public List<RefItem> Entities { get; set; }
        public List<string> Tags { get; set; }
        public List<RefItem> ICBs { get; set; }
        public List<RefItem> VSICs { get; set; }
        public string CreateBy { get; set; }
    }

    /// <summary>
    /// DTOs matching the external news JSON payload.
    /// </summary>
    public class NewsDto
    {
        public int NewsId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ShortContent { get; set; }
        public DateTime? PublicDate { get; set; }
        public string FriendlyTitle { get; set; }
        public string ImageUrl { get; set; }
        public string SourceUrl { get; set; }
        public string Author { get; set; }
        public string UpdateBy { get; set; }
        public int RecordStatusId { get; set; }
        public List<RefItem> Categories { get; set; }
        public List<int> TypeIds { get; set; }
        public List<int> SourceIds { get; set; }
        public List<RefItem> Entities { get; set; }
        public List<string> Tags { get; set; }
        public List<RefItem> ICBs { get; set; }
        public List<RefItem> VSICs { get; set; }
    }
}
