using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Infrastructure.Repositories;
using PT.Shared;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PT.UI.Areas.Manager.Controllers
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

        public TagManagerController(
            ILogger<TagManagerController> logger,
            IOptions<BaseSettings> baseSettings,
            ILinkRepository linkRepository,
            ITagRepository tagRepository,
            IWebHostEnvironment webHostEnvironment,
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
        }

        #region [Index]
        [AuthorizePermission]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(
            int? page,
            int? limit,
            string key,
            bool? status,
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
                    && !m.Delete,
                OrderByExtension(ordertype, orderby));

            data.ReturnUrl = Url.Action("Index", new { page, limit, key, status, ordertype, orderby });
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
        public IActionResult Create(string language = "vi")
        {
            var model = new TagModel
            {
                Language = language,
                Type = CategoryType.Tag
            };
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
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
                    var tag = new Tag
                    {
                        Name = model.Name,
                        Delete = false,
                        Banner = model.Banner,
                        Content = model.Content,
                        Status = model.Status,
                        Language = model.Language
                    };
                    await _tagRepository.AddAsync(tag);
                    await _tagRepository.CommitAsync();

                    await AddSeoLink(CategoryType.Tag, tag.Language, tag.Id, MapModel<SeoModel>.Go(model), tag.Name, "", "TagHome", "Details");
                    await UpdateFileData(tag.Id, CategoryType.Tag, altId);
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
                    Message = "Bạn chưa nhập đầy đủ thông tin",
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
            if (tag == null || tag.Delete)
            {
                return View("404");
            }
            var model = MapModel<TagModel>.Go(tag);
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{tag.Language}" : "";
            var link = await _linkRepository.SingleOrDefaultAsync(true, x => x.ObjectId == id && x.Type == CategoryType.Tag);
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
            }
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
                    var tag = await _tagRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (tag == null || tag.Delete)
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
                    _tagRepository.Update(tag);
                    await _tagRepository.CommitAsync();

                    await UpdateSeoLink(model.ChangeSlug, CategoryType.Tag, tag.Id, tag.Language, MapModel<SeoModel>.Go(model), tag.Name, "", "TagHome", "Details");

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
                if (tag == null || tag.Delete)
                {
                    return new ResponseModel
                    {
                        Output = 0,
                        Message = "Tag không tồn tại, vui lòng thử lại.",
                        Type = ResponseTypeMessage.Warning
                    };
                }
                tag.Delete = true;
                await _tagRepository.CommitAsync();
                await DeleteSeoLink(CategoryType.Tag, tag.Id);
                await RemoveFileData(id, CategoryType.Tag);

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
        [HttpPost, ActionName("UploadImage")]
        [AuthorizePermission("Index")]
        public async Task<object> UploadImagePost(string altId, int id, int type = 0)
        {
            try
            {
                string[] allowedExtensions = _baseSettings.Value.ImagesType.Split(',');
                string folder = Functions.GenFolderByDate();
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "Data", folder);
                string pathServer = $"/Data{folder}";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var files = Request.Form.Files;
                foreach (var file in files)
                {
                    string fileExt = Path.GetExtension(file.FileName);
                    if (!allowedExtensions.Contains(fileExt))
                    {
                        return new ResponseModel<FileDataModel>
                        {
                            Output = 2,
                            Message = "Tệp tải lên không đúng định dạng.",
                            Type = ResponseTypeMessage.Warning
                        };
                    }
                    if (_baseSettings.Value.ImagesMaxSize < file.Length)
                    {
                        return new ResponseModel<FileDataModel>
                        {
                            Output = 3,
                            Message = "Tệp tải lên vượt quá kích thước cho phép.",
                            Type = ResponseTypeMessage.Warning
                        };
                    }

                    var newFilename = Path.GetFileName(file.FileName);
                    if (System.IO.File.Exists(Path.Combine(path, file.Name)))
                    {
                        newFilename = $"{Path.GetFileName(file.FileName)}_{id}_{DateTime.Now:yyyyMMddHHmmss}";
                    }

                    string pathFile = Path.Combine(path, newFilename);
                    string pathServerFile = $"{pathServer}{newFilename}";

                    using var image = System.Drawing.Image.FromStream(file.OpenReadStream());
                    if (image.Width > _baseSettings.Value.ImageMaxWith)
                    {
                        _fileRepository.ResizeImage(file, pathFile, _baseSettings.Value.ImageMaxWith, false);
                    }
                    else
                    {
                        using var stream = new FileStream(pathFile, FileMode.Create);
                        await file.CopyToAsync(stream);
                    }

                    await AddFileData(id, pathServerFile, CategoryType.Tag, altId);

                    if (type == 1)
                    {
                        return new FileDataCKEditerModel
                        {
                            FileName = Path.GetFileName(pathServerFile),
                            Number = 200,
                            Uploaded = 1,
                            Url = pathServerFile
                        };
                    }
                    else
                    {
                        return new ResponseModel<FileDataModel>
                        {
                            Output = 1,
                            Message = "Tải tệp lên thành công.",
                            Type = ResponseTypeMessage.Success,
                            Data = new FileDataModel
                            {
                                CreatedDate = DateTime.Now,
                                CreatedUser = DataUserInfo.UserId,
                                Path = pathServerFile,
                                FileName = Path.GetFileName(pathServerFile)
                            },
                            IsClosePopup = false
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel<FileDataModel>
            {
                Output = -1,
                Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.",
                Type = ResponseTypeMessage.Danger,
                Status = false
            };
        }
        #endregion
    }
}