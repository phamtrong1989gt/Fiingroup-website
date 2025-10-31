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
using PT.Base;

namespace PT.BE.Areas.Setting.Controllers
{
    [Area("Manager")]
    [AuthorizePermission("Index", "AdvertisingBannerManager")]
    public class AdvertisingHomepageManagerController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IFileRepository _iFileRepository;
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly IOptions<List<AdvertisingHomepageSettings>> _advertisingHomepageSettings;
        public AdvertisingHomepageManagerController(
            ILogger<AdvertisingHomepageManagerController> logger, 
            IFileRepository iFileRepository, 
            IWebHostEnvironment iHostingEnvironment, 
            IOptions<List<AdvertisingHomepageSettings>> advertisingHomepageSettings
            )
        {
            controllerName = "AdvertisingHomepageManager";
            tableName = "Base";

            _logger = logger;
            _iFileRepository = iFileRepository;
            _iHostingEnvironment = iHostingEnvironment;
            _advertisingHomepageSettings = advertisingHomepageSettings;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        #region [Index]
        [HttpGet]
        public IActionResult IndexAjax(string language = "vi")
        {
            var dl = _advertisingHomepageSettings.Value.FirstOrDefault(x => x.Id == language);
            if(dl==null)
            {
                dl = new AdvertisingHomepageSettings();
            }
            dl.Id = language;
            return View(dl);
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("IndexAjax")]
        public async Task<ResponseModel> IndexAjaxPost(AdvertisingHomepageSettings model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var data = _advertisingHomepageSettings.Value.FirstOrDefault(x => x.Id == model.Id);
                    if (data == null)
                    {
                        data = new AdvertisingHomepageSettings
                        {
                            Id = model.Id
                        };
                        _advertisingHomepageSettings.Value.Add(data);
                    }
                    data.DisplayTime = model.DisplayTime;
                    data.TimeOut = model.TimeOut;
                    data.Banner = model.Banner;
                    data.StartDate = model.StartDate;
                    data.EndDate = model.EndDate;
                    data.Template = model.Template;
                    data.Status = model.Status;
                    data.Width = model.Width;
                    data.Height = model.Height;
                    data.IsBannerImg = model.IsBannerImg;
                    data.Href = model.Href;
                    data.Target = model.Target;
                    data.Rel = model.Rel;
                    data.HeaderText1 = model.HeaderText1;
                    data.HeaderText2 = model.HeaderText2;
                    data.ContentText = model.ContentText;
                    data.Phone = model.Phone;
                    data.UpdatedDate = DateTime.Now;
                    var urlFile = $"{_iHostingEnvironment.WebRootPath}\\{data.Banner}";
                    if (System.IO.File.Exists(urlFile))
                    {
                        using (var image = System.Drawing.Image.FromFile(urlFile))
                        {
                            data.Width = image.Width;
                            data.Height = image.Height;
                            image.Dispose();
                        }
                    }
                    else
                    {
                        data.Width = 0;
                        data.Height = 0;
                    }

                    _iFileRepository.SettingsUpdate(_iHostingEnvironment.ContentRootPath + "/appsettings.AdvertisingHomepage.json", new { AdvertisingHomepageSettings = _advertisingHomepageSettings.Value });

                    await AddLog(new LogModel { Name = $"Cập nhật cấu hình quảng cáo trang chủ {model.Id}.", Type = LogType.Edit });

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
    }
}