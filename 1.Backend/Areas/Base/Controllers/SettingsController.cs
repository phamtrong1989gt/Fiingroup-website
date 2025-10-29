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
using PT.UI.Areas.Base.Controllers;
using PT.Shared;
using PT.Base;

namespace PT.UI.Areas.Setting.Controllers
{
    [Area("Base")]
    [AuthorizePermission("Index")]
    public class SettingsController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IFileRepository _iFileRepository;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly IEmailSenderRepository _iEmailSenderRepository;
        private readonly IOptions<List<WebsiteInfoSettings>> _websiteInfoSettings;
        private readonly IOptions<List<SeoSettings>> _seoSettings;
        private readonly IOptions<BindContentSettings> _bindContentSettings;
        private readonly IOptions<ExchangeRateSettings> _exchangeRateSettings;

        public SettingsController(
            ILogger<BaseController> logger, 
            IOptions<BaseSettings> baseSettings, 
            IFileRepository iFileRepository, 
            IWebHostEnvironment iHostingEnvironment, 
            IOptions<EmailSettings> emailSettings,
            IEmailSenderRepository iEmailSenderRepository,
            IOptions<List<WebsiteInfoSettings>> websiteInfoSettings,
            IOptions<List<SeoSettings>> seoSettings,
            IOptions<BindContentSettings> bindContentSettings,
            IOptions<ExchangeRateSettings> exchangeRateSettings
            )
        {
            controllerName = "Settings";
            tableName = "Base";

            _logger = logger;
            _baseSettings = baseSettings;
            _iFileRepository = iFileRepository;
            _iHostingEnvironment = iHostingEnvironment;
            _emailSettings = emailSettings;
            _iEmailSenderRepository = iEmailSenderRepository;
            _websiteInfoSettings = websiteInfoSettings;
            _seoSettings = seoSettings;
            _bindContentSettings = bindContentSettings;
            _exchangeRateSettings = exchangeRateSettings;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        #region [Website]
        [HttpGet]
        public IActionResult Website()
        {
            return View(_baseSettings.Value);
        }
        [HttpPost,ValidateAntiForgeryToken,ActionName("Website")]
        public async Task<ResponseModel> WebsitePost(BaseSettings model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var data = _baseSettings.Value;
                    data.DocumentsMaxSize = model.DocumentsMaxSize;
                    data.ImagesType = model.ImagesType;
                    data.ImagesMaxSize = model.ImagesMaxSize;
                    data.DocumentsType = model.DocumentsType;
                    data.IsCapCha = model.IsCapCha;
                    data.CapChaDataSitekey = model.CapChaDataSitekey;
                    data.CapChaSecret = model.CapChaSecret;
                    data.GoogleMapKey = model.GoogleMapKey;
                    data.ClientIdFaceBook = model.ClientIdFaceBook;
                    data.ClientSecretFaceBook = model.ClientSecretFaceBook;
                    data.ToEmail = model.ToEmail;
                    _iFileRepository.SettingsUpdate(_iHostingEnvironment.ContentRootPath + "/appsettings.Base.json", new { BaseSettings = _baseSettings.Value });

                    await AddLog(new LogModel { Name = $"Cập nhật cấu hình chung.", Type = LogType.Edit });
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
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

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

        #region [Info]

        [HttpGet]
        public IActionResult Info(string language="vi")
        {
            var dl = _websiteInfoSettings.Value.FirstOrDefault(x=>x.Id==language);
            if(dl==null)
            {
                dl = new WebsiteInfoSettings { Id = language };
            }
            return View(dl);
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("Info")]
        public async Task<ResponseModel> InfoPost(WebsiteInfoSettings model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var data = _websiteInfoSettings.Value.FirstOrDefault(x => x.Id == model.Id);
                    if (data == null)
                    {
                        data = new WebsiteInfoSettings
                        {
                            Id = model.Id
                        };
                        _websiteInfoSettings.Value.Add(data);
                    }
                    data.Email = model.Email;
                    data.Email2 = model.Email2;
                    data.Email3 = model.Email3;

                    data.Logo = model.Logo;
                    data.Map = model.Map;
                    data.Name = model.Name;
                    data.Note = model.Note;
                    data.Phone = model.Phone;
                    data.Phone2 = model.Phone2;
                    data.Phone3 = model.Phone3;

                    data.Video = model.Video;
                    data.Website = model.Website;
                    data.Address = model.Address;
                    data.Address2 = model.Address2;
                    data.Address3 = model.Address3;

                    data.Fax = model.Fax;
                    data.Hotline = model.Hotline;
                    data.Hotline2 = model.Hotline2;
                    data.Hotline3 = model.Hotline3;

                    _iFileRepository.SettingsUpdate(_iHostingEnvironment.ContentRootPath + "/appsettings.WebsiteInfo.json", new { WebsiteInfoSettings = _websiteInfoSettings.Value });

                    await AddLog(new LogModel { Name = $"Cập nhật cấu thông tin website {model.Id}.", Type = LogType.Edit });

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
        #endregion

        #region [BindContent]
        [HttpGet]
        public IActionResult BindContent()
        {
            var dl = _bindContentSettings.Value;
            if(dl==null)
            {
                dl = new BindContentSettings()
                {
                   
                };
            }
            return View(dl);
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("BindContent")]
        public async Task<ResponseModel> BindContentPost(BindContentSettings model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var data = _bindContentSettings.Value;
                    if (data == null)
                    {
                        data = new BindContentSettings();
                    }
                    data.Head = model.Head;
                    data.Body = model.Body;
                    data.Footer = model.Footer;

                    _iFileRepository.SettingsUpdate(_iHostingEnvironment.ContentRootPath + "/appsettings.BindContent.json", new { BindContentSettings = _bindContentSettings.Value });

                    await AddLog(new LogModel { Name = $"Cập nhật cấu thông tin nội dung thêm ", Type = LogType.Edit });

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

        #region [BindContent]
        [HttpGet]
        public IActionResult ExchangeRate()
        {
            var dl = _exchangeRateSettings.Value;
            if (dl == null)
            {
                dl = new ExchangeRateSettings()
                {

                };
            }
            return View(dl);
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("ExchangeRate")]
        public async Task<ResponseModel> ExchangeRatePost(ExchangeRateSettings model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var data = _exchangeRateSettings.Value;
                    if (data == null)
                    {
                        data = new ExchangeRateSettings();
                    }
                    data.DollarToVND = model.DollarToVND;
                    data.PoundToVND = model.PoundToVND;
                    data.AUDToVND = model.AUDToVND;

                    _iFileRepository.SettingsUpdate(_iHostingEnvironment.ContentRootPath + "/appsettings.ExchangeRate.json", new { ExchangeRateSettings = data });

                    await AddLog(new LogModel { Name = $"Cập nhật cấu thông tin nội dung thêm ", Type = LogType.Edit });

                    return new ResponseModel() { Output = 1, Message = "Cập nhật cấu hình tỉ giá thành công.", Type = ResponseTypeMessage.Success };
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