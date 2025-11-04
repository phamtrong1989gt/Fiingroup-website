using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.BE.Areas.Base.Controllers;
using PT.Shared;
using PT.Base;

namespace PT.BE.Areas.Setting.Controllers
{
    [Area("Base")]
    [AuthorizePermission("Index")]
    public class SettingsController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IFileRepository _iFileRepository;
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly IEmailSenderRepository _iEmailSenderRepository;
        private readonly IPortalRepository _iPortalRepository;
        private readonly IBindContentSettingRepository _iBindContentSettingRepository;
        public SettingsController(
            ILogger<BaseController> logger, 
            IFileRepository iFileRepository, 
            IWebHostEnvironment iHostingEnvironment, 
            IOptions<EmailSettings> emailSettings,
            IEmailSenderRepository iEmailSenderRepository,
            IPortalRepository iPortalRepository,
            IBindContentSettingRepository iBindContentSettingRepository
            )
        {
            controllerName = "Settings";
            tableName = "Base";

            _logger = logger;
            _iFileRepository = iFileRepository;
            _iHostingEnvironment = iHostingEnvironment;
            _emailSettings = emailSettings;
            _iEmailSenderRepository = iEmailSenderRepository;
            _iPortalRepository = iPortalRepository;
            _iBindContentSettingRepository= iBindContentSettingRepository;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        #region [Email]

        [HttpGet]
        public IActionResult Email()
        {
            return View(_emailSettings.Value);
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Email")]
        public async Task<ResponseModel> EmailPost(EmailSettings model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string valueBeffo = Newtonsoft.Json.JsonConvert.SerializeObject(model);

                    var data = _emailSettings.Value;
                    data.Email = model.Email;
                    data.From = model.From;
                    data.Host = model.Host;
                    data.Password = model.Password;
                    data.Port = model.Port;

                    _iFileRepository.SettingsUpdate(_iHostingEnvironment.ContentRootPath + "/appsettings.Email.json", new { EmailSettings = _emailSettings.Value });

                    await AddLog(new LogModel { Name = $"Cập nhật cấu hình email.", Type = LogType.Edit});
                    return new ResponseModel() { Output = 1, Message = "Cập nhật cấu hình thành công.", Type = ResponseTypeMessage.Success };
                }
                else
                {
                    return new ResponseModel() { Output = 2, Message = "Bạn chưa nhập đầy đủ thông tin.", Type = ResponseTypeMessage.Warning };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        [HttpPost, ActionName("SendEmail")]
        public async Task<ResponseModel> SendEmailPost(string email)
        {
            try
            {
                var output = await _iEmailSenderRepository.SendAsync(_emailSettings.Value, "phamtrong1989@gmail.com", "Email test success.", "Email test success.");
                if(output)
                {
                    return new ResponseModel() { Output = 1, Message = "Gửi email thành công.", Type = ResponseTypeMessage.Success };
                }
                else
                {
                    return new ResponseModel() { Output = 1, Message = "Gửi email thất bại.", Type = ResponseTypeMessage.Warning };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [BindContent]
        [HttpGet]
        public async Task<IActionResult> BindContent(int portalId)
        {
            var dl = await _iBindContentSettingRepository.SingleOrDefaultAsync(false, x => x.PortalId == portalId);
            if(dl == null)
            {
                dl = new BindContentSetting() { PortalId = portalId };
                await _iBindContentSettingRepository.AddAsync(dl);
                await _iBindContentSettingRepository.CommitAsync();
            }    
            return View(dl);
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("BindContent")]
        public async Task<ResponseModel> BindContentPost(BindContentSetting model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iBindContentSettingRepository.SingleOrDefaultAsync(false, x => x.PortalId == model.PortalId);
                    if(dl == null)
                    {
                        return new ResponseModel() { Output = 2, Message = "Cấu hình không tồn tại.", Type = ResponseTypeMessage.Warning };
                    }    
                    dl.Head = model.Head;
                    dl.Body = model.Body;
                    dl.Footer = model.Footer;
                    await AddLog(new LogModel { Name = $"Cập nhật cấu thông tin nội dung thêm ", Type = LogType.Edit });
                    // Call API refesh
                    return new ResponseModel() { Output = 1, Message = "Cập nhật hình cấu hình thành công.", Type = ResponseTypeMessage.Success };
                }
                else
                {
                    return new ResponseModel() { Output = 2, Message = "Bạn chưa nhập đầy đủ thông tin.", Type = ResponseTypeMessage.Warning };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion
    }
}