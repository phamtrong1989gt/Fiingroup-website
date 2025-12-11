using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;
using PT.UI.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<ContactHomeController> _logger;

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
            IOptions<AuthorizeSettings> authorizeSettings,
            ILogger<ContactHomeController> logger
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
            _logger = logger;
        }

        [HttpPost, ActionName("Contact")]
        [AutoValidateAntiforgeryToken]
        public async Task<ResponseModel> ContactPost(ContactHomeModel use, string language)
        {
            try
            {
                _logger.LogError("ContactPost  Settings {0}", Newtonsoft.Json.JsonConvert.SerializeObject(_authorizeSettings.Value));
                _logger.LogError("ContactPost  Data {0}", Newtonsoft.Json.JsonConvert.SerializeObject(use));

                if (string.IsNullOrEmpty(use.Capcha))
                {
                    return new ResponseModel() { Output = 69, Message = "Tiến hành xác thực", Type = ResponseTypeMessage.Warning };
                }

                var output = await _iUserRepository.VeryfyCapcha(_authorizeSettings.Value.CapchaVerifyUrl, _authorizeSettings.Value.CapChaSecret, use.Capcha);

                _logger.LogError("ContactPost  OutData {0}", Newtonsoft.Json.JsonConvert.SerializeObject(output));

                if (output != null && !output.Success)
                {
                    return new ResponseModel() { Output = 69, Message = "Phiên làm việc đã hết hạn hoặc thao tác thực hiện quá nhanh, vui lòng thử lại", Type = ResponseTypeMessage.Warning };
                }

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
                        Type = Contact.ContactType.Product,
                        Products = use.Products,
                        CreatedDate = DateTime.Now,
                        PortalId = _baseSettings.Value.PortalId,
                        Language = language
                    });
                    await _iContactRepository.CommitAsync();
                
                    if(!string.IsNullOrEmpty(_baseSettings.Value.ToEmail))
                    {
                        //var strB = new StringBuilder();
                        //strB.Append($"Họ và tên: {Functions.SContent(use.FullName)}<br>");
                        //strB.Append($"Email: {use.Email}<br>");
                        //strB.Append($"Phone: {use.Phone}<br>");
                        //strB.Append($"Tên công ty: {use.ConpanyName}<br>");
                        //strB.Append($"Chức danh công việc: {use.Position}<br>");
                        //strB.Append($"Nhóm ngành : {use.ServiceId}<br>");
                        //strB.Append($"Sản phẩm & dịch vụ quan tâm: {use.Products}<br>");
                        //strB.Append($"Nội dung: {Functions.SContent(use.Content)}<br>");
                        //await Task.Run(() => SendEmail(_emailSettings.Value, _baseSettings.Value.ToEmail,  $"Có đăng ký mới từ {use.FullName} địa chỉ email là {use.Email}", strB.ToString())).ConfigureAwait(false);
                    }
                    return new ResponseModel() { Output = 1, Message = "Đăng ký thành công", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, "Error in ContactPost: {Message}", ex.Message);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }

        [HttpPost, ActionName("FlowSelectList")]
        public async Task<ResponseModel<List<ContentPage>>> FlowSelectList(string language, int portId, int parrentId)
        {
            try
            {
                var lstSelectList = _iContactRepository.FlowSelectList(language, portId, parrentId);

                return new ResponseModel<List<ContentPage>>
                {
                    Output = 1,
                    Message = "Thành công.",
                    Type = ResponseTypeMessage.Success,
                    Data = lstSelectList.Result,
                    IsClosePopup = false
                };
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, "Error in FlowSelectList: {Message} | language={language} | portId={portId} | parrentId={parrentId}", ex.Message, language, portId, parrentId);
                return new ResponseModel<List<ContentPage>>
                {
                    Output = -1,
                    Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại",
                    Type = ResponseTypeMessage.Danger,
                    Status = false
                };
            }
        }

        [HttpPost, ActionName("ContactSolution")]
        [AutoValidateAntiforgeryToken]
        public async Task<ResponseModel> ContactSolutionPost(ContactSolotionModel use, string language)
        {
            try
            {
                if (string.IsNullOrEmpty(use.Capcha))
                {
                    return new ResponseModel() { Output = 69, Message = "Tiến hành xác thực", Type = ResponseTypeMessage.Warning };
                }
                var output = await _iUserRepository.VeryfyCapcha(_authorizeSettings.Value.CapchaVerifyUrl, _authorizeSettings.Value.CapChaSecret, use.Capcha);
                var capchaOke = output.Success;
                if (!capchaOke)
                {
                    return new ResponseModel() { Output = 69, Message = "Phiên làm việc đã hết hạn hoặc thao tác thực hiện quá nhanh, vui lòng thử lại", Type = ResponseTypeMessage.Warning };
                }

                if (ModelState.IsValid)
                {
                    await _iContactRepository.AddAsync(new Contact
                    {
                        FullName = Functions.SContent(use.FullName),
                        Delete = false,
                        Status = false,
                        Email = use.Email,
                        Phone = use.Phone,
                        Type = Contact.ContactType.Contact,
                        Products = use.Products,
                        CreatedDate = DateTime.Now,
                        PortalId = _baseSettings.Value.PortalId,
                        Language = language
                    });
                    await _iContactRepository.CommitAsync();

                    if (!string.IsNullOrEmpty(_baseSettings.Value.ToEmail))
                    {
                        //var strB = new StringBuilder();
                        //strB.Append($"Họ và tên: {Functions.SContent(use.FullName)}<br>");
                        //strB.Append($"Email: {use.Email}<br>");
                        //strB.Append($"Phone: {use.Phone}<br>");
                        //strB.Append($"Tên công ty: {use.ConpanyName}<br>");
                        //strB.Append($"Chức vụ công việc: {use.Position}<br>");
                        //strB.Append($"Dịch vụ quan tâm: {use.ServiceId}<br>");
                        //strB.Append($"Nội dung: {Functions.SContent(use.Content)}<br>");

                        //await Task.Run(() => SendEmail(_emailSettings.Value, _baseSettings.Value.ToEmail,  $"Có đăng ký mới từ {use.FullName} địa chỉ email là {use.Email}", strB.ToString())).ConfigureAwait(false);
                    }
                    return new ResponseModel() { Output = 1, Message = "Đăng ký nhận tin thành công", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, "Error in ContactSolutionPost: {Message}", ex.Message);
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

