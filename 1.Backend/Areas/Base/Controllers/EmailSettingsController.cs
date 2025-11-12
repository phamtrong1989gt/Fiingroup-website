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
    public class EmailSettingsController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IFileRepository _iFileRepository;
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly IEmailSenderRepository _iEmailSenderRepository;
        private readonly IPortalRepository _iPortalRepository;
        private readonly IEmailSettingRepository _iEmailSettingRepository;
        public EmailSettingsController(
            ILogger<EmailSettingsController> logger, 
            IFileRepository iFileRepository, 
            IWebHostEnvironment iHostingEnvironment, 
            IOptions<EmailSettings> emailSettings,
            IEmailSenderRepository iEmailSenderRepository,
            IPortalRepository iPortalRepository,
            IEmailSettingRepository iEmailSettingRepository
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
            _iEmailSettingRepository= iEmailSettingRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            ViewData["portals"] = portals;
            return View(_emailSettings.Value);
        }

        [HttpGet]
        public async Task<IActionResult> Setting(int portalId)
        {
            var dl = await _iEmailSettingRepository.SingleOrDefaultAsync(false, x => x.PortalId == portalId);
            if (dl == null)
            {
                dl = new EmailSetting() { PortalId = portalId };
                await _iEmailSettingRepository.AddAsync(dl);
                await _iEmailSettingRepository.CommitAsync();
            }
            return View(dl);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Setting")]
        public async Task<ResponseModel> SettingPost(EmailSetting model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iEmailSettingRepository.SingleOrDefaultAsync(false, x => x.PortalId == model.PortalId);
                    if (dl == null)
                    {
                        return new ResponseModel() { Output = 2, Message = "Cấu hình không tồn tại.", Type = ResponseTypeMessage.Warning };
                    }

                    dl.EmailServer = model.EmailServer;
                    dl.From = model.From;
                    dl.Host = model.Host;
                    dl.Password = model.Password;
                    dl.Port = model.Port;
                    dl.CC = model.CC;

                    _iEmailSettingRepository.Update(dl);
                    await _iEmailSettingRepository.CommitAsync();
                    await _iPortalRepository.TriggerRemoteCacheRefreshByKeyAsync(dl.PortalId, $"EmailSetting::{dl.PortalId}");
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
    }
}