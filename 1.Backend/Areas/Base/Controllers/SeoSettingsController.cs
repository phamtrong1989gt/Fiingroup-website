using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Base;
using PT.Base.Services;
using PT.BE.Areas.Base.Controllers;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PT.BE.Areas.Setting.Controllers
{
    [Area("Base")]
    [AuthorizePermission("Index")]
    public class SeoSettingsController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IFileRepository _iFileRepository;
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly ILinkRepository _iLinkRepository;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly IEmployeeRepository _iEmployeeRepository;
        private readonly ISeoSettingRepository _iSeoSettingRepository;
        private readonly IPortalRepository _iPortalRepository;
        public SeoSettingsController(
            ILogger<SeoSettingsController> logger,
            IFileRepository iFileRepository,
            IWebHostEnvironment iHostingEnvironment,
            ILinkRepository iLinkRepository,
            IOptions<BaseSettings> baseSettings,
            IContentPageRepository iContentPageRepository,
            ICategoryRepository iCategoryRepository,
            IEmployeeRepository iEmployeeRepository,
            ISeoSettingRepository iSeoSettingRepository,
            IPortalRepository iPortalRepository
            )
        {
            controllerName = "SeoSettings";
            tableName = "Base";

            _logger = logger;
            _iFileRepository = iFileRepository;
            _iHostingEnvironment = iHostingEnvironment;
            _iLinkRepository = iLinkRepository;
            _baseSettings = baseSettings;
            _iContentPageRepository = iContentPageRepository;
            _iCategoryRepository = iCategoryRepository;
            _iEmployeeRepository = iEmployeeRepository;
            _iSeoSettingRepository = iSeoSettingRepository;
            _iPortalRepository = iPortalRepository;
        }
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Index()
        {
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            ViewData["portals"] = portals;
            return View();
        }

        #region [Seo]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Seo(string language = "vi", int portalId = 1)
        {
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            var data = await _iSeoSettingRepository.SingleOrDefaultAsync(true, x => x.Language == language && x.PortalId == portalId);
            if (data == null)
            {
                return View(new SeoSetting { Language = language, PortalId = portalId, Portals = portals });
            }
            else
            {
                data.Portals = portals;
                return View(data);
            }
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Seo")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> SeoPost(SeoSetting model, string language = "vi", int portalId = 1)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var data = await _iSeoSettingRepository.SingleOrDefaultAsync(true, x => x.Language == language && x.PortalId == portalId);
                    if (data == null)
                    {
                        _iSeoSettingRepository.Add(new SeoSetting
                        {
                            Language = language,
                            PortalId = portalId,
                            Title = model.Title,
                            Description = model.Description,
                            Keywords = model.Keywords,
                            MetaGoogle = model.MetaGoogle,
                            Robots = model.Robots
                        });
                    }
                    else
                    {
                        data.Title = model.Title;
                        data.Description = model.Description;
                        data.Keywords = model.Keywords;
                        data.MetaGoogle = model.MetaGoogle;
                        data.Robots = model.Robots;
                        _iSeoSettingRepository.Update(data);
                    }
                    await _iSeoSettingRepository.CommitAsync();
                    await _iPortalRepository.TriggerRemoteCacheRefreshByKeyAsync(data.PortalId, $"SeoSetting::{data.Language}::{data.PortalId}");

                    await AddLog(new LogModel { Name = $"Cập nhật cấu thông tin seo {model.Id}.", Type = LogType.Edit });

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

        [Authorize]
        [HttpPost, ActionName("SeoAnalysisResult")]
        public async Task<IActionResult> SeoAnalysisResult(string url, string keyword)
        {
            var analyzer = new SeoContentAnalyzer();
            var result = await analyzer.AnalyzeFromUrlAsync(url, keyword);
            return View("SeoAnalysisResult", result);
        }


    }
}