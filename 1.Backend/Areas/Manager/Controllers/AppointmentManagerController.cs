using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System.Linq;
using PT.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using static PT.Domain.Model.Contact;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using PT.Base;

namespace PT.UI.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class AppointmentManagerController : Base.Controllers.BaseController
    {

        private readonly ILogger _logger;
        private readonly IOptions<EmailSettings> _emailSettings;

        private readonly IContactRepository _iContactRepository;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly IEmployeeRepository _iEmployeeRepository;
        private readonly IEmailSenderRepository _iEmailSenderRepository;

        public AppointmentManagerController(
            ILogger<AppointmentManagerController> logger,
            IOptions<EmailSettings> emailSettings,
            IContactRepository iContactRepository,
            IContentPageRepository iContentPageRepository,
            IEmployeeRepository iEmployeeRepository,
            IEmailSenderRepository iEmailSenderRepository
        )
        {
            controllerName = "ContactManager";
            tableName = "Contact";
            _logger = logger;
            _iContactRepository = iContactRepository;
            _iContentPageRepository = iContentPageRepository;
            _iEmployeeRepository = iEmployeeRepository;
            _emailSettings = emailSettings;
            _iEmailSenderRepository = iEmailSenderRepository;
        }

        #region [Index]
        [AuthorizePermission]
        public async Task<IActionResult> Index(string language = "vi")
        {
            ViewData["EmployeeSelectList"] = new SelectList(await _iEmployeeRepository.SearchAsync(true, 0, 0, x => x.Status && !x.Delete && x.Language == "vi"), "Id", "FullName");
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, bool? status,int? appointmentStatus,string fromDate, string toDate,int? employeeId,int? serviceId, string ordertype = "asc", string orderby = "name")
        {
            var _appointmentStatus = (ContactConfirmAppointment)(appointmentStatus??0);
            DateTime _fromDate = fromDate == null ? DateTime.Now.Date : Convert.ToDateTime(fromDate).Date;
            DateTime _toDate = toDate == null ? DateTime.Now.Date : Convert.ToDateTime(toDate).Date;

            limit = 5;
            var data = await _iContactRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                    m => (m.FullName.Contains(key) || m.Email.Contains(key) || m.Phone.Contains(key) || key == null) &&
                        (m.AppointmentStatus== ContactConfirmAppointment.Undefined) &&
                         m.Type == Contact.ContactType.BookAnAppointment &&
                        !m.Delete,
                x=>x.OrderByDescending(m=>m.CreatedDate));
            data.ReturnUrl = Url.Action("Index",
                new
                {
                    page,
                    limit,
                    key,
                    status,
                    ordertype,
                    orderby
                });
            return View("IndexAjax", data);
        }
        #endregion

        #region [Details]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Details(int id)
        {
            var dl = await _iContactRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null || (dl != null && dl.Delete))
            {
                return View("404");
            }
            if (!dl.Status)
            {
                dl.Status = true;
                _iContactRepository.Update(dl);
                await _iContactRepository.CommitAsync();
            }
            var model = MapModel<ContactAppointmentModel>.Go(dl);
            model.AppointmentStatus = dl.AppointmentStatus;
            model.Service = await _iContentPageRepository.SingleOrDefaultAsync(true, x => x.Id == dl.ServiceId);
            model.ServiceSelectList = new SelectList(await _iContentPageRepository.SearchAsync(true, 0, 0, x => !x.Delete && x.Status && x.Type==CategoryType.Service && x.Language == "vi"), "Id", "Name");
            model.EmployeeSelectList = new SelectList(await _iEmployeeRepository.SearchAsync(true, 0, 0, x => x.Status && !x.Delete && x.Language == "vi"), "Id", "FullName");
            return View(model);
        }
        [HttpPost, ActionName("Details")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DetailsPost(ContactAppointmentModel use,int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iContactRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }
                    dl.AppointmentStatus = use.AppointmentStatus;
                    dl.ServiceId = use.ServiceId??0;
                    dl.Note = use.Note;
                    dl.EmployeeId = use.EmployeeId;
                    dl.AppointmentDate = use.AppointmentDate;
                    _iContactRepository.Update(dl);
                    await _iContactRepository.CommitAsync();
                    if(use.SendEmail)
                    {
                       await _iEmailSenderRepository.SendAsync(_emailSettings.Value, use.Email, "Yêu cầu đặt lịch của bạn đã được xử lý", ((ContactConfirmAppointment)use.AppointmentStatus).GetDisplayName());
                    }
                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Xử lý đơn khám \"{ dl.AppointmentStatus .GetDisplayName()}\" khách hàng \"{ dl.FullName }\"",
                        Type = LogType.Edit
                    });
                    return new ResponseModel() { Output = 1, Message = "Xử lý đơn hẹn thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin.", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [Search Service]
        [HttpPost, Authorize]
        public async Task<object> SearchService(string q, int top = 20, string languegu = "vi", bool? isSearch = false)
        {
            top = top > 100 ? 20 : top;
            var listData = (await _iContentPageRepository.SearchAsync(true, 0, 0, x => x.Name.Contains(q) && x.Language == languegu && x.Status && !x.Delete && x.Type == CategoryType.Service)).Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
            var list = new List<SelectListItem>();
            if (isSearch == true)
            {
                list.Add(new SelectListItem { Text = "Tất cả", Value = "" });
            }
            else
            {
                list.Add(new SelectListItem { Text = "Chọn dịch vụ", Value = "0" });
            }
            list.AddRange(listData);
            return list;
        }
        #endregion

        #region [AddSlide]
        [HttpPost, AuthorizePermission("Index"), ActionName("AddSchedule")]
        public async Task<ResponseModel<object>> AddSchedulePost([FromBody]ScheduleItem data,int employeeId)
        {
            try
            {
                if (data == null)
                {
                    return new ResponseModel<object>() { Output = 0, Message = "Đặt lịch hẹn thành công", Type = ResponseTypeMessage.Success };
                }
                var kt = await _iContactRepository.SingleOrDefaultAsync(false, x => x.Id == data.ContactId);
                if (kt != null)
                {
                    kt.AppointmentDate = data.Start ?? DateTime.Now;
                    if(data.End!=null)
                    {
                        kt.AppointmentDateTo = data.End ?? DateTime.Now;
                    }
                    else
                    {
                        kt.AppointmentDateTo = kt.AppointmentDate.Value.AddHours(1);
                    }
                    
                    kt.AppointmentStatus = ContactConfirmAppointment.Confirm;
                    kt.EmployeeId = employeeId;
                    _iContactRepository.Update(kt);
                    await _iContactRepository.CommitAsync();

                    await AddLog(new LogModel
                    {
                        ObjectId = data.ContactId,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm lịch hẹn #{data.ContactId} với khách hàng: {kt.FullName}/{kt.Phone}/{kt.Email}",
                        Type = LogType.Edit
                    });

                    return new ResponseModel<object>() { Output = 1, Message = "Cập nhật lịch thành công", Type = ResponseTypeMessage.Success, Data = kt };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel<object>() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [GetSchedules]
        [AuthorizePermission("Index"), ActionName("GetSchedules")]
        public async Task<ResponseModel<object>> GetSchedules(int employeeId, DateTime thisdate, string type)
        {
            try
            {
                int thisMonth = thisdate.Month;
                int thisYear = thisdate.Year;
                var dtDauThang = thisdate.AddMonths(-1);
                var dtDauThangTiepTheo = thisdate.AddMonths(1);
                var list = new List<Contact>();
                var newList = new List<FullCalendarModel>();
                if (type == "month")
                {
                    list.AddRange(await _iContactRepository.SearchAsync(true, 0, 0,
                    x => x.EmployeeId == employeeId && (
                         (x.AppointmentDate.Value.Year == thisYear && x.AppointmentDate.Value.Month == thisMonth) ||
                         (x.AppointmentDate.Value.Year == dtDauThang.Year && x.AppointmentDate.Value.Month == dtDauThang.Month) ||
                         (x.AppointmentDate.Value.Year == dtDauThangTiepTheo.Year && x.AppointmentDate.Value.Month == dtDauThangTiepTheo.Month) && (x.AppointmentStatus==ContactConfirmAppointment.Confirm || x.AppointmentStatus == ContactConfirmAppointment.Done || x.AppointmentStatus == ContactConfirmAppointment.Cancel))
                    ));
                }
                else if (type == "listMonth")
                {

                    list.AddRange(await _iContactRepository.SearchAsync(true, 0, 0,
                          x => x.EmployeeId == employeeId && ((x.AppointmentDate.Value.Year == thisYear && x.AppointmentDate.Value.Month == thisMonth) && (x.AppointmentStatus == ContactConfirmAppointment.Confirm || x.AppointmentStatus == ContactConfirmAppointment.Done || x.AppointmentStatus == ContactConfirmAppointment.Cancel))
                    ));
                }
                else if (type == "agendaWeek")
                {
                    if (thisdate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        thisdate = thisdate.AddDays(-6);
                    }
                    else
                    {
                        int inxx = (int)thisdate.DayOfWeek;
                        thisdate = thisdate.AddDays(inxx - 1);
                    }
                    var nextWeek = thisdate.AddDays(6).Date;
                    list.AddRange(await _iContactRepository.SearchAsync(true, 0, 0,
                        x => x.EmployeeId == employeeId && x.AppointmentDate.Value.Date >= thisdate.Date && x.AppointmentDate.Value.Date <= nextWeek && (x.AppointmentStatus == ContactConfirmAppointment.Confirm || x.AppointmentStatus == ContactConfirmAppointment.Done || x.AppointmentStatus == ContactConfirmAppointment.Cancel)
                        ));
                }
                else if (type == "agendaDay")
                {

                    list.AddRange(await _iContactRepository.SearchAsync(true, 0, 0,
                         x => x.EmployeeId == employeeId && ((x.AppointmentDate.Value.Year == thisYear && x.AppointmentDate.Value.Month == thisMonth && x.AppointmentDate.Value.Day == thisdate.Day)) && (x.AppointmentStatus == ContactConfirmAppointment.Confirm || x.AppointmentStatus == ContactConfirmAppointment.Done || x.AppointmentStatus == ContactConfirmAppointment.Cancel)
                    ));
                }

                newList = list.Select(x => new FullCalendarModel
                {
                    Start = x.AppointmentDate.Value.ToString("yyyy-MM-dd HH:mm"),
                    End = x.AppointmentDateTo==null? x.AppointmentDate.Value.AddHours(1).ToString("yyyy-MM-dd HH:mm") : x.AppointmentDateTo.Value.ToString("yyyy-MM-dd HH:mm"),
                    ContactId = x.Id,
                    Title = FormatTitle(x),
                    Stick = true,
                    AllDay = false,
                    IsChange = false,
                    Overlap = true,
                    Rendering = "",
                    Color = "#3f50b5"
                }).ToList();
                return new ResponseModel<object>() { Output = 1, Type = ResponseTypeMessage.Success, Data = newList };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel<object>() { Output = -1, Data= "[]", Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        public string FormatTitle(Contact obj)
        {
            return $"{obj.FullName} / {obj.Phone}/ {obj.Email}";
        }

        #region [EditSchedule]
        [HttpGet, AuthorizePermission("Index")]
        public IActionResult EditSchedule(string data)
        {
            var model = JsonConvert.DeserializeObject<ScheduleItem>(data);
            model.FromTime = model.Start?.ToString("HH:mm");
            model.ToTime = model.End?.ToString("HH:mm");
            model.Data = data;
            return View(model);
        }
        [HttpPost, AuthorizePermission("Index"), ActionName("EditSchedule")]
        public async Task<ResponseModel<object>> EditSchedulePost(ScheduleItem use)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<ScheduleItem>(use.Data);
                model.Start = Convert.ToDateTime(model.Start.Value.ToString("yyyy/MM/dd") + " " + use.FromTime);
                model.End = Convert.ToDateTime(model.Start.Value.ToString("yyyy/MM/dd") + " " + use.ToTime);
                var kt = await _iContactRepository.SingleOrDefaultAsync(true, x => x.Id == model.ContactId);
                if (kt == null)
                {
                    return new ResponseModel<object>() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                kt.AppointmentDate = model.Start ?? DateTime.Now;
                kt.AppointmentDateTo = model.End ?? DateTime.Now;
                _iContactRepository.Update(kt);
                await _iContactRepository.CommitAsync();

                await AddLog(new LogModel
                {
                    ObjectId = use.ContactId,
                    ActionTime = DateTime.Now,
                    Name = $"Cập nhật lịch hẹn",
                    Type = LogType.Edit
                });

                return new ResponseModel<object>() { Output = 0, Message = "Cập nhật thành công", Type = ResponseTypeMessage.Success, Data = model, IsClosePopup = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel<object>() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion
    }
}