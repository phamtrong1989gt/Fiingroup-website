using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System.Linq;
using PT.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using PT.Base;
using System.IO;
using System.Net.Http.Headers;

namespace PT.BE.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class TourManagerController : Base.Controllers.BaseController
    {
        private readonly ILogger _logger;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ILinkRepository _iLinkRepository;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly IContentPageTagRepository _iContentPageTagRepository;
        private readonly ITourRepository _iTourRepository;
        private readonly ITourDayRepository _iTourDayRepository;
        private readonly IWebHostEnvironment _iWebHostEnvironment;
        private readonly ITourCategoryRepository _iTourCategoryRepository;
        private readonly IFileRepository _iFileRepository;
        private readonly ITourTypeRepository _iTourTypeRepository;

        public TourManagerController(
            ILogger<TourManagerController> logger,
            IOptions<BaseSettings> baseSettings,
            IContentPageRepository iContentPageRepository,
            ILinkRepository iLinkRepository,
            ICategoryRepository iCategoryRepository,
            IContentPageTagRepository iContentPageTagRepository,
            ITourRepository iTourRepository,
            ITourDayRepository iTourDayRepository,
            IWebHostEnvironment iWebHostEnvironment,
            ITourCategoryRepository iTourCategoryRepository,
            IFileRepository iFileRepository,
            ITourTypeRepository iTourTypeRepository
        )
        {
            controllerName = "TourManager";
            tableName = "Tour";
            _logger = logger;
            _baseSettings = baseSettings;
            _iContentPageRepository = iContentPageRepository;
            _iLinkRepository = iLinkRepository;
            _iCategoryRepository = iCategoryRepository;
            _iContentPageTagRepository = iContentPageTagRepository;
            _iTourRepository = iTourRepository;
            _iTourDayRepository = iTourDayRepository;
            _iWebHostEnvironment = iWebHostEnvironment;
            _iTourCategoryRepository = iTourCategoryRepository;
            _iFileRepository = iFileRepository;
            _iTourTypeRepository = iTourTypeRepository;
        }

        #region [Index]
        [AuthorizePermission]
        public async Task<IActionResult> Index()
        {
            ViewData["TourStyleSelectListItems"] = new List<SelectListItem>()
            {
                new() { Value = ((int)TourStyle.Tour).ToString(), Text = TourStyle.Tour.GetDisplayName()},
                new() { Value = ((int)TourStyle.Car).ToString(), Text = TourStyle.Car.GetDisplayName()},
                new() { Value = ((int)TourStyle.Hotel).ToString(), Text = TourStyle.Hotel.GetDisplayName()},
            };
            ViewData["TourTypeSelectListItems"] = new SelectList(await _iTourTypeRepository.SearchAsync(true, 0, 100, x => !x.Delete, null, x=> new TourType { Id = x.Id, Name = x.Name, Status = x.Status, Delete = x.Delete }), "Id", "Name");
            return View();
        }

        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(
            int? page, 
            int? limit, 
            string key, 
            int? categoryId, 
            bool? status, 
            TourStyle? style,
            int? tourTypeId,
            string language ="vi", 
            string ordertype = "asc", 
            string orderby = "name")
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;

            var data = await _iTourRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                categoryId,
                m =>(m.Name.Contains(key) || key == null) && (m.Language== language) &&  (m.Status==status || status ==null) && (m.Style == style || style == null) && (m.TourTypeId == tourTypeId || tourTypeId == null),
                OrderByExtention(ordertype, orderby));
              
            return View("IndexAjax", data);
        }

        private Func<IQueryable<Tour>, IOrderedQueryable<Tour>> OrderByExtention(string ordertype, string orderby)
        {
            return orderby switch
            {
                "name" => ordertype == "asc" ? EntityExtention<Tour>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<Tour>.OrderBy(m => m.OrderByDescending(x => x.Name)),
                _ => ordertype == "asc" ? EntityExtention<Tour>.OrderBy(m => m.OrderBy(x => x.CreatedDate)) : EntityExtention<Tour>.OrderBy(m => m.OrderByDescending(x => x.CreatedDate)),
            };
        }
        #endregion

        #region [Create]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Create(string language = "vi")
        {
            var dl = new TourModel
            {
                Language = language,
                Type = CategoryType.Tour
            };
            dl.PhotoFileDatas = GetFiles(dl.Photos);
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";

            dl.TourStyleSelectListItems =
            [
                new() { Value = ((int)TourStyle.Tour).ToString(), Text = TourStyle.Tour.GetDisplayName()},
                new() { Value = ((int)TourStyle.Car).ToString(), Text = TourStyle.Car.GetDisplayName()},
                new() { Value = ((int)TourStyle.Hotel).ToString(), Text = TourStyle.Hotel.GetDisplayName()},
            ];

            dl.TourTypeSelectListItems = new SelectList(await _iTourTypeRepository.SearchAsync(true, 0, 100, x => !x.Delete, null, x => new TourType { Id = x.Id, Name = x.Name, Status = x.Status, Delete = x.Delete }), "Id", "Name");

            return View(dl);
        }
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(TourModel use, string categoryIds,string altId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _iTourRepository.BeginTransaction();
                    var data = new Tour
                    {
                        Name = use.Name,
                        Banner = use.Banner,
                        Language = use.Language,
                        CreatedDate = DateTime.Now,
                        CreatedUser = DataUserInfo.UserId,
                        Days = use.Days,
                        DetailDifference = use.DetailDifference,
                        DetailNote = use.DetailNote,
                        DetailServicesExclusion = use.DetailServicesExclusion,
                        DetailServicesInclusion = use.DetailServicesInclusion,
                        IsTop = use.IsTop,
                        Nights = use.Nights,
                        Overview = use.Overview,
                        Photos = use.Photos,
                        Status = use.Status,
                        Style = use.Style,
                        Trips = use.Trips,
                        TourTypeId = use.TourTypeId,
                        Address = use.Address,
                        BannerFooter = use.BannerFooter,
                        BannerHeader = use.BannerHeader,
                        AdultPrice = use.AdultPrice,
                        ChildrenPrice = use.ChildrenPrice,
                        ElderlyPrice = use.ElderlyPrice,
                        InfantPrice = use.InfantPrice,
                        From = use.From,
                        To = use.To,
                        PickUp = use.PickUp,
                        PickOut = use.PickOut,
                        Order = use.Order
                    };

                    await _iTourRepository.AddAsync(data);
                    await _iTourRepository.CommitAsync();

                    await AddSeoLink(CategoryType.Tour, data.Language, data.Id, MapModel<SeoModel>.Go(use), data.Name, "", "TourHome", "Details");

                    await UpdateCategory(data.Id, categoryIds);

                    await _iTourRepository.CommitTransaction();

                    await UpdateFileData(data.Id, CategoryType.Tour, altId);

                    return new ResponseModel() { Output = 1, Message = "Thêm mới Tour thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin hoặc liên kết thân thiện/Permalink đã tồn tại, vui lòng thay thêm ký tự bất kỳ đằng sau", Type = ResponseTypeMessage.Warning };
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
            var dl = await _iTourRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null)
            {
                return View("404");
            }


            var model = MapModel<TourModel>.Go(dl);
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{dl.Language}" : "";
            var ktLink = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.ObjectId == id && x.Type == CategoryType.Tour);
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
            }

            var TourTagIds = (await _iContentPageTagRepository.SearchAsync(true, 0, 0, x => x.ContentPageId == id)).Select(x=>x.TagId).ToList();

            model.CategoryIds = string.Join(",", (await _iTourCategoryRepository.SearchAsync(true, 0, 0, x => x.TourId == id)).Select(x => x.CategoryId));
            model.PhotoFileDatas = GetFiles(model.Photos);
            model.TourStyleSelectListItems =
            [
                new() { Value = ((int)TourStyle.Tour).ToString(), Text = TourStyle.Tour.GetDisplayName()},
                new() { Value = ((int)TourStyle.Car).ToString(), Text = TourStyle.Car.GetDisplayName()},
                new() { Value = ((int)TourStyle.Hotel).ToString(), Text = TourStyle.Hotel.GetDisplayName()},
            ];
            model.TourTypeSelectListItems = new SelectList(await _iTourTypeRepository.SearchAsync(true, 0, 100, x => !x.Delete, null, x => new TourType { Id = x.Id, Name = x.Name, Status = x.Status, Delete = x.Delete }), "Id", "Name");
            return View(model);
        }
        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(TourModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iTourRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    await _iTourRepository.BeginTransaction();

                    dl.Name = use.Name;
                    dl.Banner = use.Banner;
                    dl.Language = use.Language;
                    dl.Days = use.Days;
                    dl.DetailDifference = use.DetailDifference;
                    dl.DetailNote = use.DetailNote;
                    dl.DetailServicesExclusion = use.DetailServicesExclusion;
                    dl.DetailServicesInclusion = use.DetailServicesInclusion;
                    dl.IsTop = use.IsTop;
                    dl.Nights = use.Nights;
                    dl.Overview = use.Overview;
                    dl.Photos = use.Photos;
                    dl.Status = use.Status;
                    dl.Style = use.Style;
                    dl.Trips = use.Trips;
                    dl.TourTypeId = use.TourTypeId;
                    dl.Address = use.Address;
                    dl.BannerFooter = use.BannerFooter;
                    dl.BannerHeader = use.BannerHeader;
                    dl.AdultPrice = use.AdultPrice;
                    dl.ChildrenPrice = use.ChildrenPrice;
                    dl.ElderlyPrice = use.ElderlyPrice;
                    dl.InfantPrice = use.InfantPrice;
                    dl.From = use.From;
                    dl.To = use.To;
                    dl.PickUp = use.PickUp;
                    dl.PickOut = use.PickOut;
                    dl.Order = use.Order;
                    _iTourRepository.Update(dl);
                    await _iContentPageRepository.CommitAsync();
                    await UpdateSeoLink(use.ChangeSlug, CategoryType.Tour, CategoryType.Tour, dl.Id, dl.Language, MapModel<SeoModel>.Go(use), dl.Name, "", "TourHome", "Details");
                    await UpdateCategory(id, use.CategoryIds);
                    await _iTourRepository.CommitTransaction();

                    return new ResponseModel() { Output = 1, Message = "Cập nhật tin tức thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin hoặc liên kết thân thiện/Permalink đã tồn tại, vui lòng thay thêm ký tự bất kỳ đằng sau.", Type = ResponseTypeMessage.Warning, IsClosePopup =true };
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
                var kt = await _iTourRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null)
                {
                    return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                await _iTourRepository.BeginTransaction();
                _iTourRepository.Delete(kt);
                await _iTourRepository.RemoveCategory(id);
                await _iTourRepository.CommitAsync();
                await DeleteSeoLink(CategoryType.Tour, kt.Id);
                await _iTourRepository.CommitTransaction();

                return new ResponseModel() { Output = 1, Message = "Xóa Tin tức thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [Tree Category]
        [HttpPost, Authorize]
        public async Task<List<TreeRoleModel>> TreeCategory(int id, string language="vi")
        {
            var listCurent = await _iTourCategoryRepository.SearchAsync(true, 0, 0, x => x.TourId == id);
            var listCategory = await _iCategoryRepository.SearchAsync(true, 0, 0, x =>  x.Status && x.Type == CategoryType.CategoryTour && x.Language==language);
            var abc  = listCategory.Select(x => 
            new TreeRoleModel {
                Id = x.Id.ToString(),
                Parent = x.ParentId==0?"#": x.ParentId.ToString(),
                Text = x.Name,
                State = new TreeRoleStateModel { Disabled = false, Opened = true, Selected = listCurent.Any(m => m.CategoryId == x.Id && !listCategory.Any(z => z.ParentId == x.Id)) },
                Icon = null
            }
            ).ToList();
            return abc;
        }
        #endregion

        #region [Search Category]
        [HttpGet, Authorize]
        public async Task<object> SearchCategory(string language = "vi")
        {
            var listCategory = await _iCategoryRepository.SearchAsync(true, 0, 0, x => x.Language == language && x.Status  && x.Type == CategoryType.CategoryTour);
            return GenSelectCategory(listCategory, 0, 0);
        }
        [NonAction]
        private List<SelectListItem> GenSelectCategory(List<Category> listCategory, int parentId = 0, int level = 0)
        {
            var list = new List<SelectListItem>();
            foreach (var item in listCategory.Where(x => x.ParentId == parentId).OrderBy(x => x.Order).ToList())
            {
                string spl = String.Concat(Enumerable.Repeat("---", level));
                list.Add(new SelectListItem { Text = $"{spl} {item.Name}", Value = item.Id.ToString() });
                list.AddRange(GenSelectCategory(listCategory, item.Id, level + 1));
            }
            return list;
        }
        #endregion

        #region [TourDaye]

        #region [ListTourDay]
        [HttpPost, ActionName("ListTourDay")]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> ListTourDayPost(int? page, int? tourId)
        {
            page = page < 0 ? 1 : page;
            var data = await _iTourDayRepository.SearchPagedListAsync(
            page ?? 1,
            30,
            m => m.TourId == tourId, m => m.OrderBy(x => x.Day));
            return View("ListTourDayAjax", data);
        }
        #endregion

        #region [CreateImage]
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult CreateTourDay(int tourId)
        {
            var model = new TourDayModel
            {
                TourId = tourId
            };
            model.PhotoFileDatas = GetFiles(model.Photos);
            return View(model);
        }

        [HttpPost, ActionName("CreateTourDay")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreateTourDayPost(TourDayModel use)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var data = new TourDay
                    {
                       TourId = use.TourId,
                       Details = use.Details,
                       Day = use.Day,
                       Id = use.Id,
                       IsCar = use.IsCar,
                       IsCruising = use.IsCruising,
                       IsCycling = use.IsCycling,
                       IsFlight = use.IsFlight,
                       IsHotel = use.IsHotel,
                       IsLocalBoat = use.IsLocalBoat,
                       IsLocalTouch = use.IsLocalTouch,
                       Photos = use.Photos,
                       Name = use.Name
                    };
                    await _iTourDayRepository.AddAsync(data);
                    await _iTourDayRepository.CommitAsync();
                   
                    return new ResponseModel() { Output = 1, Message = "Thêm mới dữ liệu thành công", Type = ResponseTypeMessage.Success, IsClosePopup = false };
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

        #region [EditImage]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> EditTourDay(int id)
        {
            var dl = await _iTourDayRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null)
            {
                return View("404");
            }
            var model = MapModel<TourDayModel>.Go(dl);
            model.PhotoFileDatas = GetFiles(model.Photos);
            return View(model);
        }
        [HttpPost, ActionName("EditTourDay")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditTourDayPost(TourDayModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iTourDayRepository.SingleOrDefaultAsync(true, m => m.Id == id);
                    if (dl == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }
                    dl.TourId = use.TourId;
                    dl.Details = use.Details;
                    dl.Day = use.Day;
                    dl.Id = use.Id;
                    dl.IsCar = use.IsCar;
                    dl.IsCruising = use.IsCruising;
                    dl.IsCycling = use.IsCycling;
                    dl.IsFlight = use.IsFlight;
                    dl.IsHotel = use.IsHotel;
                    dl.IsLocalBoat = use.IsLocalBoat;
                    dl.IsLocalTouch = use.IsLocalTouch;
                    dl.Photos = use.Photos;
                    dl.Name = use.Name;

                    _iTourDayRepository.Update(dl);
                    await _iTourDayRepository.CommitAsync();

                    return new ResponseModel() { Output = 1, Message = "Cập nhật dữ liệu thành công", Type = ResponseTypeMessage.Success, IsClosePopup = false };
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

        #region [DeleteTourDay]
        [HttpPost, ActionName("DeleteTourDay")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeleteTourDayPost(int id)
        {
            try
            {
                var kt = await _iTourDayRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null)
                {
                    return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại", Type = ResponseTypeMessage.Warning };
                }
                _iTourDayRepository.Delete(kt);
                await _iTourDayRepository.CommitAsync();

                await DeleteSeoLink(CategoryType.Tour, kt.Id);

                return new ResponseModel() { Output = 1, Message = "Xóa dữ liệu thành công", Type = ResponseTypeMessage.Success, IsRedirect = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion
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
                        if (image.Width > _baseSettings.Value.ImageMaxWith)
                        {
                            _iFileRepository.ResizeImage(file, pathFile, _baseSettings.Value.ImageMaxWith, false);
                        }
                        else
                        {
                            using var stream = new FileStream(pathFile, FileMode.Create);
                            await file.CopyToAsync(stream);
                        }

                        await AddFileData(id, pathServer, CategoryType.Tour, altId);

                        if (type == 1)
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

        [HttpPost, Authorize]
        public async Task<List<SelectListItem>> SearchContentPage(string q, int top = 10, string language = "vi")
        {
            top = top > 100 ? 100 : top;
            return (await _iTourRepository.SearchAsync(true, 0, top, x => x.Name.ToLower().Contains(q.ToLower()) && x.Status  && x.Language == language, x => x.OrderBy(y => y.Name),
                x => new Tour { Id = x.Id, Name = x.Name,  Status = x.Status, Language = x.Language})).Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
        }

        private async Task UpdateCategory(int TourId, string categoryIds)
        {
            var list = new List<int>();
            if (categoryIds != null)
            {
                list = categoryIds.Split(',').Select(x => int.Parse(x)).ToList();
            }
            var _currentCategory = await _iTourCategoryRepository.SearchAsync(true, 0, 0, x => x.TourId == TourId);
            var idsAdd = list.Where(x => !_currentCategory.Any(y => y.CategoryId == x));
            _iTourCategoryRepository.DeleteWhere(x => x.TourId == TourId && !list.Contains(x.CategoryId));
            foreach (var item in idsAdd)
            {
                await _iTourCategoryRepository.AddAsync(new TourCategory { TourId = TourId, CategoryId = item });
            }
            await _iTourCategoryRepository.CommitAsync();
        }

        private List<FileDataModel> GetFiles(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                {
                    return new List<FileDataModel>();
                }
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<FileDataModel>>(data);
            }
            catch
            {
                return new List<FileDataModel>();
            }
        }
    }
}