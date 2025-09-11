using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System.Linq;
using PT.Shared;
using PT.Base;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Net.Http.Headers;
using PT.Infrastructure;

namespace PT.UI.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class TourTypeManagerController : Base.Controllers.BaseController
    {
        private readonly ILogger _logger;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly ILinkRepository _iLinkRepository;
        private readonly ITourTypeRepository _iTourTypeRepository;
        private readonly IWebHostEnvironment _iWebHostEnvironment;
        private readonly IFileRepository _iFileRepository;


        public TourTypeManagerController(
            ILogger<TourTypeManagerController> logger,
            IOptions<BaseSettings> baseSettings,
            ILinkRepository iLinkRepository,
            ITourTypeRepository iTourTypeRepository,
            IWebHostEnvironment iWebHostEnvironmen,
            IFileRepository iFileRepository
        )
        {
            controllerName = "TourTypeManager";
            tableName = "TourType";
            _logger = logger;
            _baseSettings = baseSettings;
            _iLinkRepository = iLinkRepository;
            _iTourTypeRepository = iTourTypeRepository;
            _iWebHostEnvironment = iWebHostEnvironmen;
            _iFileRepository = iFileRepository;
        }

        #region [Index]
        [AuthorizePermission]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, bool? status, string language = "vi", string ordertype = "asc", string orderby = "name")
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iTourTypeRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                m => (m.Name.Contains(key) || key == null) &&
                (m.Language == language) &&
                (m.Status == status || status == null) &&
                !m.Delete, OrderByExtention(ordertype, orderby));

            return View("IndexAjax", data);
        }
        private Func<IQueryable<TourType>, IOrderedQueryable<TourType>> OrderByExtention(string ordertype, string orderby)
        {
            return orderby switch
            {
                "name" => ordertype == "asc" ? EntityExtention<TourType>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<TourType>.OrderBy(m => m.OrderByDescending(x => x.Name)),
                _ => ordertype == "asc" ? EntityExtention<TourType>.OrderBy(m => m.OrderBy(x => x.Id)) : EntityExtention<TourType>.OrderBy(m => m.OrderByDescending(x => x.Id)),
            };
        }
        #endregion

        #region [Create]
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Create(string language = "vi")
        {
            var dl = new TourTypeModel
            {
                Language = language,
                Type= CategoryType.TourType
            };
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
            return View(dl);
        }
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(TourTypeModel use, string altId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _iTourTypeRepository.BeginTransaction();
                    var data = new TourType
                    {
                        Name = use.Name,
                        Delete = false,
                        Banner = use.Banner,
                        Content = use.Content,
                        Status = use.Status,
                        Language = use.Language
                    };
                    await _iTourTypeRepository.AddAsync(data);
                    await _iTourTypeRepository.CommitAsync();

                    await AddSeoLink(CategoryType.TourType, data.Language, data.Id, MapModel<SeoModel>.Go(use), data.Name, "", "TourTypeHome", "Tours");
                    await UpdateFileData(data.Id, CategoryType.TourType, altId);

                    await _iTourTypeRepository.CommitTransaction();

                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới TourType \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới loại tour thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
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
            var dl = await _iTourTypeRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null || (dl != null && dl.Delete))
            {
                return View("404");
            }
            var model = MapModel<TourTypeModel>.Go(dl);
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{dl.Language}" : "";
            var ktLink = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.ObjectId == id && x.Type == CategoryType.TourType);
            if (ktLink != null)
            {
                model.Changefreq = ktLink.Changefreq;
                model.Lastmod = ktLink.Lastmod;
                model.Priority = ktLink.Priority.ConvertToString();
                model.Description = ktLink.Description;
                model.FacebookBanner = ktLink.FacebookBanner;
                model.FacebookDescription = ktLink.FacebookDescription;
                model.FocusKeywords = ktLink.FocusKeywords;
                model.GooglePlusDescription = ktLink.GooglePlusDescription;
                model.IncludeSitemap = ktLink.IncludeSitemap;
                model.Keywords = ktLink.Keywords;
                model.MetaRobotsAdvance = ktLink.MetaRobotsAdvance;
                model.MetaRobotsFollow = ktLink.MetaRobotsFollow;
                model.MetaRobotsIndex = ktLink.MetaRobotsIndex;
                model.Redirect301 = ktLink.Redirect301;
                model.Title = ktLink.Title;
                model.LinkId = ktLink.Id;
                model.Slug = ktLink.Slug;
                model.Type = ktLink.Type;
            }
            return View(model);
        }
        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(TourTypeModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _iTourTypeRepository.BeginTransaction();
                    var dl = await _iTourTypeRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null || (dl != null && dl.Delete))
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    dl.Name = use.Name;
                    dl.Status = use.Status;
                    dl.Banner = use.Banner;
                    dl.Content = use.Content;
                    dl.BannerHeader = use.BannerHeader;
                    dl.BannerFooter = use.BannerFooter;

                    _iTourTypeRepository.Update(dl);
                    await _iTourTypeRepository.CommitAsync();

                 
                    await UpdateSeoLink(use.ChangeSlug, CategoryType.TourType, dl.Id, dl.Language, MapModel<SeoModel>.Go(use), dl.Name, "", "TourTypeHome", "Tours");

                    await _iTourTypeRepository.CommitTransaction();

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật TourType \"{dl.Name}\".",
                        Type = LogType.Edit
                    });

                    return new ResponseModel() { Output = 1, Message = "Cập nhật loại tour thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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

        #region [Delete]
        [HttpPost, ActionName("Delete")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeletePost(int id)
        {
            try
            {
                var kt = await _iTourTypeRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null || (kt != null && kt.Delete))
                {
                    return new ResponseModel() { Output = 0, Message = "Loại tour không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }

                kt.Delete = true;
                await _iTourTypeRepository.CommitAsync();

                await DeleteSeoLink(CategoryType.TourType, kt.Id);

                await RemoveFileData(id, CategoryType.TourType);

                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa TourType \"{kt.Name}\".",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa loại tour thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [Upload file]
        [HttpPost, ActionName("UploadImage")]
        [AuthorizePermission("Index")]
        public async Task<object> UploadImagePost(string altId, int id, int type = 0)
        {
            try
            {
                string[] allowedExtensions = _baseSettings.Value.ImagesType.Split(',');
                string path = $"{_iWebHostEnvironment.WebRootPath}/Data" + Functions.GenFolderByDate();
                string pathServer = $"/Data" + Functions.GenFolderByDate();

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var files = Request.Form.Files;
                foreach (var file in files)
                {
                    if (!allowedExtensions.Contains(Path.GetExtension(file.FileName)))
                    {
                        return new ResponseModel<FileDataModel>() { Output = 2, Message = "Tệp tải lên không đúng định dạng.", Type = ResponseTypeMessage.Warning };
                    }
                    else if (_baseSettings.Value.ImagesMaxSize < file.Length)
                    {
                        return new ResponseModel<FileDataModel>() { Output = 3, Message = "Tệp tải lên vượt quá kích thước cho phép.", Type = ResponseTypeMessage.Warning };
                    }
                    else
                    {
                        var newFilename = Path.GetFileName(file.FileName);
                        if (System.IO.File.Exists(path + file.Name))
                        {
                            newFilename = $"{Path.GetFileName(file.FileName)}_{id}_{DateTime.Now:yyyyMMddHHmmss}";
                        }

                        string pathFile = ContentDispositionHeaderValue
                        .Parse(file.ContentDisposition)
                        .FileName
                        .Trim('"');

                        pathFile = $"{path}{newFilename}";
                        pathServer = $"{pathServer}{newFilename}";

                        using var image = System.Drawing.Image.FromStream(file.OpenReadStream());
                        if(image.Width > _baseSettings.Value.ImageMaxWith)
                        {
                            _iFileRepository.ResizeImage(file, pathFile, _baseSettings.Value.ImageMaxWith, false);
                        }    
                        else
                        {
                            using var stream = new FileStream(pathFile, FileMode.Create);
                            await file.CopyToAsync(stream);
                        }    

                        await AddFileData(id, pathServer, CategoryType.TourType, altId);

                        if(type == 1)
                        {
                            return new FileDataCKEditerModel()
                            {
                                FileName = Path.GetFileName(pathServer),
                                Number = 200,
                                Uploaded = 1,
                                Url = pathServer
                            };
                        }   
                        else
                        {
                            return new ResponseModel<FileDataModel>()
                            {
                                Output = 1,
                                Message = "Tải tệp lên thành công.",
                                Type = ResponseTypeMessage.Success,
                                Data = new FileDataModel
                                {
                                    CreatedDate = DateTime.Now,
                                    CreatedUser = DataUserInfo.UserId,
                                    Path = pathServer,
                                    FileName = Path.GetFileName(pathServer)
                                },
                                IsClosePopup = false
                            };
                        }    
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel<FileDataModel>() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

    }
}