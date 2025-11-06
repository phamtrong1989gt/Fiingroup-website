using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Infrastructure.Repositories;
using PT.Shared;
using PT.UI.Models;
using PT.UI.SignalR;

namespace PT.UI.Controllers
{
    public class ContactHomeController : Controller
    {
        
        private readonly IContactRepository _iContactRepository;
        private readonly ICustomerRepository _iCustomerRepository;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly IEmailSenderRepository _iEmailSenderRepository;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly ICountryRepository _iCountryRepository;
        private readonly IUserRepository _iUserRepository;
        private readonly IOptions<AuthorizeSettings> _authorizeSettings;

        public ContactHomeController(
            IContactRepository iContactRepository, 
            ICustomerRepository iCustomerRepository,
            IContentPageRepository iContentPageRepositor,
            IEmailSenderRepository iEmailSenderRepository,
            IOptions<BaseSettings> baseSettings ,
            IOptions<EmailSettings> emailSettings,
            ICategoryRepository iCategoryRepository,
            ICountryRepository iCountryRepository,
            IUserRepository iUserRepository,
            IOptions<AuthorizeSettings> authorizeSettings
        )
        {
            _iContactRepository = iContactRepository;
            _iCustomerRepository = iCustomerRepository;
            _iContentPageRepository = iContentPageRepositor;
            _iEmailSenderRepository = iEmailSenderRepository;
            _baseSettings = baseSettings;
            _emailSettings = emailSettings;
            _iCategoryRepository = iCategoryRepository;
            _iCountryRepository = iCountryRepository;
            _iUserRepository = iUserRepository;
            _authorizeSettings = authorizeSettings;
        }

        [HttpPost, ActionName("Contact")]
        [AutoValidateAntiforgeryToken]
        public async Task<ResponseModel> ContactPost(ContactHomeModel use)
        {
            try
            {
                //var output = await _iUserRepository.VeryfyCapcha(_authorizeSettings.Value.CapchaVerifyUrl, _authorizeSettings.Value.CapChaSecret, use.Capcha);
                //var capchaOke = output.Success;
                //if (!capchaOke)
                //{
                //    return new ResponseModel() { Output = 69, Message = "Phiên làm việc đã hết hạn hoặc thao tác thực hiện quá nhanh, vui lòng thử lại", Type = ResponseTypeMessage.Warning };
                //}

                //if (IsCheckRequest("Contact"))
                //{
                //    SetRequest("Contact");
                //}
                //else
                //{
                //    return new ResponseModel() { Output = -1, Message = "Bạn thao tác gửi liên hệ quá nhanh trong một khoảng thời gian, hãy đợi và thực hiện lại", Type = ResponseTypeMessage.Warning };
                //}

                if (ModelState.IsValid)
                {
                    await _iContactRepository.AddAsync(new Contact
                    {
                        FullName = Functions.SContent(use.FullName),
                        Content = Functions.SContent(use.Content),
                        Delete = false,
                        Status = false,
                        Email = use.Email,
                        Phone = use.Phone,
                        Position = use.Position,
                        ConpanyName = use.ConpanyName,
                        ServiceId = (int)use.ServiceId,
                        CreatedDate = DateTime.Now
                        
                    });
                    await _iContactRepository.CommitAsync();

                    //if(use.FullName.ToLower().Contains("sex") || use.FullName.ToLower().Contains("girl") || use.FullName.ToLower().Contains("human"))
                    //{
                    //    return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
                    //}

                    //if ((use.Content ?? "").ToLower().Contains("sex") || (use.Content ?? "").ToLower().Contains("girl") || (use.Content ?? "").ToLower().Contains("human"))
                    //{
                    //    return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
                    //}

                    //var dlCountry = await _iCountryRepository.SingleOrDefaultAsync(true, x => x.Id == use.CountryId);

                    if(!string.IsNullOrEmpty(_baseSettings.Value.ToEmail))
                    {
                        var strB = new StringBuilder();
                        strB.Append($"Họ và tên: {Functions.SContent(use.FullName)}<br>");
                        strB.Append($"Email: {use.Email}<br>");
                        strB.Append($"Phone: {use.Phone}<br>");
                        strB.Append($"Tên công ty: {use.ConpanyName}<br>");
                        strB.Append($"Chức vụ công việc: {use.Position}<br>");
                        strB.Append($"Dịch vụ quan tâm: {use.ServiceId}<br>");
                        strB.Append($"Nội dung: {Functions.SContent(use.Content)}<br>");

                        //await Task.Run(() => SendEmail(_emailSettings.Value, _baseSettings.Value.ToEmail,  $"Có đăng ký mới từ {use.FullName} địa chỉ email là {use.Email}", strB.ToString())).ConfigureAwait(false);
                    }
                    return new ResponseModel() { Output = 1, Message = "Đăng ký thành công", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
            }
            catch
            {
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }

        private async void SendEmail(EmailSettings emailSettings,string toEmail,string title, string content)
        {
           await _iEmailSenderRepository.SendEmailAsync(emailSettings, null, title, content, toEmail);
        }

        private void SetRequest(string type)
        {
            HttpContext.Session.SetString(type, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
        }
        private bool IsCheckRequest(string type)
        {
            var getData = HttpContext.Session.GetString(type);
            if(getData==null)
            {
                return true;
            }
            var dt = Convert.ToDateTime(getData);
            if(dt.AddSeconds(_baseSettings.Value.TimeOutSendRequest) >=DateTime.Now)
            {
                return false;
            }
            return true;
        }
    }
}

