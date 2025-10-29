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

namespace PT.UI.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class ProductManagerController : Base.Controllers.BaseController
    {
        private readonly ILogger _logger;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IProductRepository _iProductRepository;
        private readonly ILinkRepository _iLinkRepository;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly IProductCategoryRepository _iProductCategoryRepository;
        private readonly IWebHostEnvironment _iWebHostEnvironment;
        private readonly IFileRepository _iFileRepository;

        public ProductManagerController(
            ILogger<ProductManagerController> logger,
            IOptions<BaseSettings> baseSettings,
            IProductRepository iProductRepository,
            ILinkRepository iLinkRepository,
            ICategoryRepository iCategoryRepository,
            IWebHostEnvironment iWebHostEnvironment,
            IFileRepository iFileRepository,
            IProductCategoryRepository iProductCategoryRepository
        )
        {
            controllerName = "ProductManager";
            tableName = "Product";
            _logger = logger;
            _baseSettings = baseSettings;
            _iProductRepository = iProductRepository;
            _iLinkRepository = iLinkRepository;
            _iCategoryRepository = iCategoryRepository;
            _iProductCategoryRepository = iProductCategoryRepository;
            _iWebHostEnvironment = iWebHostEnvironment;
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
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, int? categoryId, bool? status, string language = "vi", string ordertype = "asc", string orderby = "name")
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iProductRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                categoryId,
                m => (m.Name.Contains(key) || key == null || m.Content.Contains(key)) && m.Type == CategoryType.Product &&
                    (m.Language == language) &&
                    (m.Status == status || status == null) &&
                    !m.Delete,
                OrderByExtention(ordertype, orderby), x => new Product
                {
                    Category = x.Category,
                    Id = x.Id,
                    Banner = x.Banner,
                    Delete = x.Delete,
                    Name = x.Name,
                    Language = x.Language,
                    Price = x.Price,
                    Status = x.Status,
                    Type = x.Type,
                    Link = x.Link,
                    IsHome = x.IsHome,
                    CategoryId = x.CategoryId,
                    Content = x.Content
                });

            return View("IndexAjax", data);
        }
        private Func<IQueryable<Product>, IOrderedQueryable<Product>> OrderByExtention(string ordertype, string orderby) =>
            orderby switch
            {
                "name" => ordertype == "asc" ? EntityExtention<Product>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<Product>.OrderBy(m => m.OrderByDescending(x => x.Name)),
                _ => ordertype == "asc" ? EntityExtention<Product>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<Product>.OrderBy(m => m.OrderByDescending(x => x.Name))
            };
        #endregion

        #region [Create]
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Create(string language = "vi")
        {
            var dl = new ProductModel
            {
                Language = language
            };
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
            return View(dl);
        }

        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(ProductModel use, string categoryIds, string altId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _iProductRepository.BeginTransaction();
                    var data = new Product
                    {
                        Name = use.Name,
                        Banner = use.Banner,
                        Content = use.Content,
                        Delete = false,
                        Status = use.Status,
                        Language = use.Language,
                        Type = CategoryType.Product,
                        Specification = use.Specification,
                        Images = use.Images
                    };

                    await _iProductRepository.AddAsync(data);
                    await _iProductRepository.CommitAsync();

                    await AddSeoLink(CategoryType.Product, data.Language, data.Id, MapModel<SeoModel>.Go(use), data.Name, "", "ProductHome", "Details");
                    await UpdateCategory(data.Id, categoryIds);
                    await UpdateFileData(data.Id, CategoryType.Product, altId);
                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới sản phẩm \"{data.Name}\".",
                        Type = LogType.Create
                    });
                    await _iProductRepository.CommitTransaction();
                    return new ResponseModel() { Output = 1, Message = "Thêm mới sản phẩm thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
            var dl = await _iProductRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null || (dl != null && dl.Delete && dl.Type == CategoryType.Product))
            {
                return View("404");
            }
            var model = MapModel<ProductModel>.Go(dl);
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{dl.Language}" : "";
            var ktLink = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.ObjectId == id && x.Type == CategoryType.Product);
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
           
            model.CategoryIds = string.Join(",", (await _iProductCategoryRepository.SearchAsync(true, 0, 0, x => x.ProductId == id)).Select(x => x.CategoryId));
            model.PhotoFileDatas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FileDataModel>>(model.Images);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(ProductModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _iProductRepository.BeginTransaction();

                    var dl = await _iProductRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null || (dl != null && dl.Delete))
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }
                    dl.Name = use.Name;
                    dl.Banner = use.Banner;
                    dl.Content = use.Content;
                    dl.Status = use.Status;
                    dl.Specification = use.Specification;
                    dl.Images = use.Images;
                    _iProductRepository.Update(dl);
                    await _iProductRepository.CommitAsync();

                    await UpdateSeoLink(use.ChangeSlug, CategoryType.Product, dl.Id, dl.Language, MapModel<SeoModel>.Go(use), dl.Name, "", "ProductHome", "Details");

                    await UpdateCategory(id, use.CategoryIds);

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật sản phẩm \"{dl.Name}\".",
                        Type = LogType.Edit
                    });
                    await _iProductRepository.CommitTransaction();
                    return new ResponseModel() { Output = 1, Message = "Cập nhật sản phẩm thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin hoặc liên kết thân thiện/Permalink đã tồn tại, vui lòng thay thêm ký tự bất kỳ đằng sau.", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        private async Task UpdateCategory(int blogId, string categoryIds)
        {
            var list = new List<int>();
            if (categoryIds != null)
            {
                list = categoryIds.Split(',').Select(x => int.Parse(x)).ToList();
            }
            list = list.Distinct().ToList();
            _iProductCategoryRepository.DeleteWhere(x => x.ProductId == blogId);
            foreach (var item in list)
            {
                await _iProductCategoryRepository.AddAsync(new ProductCategory { ProductId = blogId, CategoryId = item });
            }
            await _iProductCategoryRepository.CommitAsync();
        }

        #region [Delete]
        [HttpPost, ActionName("Delete")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeletePost(int id)
        {
            try
            {
                await _iProductRepository.BeginTransaction();
                var kt = await _iProductRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null || (kt != null && kt.Delete && kt.Type == CategoryType.Product))
                {
                    return new ResponseModel() { Output = 0, Message = "sản phẩm không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                kt.Delete = true;
                await _iProductRepository.CommitAsync();

                await DeleteSeoLink(CategoryType.Product, kt.Id);
                await RemoveFileData(id, CategoryType.Product);
                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa sản phẩm \"{kt.Name}\".",
                    Type = LogType.Delete
                });
                await _iProductRepository.CommitTransaction();
                return new ResponseModel() { Output = 1, Message = "Xóa sản phẩm thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
        public async Task<List<TreeRoleModel>> TreeCategory(int id, string language = "vi")
        {
            var listCurent = await _iProductCategoryRepository.SearchAsync(true, 0, 0, x => x.ProductId == id);
            var listCategory = await _iCategoryRepository.SearchAsync(true, 0, 0, x => !x.Delete && x.Status && x.Type == CategoryType.CategoryProduct && x.Language == language);
            var abc = listCategory.Select(x =>
           new TreeRoleModel
           {
               Id = x.Id.ToString(),
               Parent = x.ParentId == 0 ? "#" : x.ParentId.ToString(),
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
            var listCategory = await _iCategoryRepository.SearchAsync(true, 0, 0, x => x.Language == language && x.Status && !x.Delete && x.Type == CategoryType.CategoryProduct);
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

        [HttpPost, Authorize]
        public async Task<List<SelectListItem>> SearchProduct(string q, int top = 10, string language = "vi")
        {
            top = top > 100 ? 100 : top;
            return (await _iProductRepository.SearchAsync(true, 0, top, x => x.Name.ToLower().Contains(q.ToLower()) && !x.Delete && x.Status && (x.Type == CategoryType.Product) && x.Language == language, x => x.OrderBy(y => y.Name),
                x => new Product { Id = x.Id, Name = x.Name, Delete = x.Delete, Status = x.Status, Language = x.Language, Type = x.Type })).Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
        }

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

                        await AddFileData(id, pathServer, CategoryType.Product, altId);

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
    }
}