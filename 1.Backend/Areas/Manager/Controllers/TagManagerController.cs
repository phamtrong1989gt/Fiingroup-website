using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Infrastructure.Repositories;
using PT.Shared;
using System;
using System.ClientModel.Primitives;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PT.BE.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class TagManagerController : Base.Controllers.BaseController
    {
        private readonly ILogger<TagManagerController> _logger;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly ILinkRepository _linkRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IFileRepository _fileRepository;
        private readonly IPortalRepository _iPortalRepository;
        private readonly IContentPageTagRepository _iContentPageTagRepository;
        public TagManagerController(
            ILogger<TagManagerController> logger,
            IOptions<BaseSettings> baseSettings,
            ILinkRepository linkRepository,
            ITagRepository tagRepository,
            IWebHostEnvironment webHostEnvironment,
            IPortalRepository iPortalRepository,
            IContentPageTagRepository contentPageTagRepository,
            IFileRepository fileRepository)
        {
            controllerName = "TagManager";
            tableName = "Tag";
            _logger = logger;
            _baseSettings = baseSettings;
            _linkRepository = linkRepository;
            _tagRepository = tagRepository;
            _webHostEnvironment = webHostEnvironment;
            _fileRepository = fileRepository;
            _iPortalRepository = iPortalRepository;
            _iContentPageTagRepository = contentPageTagRepository;
        }

        #region [Index]
        [AuthorizePermission]
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách portal để hiển thị bộ lọc portal trên giao diện quản trị
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            ViewBag.PortalSelectList = new SelectList(portals, "Id", "Name");
            // Trả về view mặc định cho quản lý slide ảnh
            return View();
        }

        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(
            int? page,
            int? limit,
            string key,
            bool? status,
            int? portalId,
            string language = "vi",
            string ordertype = "asc",
            string orderby = "name")
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;

            var data = await _tagRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                m => (string.IsNullOrEmpty(key) || m.Name.Contains(key))
                    && m.Language == language
                    && (m.Status == status || status == null)
                    && (m.PortalId == portalId || portalId == null)
                   ,
                OrderByExtension(ordertype, orderby));

            var portals = await _iPortalRepository.SearchAsync(true,0,0);
            foreach (var item in data.Data)
            {
                item.Portal = portals.FirstOrDefault(p => p.Id == item.PortalId);
                // Use repository helper that accepts existing portals list to avoid extra DB queries
                item.FullPath = await _iPortalRepository.GetFullPathAsync(item.PortalId, item.Link?.Slug ?? string.Empty, portals, item.Language, _baseSettings.Value.MultipleLanguage);
            }
            return View("IndexAjax", data);
        }

        private Func<IQueryable<Tag>, IOrderedQueryable<Tag>> OrderByExtension(string ordertype, string orderby)
        {
            return orderby switch
            {
                "name" => ordertype == "asc"
                    ? EntityExtention<Tag>.OrderBy(m => m.OrderBy(x => x.Name))
                    : EntityExtention<Tag>.OrderBy(m => m.OrderByDescending(x => x.Name)),
                _ => ordertype == "asc"
                    ? EntityExtention<Tag>.OrderBy(m => m.OrderBy(x => x.Id))
                    : EntityExtention<Tag>.OrderBy(m => m.OrderByDescending(x => x.Id))
            };
        }
        #endregion

        #region [Create]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Create(int portalId, string language = "vi")
        {
            var model = new TagModel
            {
                Language = language,
                SlugType = CategoryType.Tag
            };
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            model.PortalSelectList = new SelectList(portals, "Id", "Name");
            model.PortalName = portals.FirstOrDefault(x => x.Id == portalId)?.Name;
            model.PortalId = portalId;
            model.PrefixSlug = "tags";
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(TagModel model, string altId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _tagRepository.BeginTransaction();
                    // Kiểm tra trùng tên tag (Create)
                    var normalizedName = (model.Name ?? string.Empty).Trim().ToLowerInvariant();
                    var portalIdToCheck = model.PortalId ?? 0;
                    var exists = await _tagRepository.AnyAsync(t =>
                        t.PortalId == portalIdToCheck
                        && t.Name != null
                        && t.Name.ToLower() == normalizedName);
                    if (exists)
                    {
                        return new ResponseModel
                        {
                            Output = 0,
                            Message = "Tên tag đã tồn tại, vui lòng chọn tên khác.",
                            Type = ResponseTypeMessage.Warning
                        };
                    }
                    var tag = new Tag
                    {
                        Name = model.Name,
                        Banner = model.Banner,
                        Content = model.Content,
                        Status = model.Status,
                        Language = model.Language,
                        PortalId = model.PortalId ?? 0,
                        SlugType = ESlugType.Tag
                    };
                    await _tagRepository.AddAsync(tag);
                    await _tagRepository.CommitAsync();

                    await CreateLinkAsync(ESlugType.Tag, tag.Language, tag.Id, MapModel<SeoModel>.Go(model), tag.Name, "", "TagHome", "Details", tag.PortalId);
                    await UpdateFileData(tag.Id, ESlugType.Tag, altId);
                    await AddLog(new LogModel
                    {
                        ObjectId = tag.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới tag \"{tag.Name}\".",
                        Type = LogType.Create
                    });
                    await _tagRepository.CommitTransaction();
                    return new ResponseModel
                    {
                        Output = 1,
                        Message = "Thêm mới tag thành công",
                        Type = ResponseTypeMessage.Success,
                        IsClosePopup = true
                    };
                }
                return new ResponseModel
                {
                    Output = 0,
                    Message = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()),
                    Type = ResponseTypeMessage.Warning
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel
            {
                Output = -1,
                Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại",
                Type = ResponseTypeMessage.Danger,
                Status = false
            };
        }
        #endregion

        #region [Edit]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Edit(int id)
        {
            var tag = await _tagRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (tag == null)
            {
                return View("404");
            }
            var model = MapModel<TagModel>.Go(tag);
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{tag.Language}" : "";
            var link = await _linkRepository.SingleOrDefaultAsync(true, x => x.ObjectId == id && x.Type == ESlugType.Tag);
            if (link != null)
            {
                model.Changefreq = link.Changefreq;
                model.Lastmod = link.Lastmod;
                model.Priority = link.Priority.ConvertToString();
                model.Description = link.Description;
                model.FacebookBanner = link.FacebookBanner;
                model.FacebookDescription = link.FacebookDescription;
                model.FocusKeywords = link.FocusKeywords;
                model.GooglePlusDescription = link.GooglePlusDescription;
                model.IncludeSitemap = link.IncludeSitemap;
                model.Keywords = link.Keywords;
                model.MetaRobotsAdvance = link.MetaRobotsAdvance;
                model.MetaRobotsFollow = link.MetaRobotsFollow;
                model.MetaRobotsIndex = link.MetaRobotsIndex;
                model.Redirect301 = link.Redirect301;
                model.Title = link.Title;
                model.LinkId = link.Id;
                model.Slug = link.Slug;
                model.Type = link.Type;
                model.PortalId = link.PortalId;
                model.PrefixSlug = "tags";
            }
            model.PortalId = tag.PortalId;
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            model.PortalSelectList = new SelectList(portals, "Id", "Name");
            model.PortalName = portals.FirstOrDefault(x => x.Id == model.PortalId)?.Name;
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(TagModel model, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _tagRepository.BeginTransaction();
                    // Kiểm tra trùng tên tag (Edit) - exclude bản ghi hiện tại
                    var normalizedName = (model.Name ?? string.Empty).Trim().ToLowerInvariant();
                    var portalIdToCheck = model.PortalId ?? 0;
                    var exists = await _tagRepository.AnyAsync(t =>
                        t.PortalId == portalIdToCheck
                        && t.Id != id
                        && t.Name != null
                        && t.Name.ToLower() == normalizedName);

                    if (exists)
                    {
                        return new ResponseModel
                        {
                            Output = 0,
                            Message = "Tên tag đã tồn tại trên cổng này, vui lòng chọn tên khác.",
                            Type = ResponseTypeMessage.Warning
                        };
                    }

                    var tag = await _tagRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (tag == null)
                    {
                        return new ResponseModel
                        {
                            Output = 0,
                            Message = "Dữ liệu không tồn tại, vui lòng thử lại.",
                            Type = ResponseTypeMessage.Warning
                        };
                    }
                    tag.Name = model.Name;
                    tag.Status = model.Status;
                    tag.Banner = model.Banner;
                    tag.Content = model.Content;
                    tag.PortalId = model.PortalId ?? 0;
                    _tagRepository.Update(tag);
                    await _tagRepository.CommitAsync();

                    await UpdateLinkAsync(model.ChangeSlug, ESlugType.Tag, tag.Id, tag.Language, MapModel<SeoModel>.Go(model), tag.Name, "", "TagHome", "Details");

                    await AddLog(new LogModel
                    {
                        ObjectId = tag.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật tag \"{tag.Name}\".",
                        Type = LogType.Edit
                    });
                    await _tagRepository.CommitTransaction();
                    return new ResponseModel
                    {
                        Output = 1,
                        Message = "Cập nhật tag thành công.",
                        Type = ResponseTypeMessage.Success,
                        IsClosePopup = true
                    };
                }
                return new ResponseModel
                {
                    Output = -2,
                    Message = "Bạn chưa nhập đầy đủ thông tin.",
                    Type = ResponseTypeMessage.Warning
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel
            {
                Output = -1,
                Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.",
                Type = ResponseTypeMessage.Danger,
                Status = false
            };
        }
        #endregion

        #region [Delete]
        [HttpPost, ActionName("Delete")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeletePost(int id)
        {
            try
            {
                await _tagRepository.BeginTransaction();
                var tag = await _tagRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (tag == null)
                {
                    return new ResponseModel
                    {
                        Output = 0,
                        Message = "Tag không tồn tại, vui lòng thử lại.",
                        Type = ResponseTypeMessage.Warning
                    };
                }
                _tagRepository.Delete(tag);
                await _tagRepository.CommitAsync();
                // Xóa hết bảng liên quan
                _iContentPageTagRepository.DeleteWhere(x=>x.TagId == id);
                await _iContentPageTagRepository.CommitAsync();
                await DeleteSeoLink(ESlugType.Tag, tag.Id);
                await RemoveFileData(id, tag.SlugType ?? ESlugType.Tag);
                await AddLog(new LogModel
                {
                    ObjectId = tag.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa tag \"{tag.Name}\".",
                    Type = LogType.Delete
                });

                await _tagRepository.CommitTransaction();

                return new ResponseModel
                {
                    Output = 1,
                    Message = "Xóa tag thành công.",
                    Type = ResponseTypeMessage.Success,
                    IsClosePopup = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel
            {
                Output = -1,
                Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.",
                Type = ResponseTypeMessage.Danger,
                Status = false
            };
        }
        #endregion

        #region [Upload file]
        /// <summary>
        /// Upload image cho category: validate extension, kích thước, resize nếu cần và ghi file metadata.
        /// - Trả về ResponseModel hoặc CKEditor response tuỳ type.
        /// </summary>
        [HttpPost, ActionName("UploadImage")]
        [AuthorizePermission("Index")]
        public async Task<object> UploadImagePost(string altId, int id, int type = 0)
        {
            try
            {
                var allowed = (_baseSettings.Value.ImagesType ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

                var folderByDate = Functions.GenFolderByDate();
                string configuredDataPath = _baseSettings.Value.DataPath;

                // Lấy đường dẫn vật lý và public base url, helper đã tạo và đảm bảo thư mục tồn tại
                var (physicalPath, publicUrlBase) = Functions.SetupSharedDataFolder(configuredDataPath, folderByDate);

                var file = Request.Form.Files.FirstOrDefault();
                if (file == null)
                    return new ResponseModel<FileDataModel> { Output = 0, Message = "Không có tệp được gửi.", Type = ResponseTypeMessage.Warning };

                var ext = Path.GetExtension(file.FileName);
                if (!allowed.Contains(ext))
                    return new ResponseModel<FileDataModel> { Output = 2, Message = "Tệp tải lên không đúng định dạng.", Type = ResponseTypeMessage.Warning };

                if (_baseSettings.Value.ImagesMaxSize < file.Length)
                    return new ResponseModel<FileDataModel> { Output = 3, Message = "Tệp tải lên vượt quá kích thước cho phép.", Type = ResponseTypeMessage.Warning };

                var safeName = Path.GetFileNameWithoutExtension(file.FileName);
                var fileName = safeName + ext;
                var fullPath = Path.Combine(physicalPath, fileName);
                if (System.IO.File.Exists(fullPath))
                {
                    fileName = $"{safeName}_{id}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                    fullPath = Path.Combine(physicalPath, fileName);
                }

                // Lưu file: resize nếu cần, else copy
                try
                {
                    using var img = System.Drawing.Image.FromStream(file.OpenReadStream());
                    if ((img.Width > _baseSettings.Value.ImageMaxWith) || (img.Height > _baseSettings.Value.ImageMaxWith))
                    {
                        _fileRepository.ResizeImage(file, fullPath, _baseSettings.Value.ImageMaxWith, false);
                    }
                    else
                    {
                        using var fs = new FileStream(fullPath, FileMode.Create);
                        // reset stream position
                        file.OpenReadStream().CopyTo(fs);
                    }
                }
                catch
                {
                    // Nếu không thể load image thì ghi trực tiếp
                    using var fs = new FileStream(fullPath, FileMode.Create);
                    await file.CopyToAsync(fs);
                }

                var publicUrl = (publicUrlBase ?? "/Data/").Replace("\\", "/");
                if (!publicUrl.EndsWith("/")) publicUrl += "/";
                publicUrl = publicUrl + Uri.EscapeDataString(fileName);

                await AddFileData(id, publicUrl, ESlugType.ContentPage, altId);

                if (type == 1)
                {
                    return new FileDataCKEditerModel
                    {
                        FileName = fileName,
                        Number = 200,
                        Uploaded = 1,
                        Url = publicUrl
                    };
                }

                return new ResponseModel<FileDataModel>
                {
                    Output = 1,
                    Message = "Tải tệp lên thành công.",
                    Type = ResponseTypeMessage.Success,
                    Data = new FileDataModel
                    {
                        CreatedDate = DateTime.Now,
                        CreatedUser = DataUserInfo.UserId,
                        Path = publicUrl,
                        FileName = fileName
                    },
                    IsClosePopup = false
                };
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