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
    public class ScriptSettingsController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IFileRepository _iFileRepository;
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly IEmailSenderRepository _iEmailSenderRepository;
        private readonly IPortalRepository _iPortalRepository;
        private readonly IBindContentSettingRepository _iBindContentSettingRepository;

        public ScriptSettingsController(
            ILogger<ScriptSettingsController> logger, 
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
        public async Task<IActionResult> Index()
        {
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            ViewData["portals"] = portals;
            return View();
        }

        #region [Setting]
        [HttpGet]
        public async Task<IActionResult> Setting(int portalId)
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

        [HttpPost, ValidateAntiForgeryToken, ActionName("Setting")]
        public async Task<ResponseModel> SettingPost(BindContentSetting model)
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
                    _iBindContentSettingRepository.Update(dl);
                    await _iBindContentSettingRepository.CommitAsync();
                    await AddLog(new LogModel { Name = $"Cập nhật cấu thông tin nội dung thêm ", Type = LogType.Edit });
                    await _iPortalRepository.TriggerRemoteCacheRefreshByKeyAsync(dl.PortalId, $"BindContentSetting::{dl.PortalId}");
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