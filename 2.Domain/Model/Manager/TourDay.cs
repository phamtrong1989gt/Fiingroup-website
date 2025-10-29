using PT.Domain.Seedwork;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PT.Domain.Model
{
    public class TourDay : IAggregateRoot
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public int Day { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public bool IsCar { get; set; }
        public bool IsCycling { get; set; }
        public bool IsCruising { get; set; }
        public bool IsFlight { get; set; }
        public bool IsLocalBoat { get; set; }
        public bool IsLocalTouch { get; set; }
        public bool IsHotel { get; set; }
        public string Photos { get; set; }
    }

    public class TourDayModel
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        [Display(Name = "Tên")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string Name { get; set; }
        [Display(Name = "Ngày")]
        public int Day { get; set; }
        [Display(Name = "Ghi chú")]
        public string Details { get; set; }
        [Display(Name = "Đi xe ô tô")]
        public bool IsCar { get; set; }
        [Display(Name = "Đạp xe")]
        public bool IsCycling { get; set; }
        [Display(Name = "Du thuyền")]
        public bool IsCruising { get; set; }
        [Display(Name = "Máy bay")]
        public bool IsFlight { get; set; }
        [Display(Name = "Đi thuyền")]
        public bool IsLocalBoat { get; set; }
        [Display(Name = "Đi bộ")]
        public bool IsLocalTouch { get; set; }
        [Display(Name = "Nghỉ ở khách sạn")]
        public bool IsHotel { get; set; }
        public string Photos { get; set; }
        public List<FileDataModel> PhotoFileDatas { get; set; }
        public string AltId { get; set; }
    }
}
