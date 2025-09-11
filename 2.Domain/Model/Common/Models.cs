using System;
using System.Collections.Generic;
using System.Text;

namespace PT.Domain.Model
{
    public enum ModuleType
    {
        StaticInformation,
        Menu,
        AdvertisingBanner,
        PhotoSlide
    }

    public class PluginFileUploadModel
    {
        public string ParentTagName { get; set; }
        public string Extention { get; set; }
        public double MaxSize { get; set; }
        public string UrlUpload { get; set; }
        public string UrlDelete { get; set; }
        public string UrlFile { get; set; }
        public string InputName { get; set; }
        public List<FileDataModel> FileDatas { get; set; } = new List<FileDataModel>();
        public string FileDataString { get; set; }
        public bool? IsS { get; set; }
        public bool IsSingle { get; set; }
    }
    public class FileDataModel
    {
        public string Path { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedUser { get; set; }
    }
    public class FileDataCKEditerModel
    {
        public int Uploaded { get; set; }
        public string FileName { get; set; }
        public int Number { get; set; }
        public string Url { get; set; }
    }
}
