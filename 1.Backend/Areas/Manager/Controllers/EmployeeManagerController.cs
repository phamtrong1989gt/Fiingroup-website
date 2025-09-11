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
using PT.Base;

namespace PT.UI.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class EmployeeManagerController : Base.Controllers.BaseController
    {
        private readonly ILogger _logger;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IEmployeeRepository _iEmployeeRepository;
        private readonly ILinkRepository _iLinkRepository;

        public EmployeeManagerController(
            ILogger<EmployeeManagerController> logger,
            IOptions<BaseSettings> baseSettings,
            IEmployeeRepository iEmployeeRepository,
            ILinkRepository iLinkRepository
        )
        {
            controllerName = "EmployeeManager";
            tableName = "Employee";
            _logger = logger;
            _baseSettings = baseSettings;
            _iEmployeeRepository = iEmployeeRepository;
            _iLinkRepository = iLinkRepository;
        }

        #region [Index]
        [AuthorizePermission]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, int? categoryId, int? tagId, bool? status, string language ="vi", string ordertype = "asc", string orderby = "name")
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iEmployeeRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                    m =>(m.FullName.Contains(key) || key == null) && 
                        (m.Language== language) && 
                        (m.Status==status || status ==null) && 
                        !m.Delete,
                OrderByExtention(ordertype, orderby));
                data.ReturnUrl =  Url.Action("Index", 
                    new {
                        page,
                        limit,
                        key,
                        categoryId,
                        tagId,
                        status,
                        ordertype,
                        orderby
            });
            return View("IndexAjax", data);
        }
        private Func<IQueryable<Employee>, IOrderedQueryable<Employee>> OrderByExtention(string ordertype, string orderby)
        {
            Func<IQueryable<Employee>, IOrderedQueryable<Employee>> functionOrder = null;
            switch (orderby)
            {
                case "name":
                    functionOrder = ordertype == "asc" ? EntityExtention<Employee>.OrderBy(m => m.OrderBy(x => x.FullName)) : EntityExtention<Employee>.OrderBy(m => m.OrderByDescending(x => x.FullName));
                    break;
                default:
                    functionOrder = ordertype == "asc" ? EntityExtention<Employee>.OrderBy(m => m.OrderBy(x => x.Id)) : EntityExtention<Employee>.OrderBy(m => m.OrderByDescending(x => x.Id));
                    break;
            }
            return functionOrder;
        }
        #endregion

        #region [Create]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Create(string language = "vi")
        {
            var dl = new EmployeeModel
            {
                Language = language
            };
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
            if(language != "vi")
            {
                dl.EmployeeMappingSelectList = new SelectList(await _iEmployeeRepository.SearchAsync(true, 0, 0, x => !x.Delete && x.Language== "vi"),"Id", "FullName");
            }
            return View(dl);
        }
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(EmployeeModel use)
        {
            try
            {
                //await CheckValidateSlug(use.Slug, CategoryType.Employee, use.Id,use.Language);
                if (ModelState.IsValid)
                {
                    // Add kink

                    var data = new Employee
                    {
                        FullName = use.FullName,
                        Banner = use.Banner,
                        Content = use.Content,
                        Delete = false,
                        Status = use.Status,
                        Language = use.Language,
                        Degrees =use.Degrees,
                        Email =use.Email,
                        Facebook =use.Facebook,
                        Gender =use.Gender,
                        Job =use.Job,
                        Office =use.Office,
                        Phone =use.Phone,
                        EmployeeMappingId = use.EmployeeMappingId,

                        Endodontics =use.Endodontics,
                        GeneralDentistry=use.GeneralDentistry,
                        OralMedicine=use.OralMedicine,
                        OralSurgery=use.OralSurgery,
                        Orthodontics=use.Orthodontics,
                        Periodontics=use.Periodontics,
                        Prosthodontics=use.Prosthodontics,
                        Summary =use.Summary
                    };
                    await _iEmployeeRepository.AddAsync(data);
                    await _iEmployeeRepository.CommitAsync();

                    //await AddSeoLink(CategoryType.Employee, data.Language, data.Id, MapModel<SeoModel>.Go(use));

                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới nhân viên \"{data.FullName}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới nhân viên thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = 0, Message = "Đường dẫn đã tồn tại, vui lòng thay đổi hoặc thêm một số ký tự khác bao gồm (a-z), (0-9), (-,/), vui lòng chọn tab thông tin seo để thay đổi Url", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [Edit]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Edit(int id)
        {
            var dl = await _iEmployeeRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null || (dl != null && dl.Delete))
            {
                return View("404");
            }
            var model = MapModel<EmployeeModel>.Go(dl);
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{dl.Language}" : "";
            //var ktLink = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.ObjectId == id && x.Type == CategoryType.Employee);
            //if (ktLink != null)
            //{
            //    model.Changefreq = ktLink.Changefreq;
            //    model.Lastmod = ktLink.Lastmod;
            //    model.Priority = ktLink.Priority.ConvertToString();
            //    model.Description = ktLink.Description;
            //    model.FacebookBanner = ktLink.FacebookBanner;
            //    model.FacebookDescription = ktLink.FacebookDescription;
            //    model.FocusKeywords = ktLink.FocusKeywords;
            //    model.GooglePlusDescription = ktLink.GooglePlusDescription;
            //    model.IncludeSitemap = ktLink.IncludeSitemap;
            //    model.Keywords = ktLink.Keywords;
            //    model.MetaRobotsAdvance = ktLink.MetaRobotsAdvance;
            //    model.MetaRobotsFollow = ktLink.MetaRobotsFollow;
            //    model.MetaRobotsIndex = ktLink.MetaRobotsIndex;
            //    model.Redirect301 = ktLink.Redirect301;
            //    model.Title = ktLink.Title;
            //    model.LinkId = ktLink.Id;
            //    model.Slug = ktLink.Slug;
            //}
            if (model.Language != "vi")
            {
                model.EmployeeMappingSelectList = new SelectList(await _iEmployeeRepository.SearchAsync(true, 0, 0, x => !x.Delete && x.Language == "vi"), "Id", "FullName");
            }
            return View(model);
        }
        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(EmployeeModel use, int id)
        {
            try
            {
                //await CheckValidateSlug(use.Slug, CategoryType.Employee, use.Id, use.Language);
                if (ModelState.IsValid)
                {
                    var dl = await _iEmployeeRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null || (dl != null && dl.Delete))
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }
                    dl.FullName = use.FullName;
                    dl.Banner = use.Banner;
                    dl.Content = use.Content;
                    dl.Status = use.Status;
          
                    dl.Degrees = use.Degrees;
                    dl.Email = use.Email;
                    dl.Facebook = use.Facebook;
                    dl.Gender = use.Gender;
                    dl.Job = use.Job;
                    dl.Office = use.Office;
                    dl.Phone = use.Phone;
                    dl.EmployeeMappingId = use.EmployeeMappingId;


                    dl.Endodontics = use.Endodontics;
                    dl.GeneralDentistry = use.GeneralDentistry;
                    dl.OralMedicine = use.OralMedicine;
                    dl.OralSurgery = use.OralSurgery;
                    dl.Orthodontics = use.Orthodontics;
                    dl.Periodontics = use.Periodontics;
                    dl.Prosthodontics = use.Prosthodontics;
                    dl.Summary = use.Summary;
                    _iEmployeeRepository.Update(dl);
                    await _iEmployeeRepository.CommitAsync();

                   //await UpdateSeoLink(use.ChangeSlug, CategoryType.Employee, dl.Id, dl.Language, MapModel<SeoModel>.Go(use));

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật nhân viên \"{dl.FullName}\".",
                        Type = LogType.Edit
                    });

                    return new ResponseModel() { Output = 1, Message = "Cập nhật nhân viên thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = 0, Message = "Đường dẫn đã tồn tại, vui lòng thay đổi hoặc thêm một số ký tự khác bao gồm (a-z), (0-9), (-,/), vui lòng chọn tab thông tin seo để thay đổi Url", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [Delete]
        [HttpPost, ActionName("Delete")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeletePost(int id)
        {
            try
            {
                var kt = await _iEmployeeRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null || (kt != null && kt.Delete))
                {
                    return new ResponseModel() { Output = 0, Message = "nhân viên không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                kt.Delete = true;
                await _iEmployeeRepository.CommitAsync();
                //await DeleteSeoLink(CategoryType.Employee, kt.Id);
                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa nhân viên \"{kt.FullName}\".",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa nhân viên thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

    }
}