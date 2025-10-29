using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Seedwork;
using static PT.Domain.Model.Contact;

namespace PT.Domain.Model
{
    public class Contact : IAggregateRoot
    {
        public enum ContactType
        {
            [Display(Name = "Liên hệ")]
            Contact,
            [Display(Name = "Đặt lịch hẹn")]
            BookAnAppointment,
            [Display(Name = "Đánh giá")]
            Testimonial,
            [Display(Name = "Đánh giá người nổi tiếng")]
            FamousPeople,
            [Display(Name = "Đánh giá video")]
            TestimonialVideo,
            [Display(Name = "Người nổi tiếng đánh giá người nổi tiếng")]
            FamousPeopleVideo,
        }

        public enum ContactConfirmAppointment
        {
            [Display(Name = "Đơn mới")]
            Undefined,
            [Display(Name = "Xác nhận đơn")]
            Confirm,
            [Display(Name = "Hoàn thành")]
            Done,
            [Display(Name = "Hủy đơn")]
            Cancel
        }
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public int ServiceId { get; set; }
        public int Age { get; set; }
        public int CountryId { get; set; }
        public string FullName { get; set; } = "";
        public ContactType Type { get; set; }
        public string PhoneCode { get; set; }
        public string Avatar { get; set; }
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public string Email { get; set; } = "";
        public string Content { get; set; } = "";
        public string Note { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public DateTime? AppointmentDateTo { get; set; }
        public ContactConfirmAppointment AppointmentStatus { get; set; }
        public bool Status { get; set; }
        [MaxLength(10)]
        public string Language { get; set; }
        public bool Delete { get; set; }
        [NotMapped]
        public Customer Customer { get; set; }
        [NotMapped]
        public ContentPage Service { get; set; }
        [NotMapped]
        public Employee Employee { get; set; }
        public double Rating { get; set; }
        public bool IsHome { get; set; }
    }

    public class ContactModel
    {
        public int Id { get; set; }
        public ContactType Type { get; set; } = ContactType.Contact;
        [Display(Name ="Họ và tên")]
        public string FullName { get; set; }
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Email")]
        public string Address { get; set; }
        [Display(Name = "Nội dung")]
        public string Content { get; set; }
        [Display(Name = "Trạng thái")]
        public bool Status { get; set; }
        [MaxLength(10)]
        public string Language { get; set; }
        public bool Delete { get; set; }
        public DateTime CreatedDate { get; set; }

    }
    public class ContactAppointmentModel
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        [Display(Name = "Dịch vụ")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public int? ServiceId { get; set; }

        [Display(Name = "Tuổi")]
        public int Age { get; set; }

        public ContactType Type { get; set; }
        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string FullName { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string Phone { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Nội dùng")]
        public string Content { get; set; }

        [Display(Name = "Ngày gửi")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Ngày hẹn")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public DateTime? AppointmentDate { get; set; }

        [Display(Name = "Giờ hẹn")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string AppointmentTime { get; set; }

        [Display(Name = "Trạng thái xử lý")]

        public ContactConfirmAppointment AppointmentStatus { get; set; }

        public int AppointmentScheduleId { get; set; }
        public bool Status { get; set; }
        public string Language { get; set; }
        public bool Delete { get; set; }
        [Display(Name = "Bác sỹ")]
        public int EmployeeId { get; set; } = 0;
        public SelectList ServiceSelectList { get; set; }
        public SelectList EmployeeSelectList { get; set; }
        public virtual ContentPage Service { get; set; }
        [Display(Name = "Gửi email khi thay đổi trạng thái yếu cầu")]
        public bool SendEmail { get; set; }
        [Display(Name = "Ghi chú")]
        public string Note { get; set; }
        public string Address { get; set; }
    }
    public class ScheduleItem
    {
        public string UniqueId { get; set; }
        public int ContactId { get; set; }
        public string Title { get; set; }
        [Display(Name = "Ngày hẹn")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public bool IsChange { get; set; }
        [Display(Name = "Thời gian hẹn từ")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public string FromTime { get; set; }
        [Display(Name = "Thời gian hẹn đến")]
        [Required(ErrorMessage = "{0} không được để trống!")]
        public string ToTime { get; set; }
        public string Data { get; set; }
    }
    public class FullCalendarModel
    {
        public string Id { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public int ContactId { get; set; } = 0;
        public string Title { get; set; }
        public bool Stick { get; set; }
        public bool AllDay { get; set; }
        public bool IsChange { get; set; }
        public bool Overlap { get; set; }
        public string Rendering { get; set; }
        public string Color { get; set; }
        public string Constraint { get; internal set; }
    }

    public class TestimonialModel
    {
        public int Id { get; set; }
        [Display(Name = "Loại đánh giá")]
        public ContactType Type { get; set; } = ContactType.Testimonial;

        [Display(Name = "Tên khách hàng")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string FullName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [Display(Name = "Rating")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [Range(1, 5, ErrorMessage = "{0} từ 1 đến 5")]
        public double? Rating { get; set; } = 5;

        public string Language { get; set; }

        [Display(Name = "Check chọn thì nội dung này sẽ ưu tiên hiển thị ngoài trang index")]
        public bool IsHome { get; set; }

        public string Avatar { get; set; }
        public string Address { get; set; }
    }
}
