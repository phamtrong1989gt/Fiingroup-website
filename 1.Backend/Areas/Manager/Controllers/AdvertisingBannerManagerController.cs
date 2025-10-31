using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;

namespace PT.BE.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class AdvertisingBannerManagerController : Base.Controllers.BaseController
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<AdvertisingBannerManagerController> _logger;
        private readonly IBannerRepository _bannerRepository;
        private readonly IBannerItemRepository _bannerItemRepository;
        private readonly IPortalRepository _portalRepository;

        private const string TokenUrl = "[Url]";
        private const string TokenTarget = "[Target]";
        private const string TokenName = "[Name]";
        private const string TokenImg = "[Img]";
        private const string TokenContent = "[Content]";

        public AdvertisingBannerManagerController(
            ILogger<AdvertisingBannerManagerController> logger,
            IWebHostEnvironment hostingEnvironment,
            IBannerRepository bannerRepository,
            IBannerItemRepository bannerItemRepository,
            IPortalRepository portalRepository)
        {
            controllerName = "AdvertisingBannerManager";
            tableName = "Banner";
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _bannerRepository = bannerRepository;
            _bannerItemRepository = bannerItemRepository;
            _portalRepository = portalRepository;
        }

        #region Index

        [AuthorizePermission]
        public async Task<IActionResult> Index()
        {
            var portals = await _portalRepository.SearchAsync(true, 0, 0);
            ViewData["PortalSelectList"] = new SelectList(portals, "Id", "Name");
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(
            int? id, int? page, int? limit, bool? status, string code, string key, string language = "vi",
            string ordertype = "asc", string orderby = "name", int? portalId = null)
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;

            var data = await _bannerRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                m =>
                    (string.IsNullOrEmpty(key) || m.Name.Contains(key)) &&
                    m.Language == language &&
                    (m.PortalId == portalId || portalId == null) &&
                    m.Type == BannerType.Advertising &&
                    !m.Delete &&
                    (id == null || m.Id == id) &&
                    (code == null || m.Code == code) &&
                    (status == null || m.Status == status),
                GetOrderBy(orderby, ordertype)
            );

            var portals = await _portalRepository.SearchAsync(true, 0, 0);
            foreach (var item in data.Data)
            {
                item.Portal = portals.FirstOrDefault(p => p.Id == item.PortalId);
            }
            return View("IndexAjax", data);
        }

        private static Func<IQueryable<Banner>, IOrderedQueryable<Banner>> GetOrderBy(string orderby, string ordertype)
        {
            return orderby switch
            {
                "name" => ordertype == "asc"
                    ? EntityExtention<Banner>.OrderBy(q => q.OrderBy(x => x.Name))
                    : EntityExtention<Banner>.OrderBy(q => q.OrderByDescending(x => x.Name)),
                _ => ordertype == "asc"
                    ? EntityExtention<Banner>.OrderBy(q => q.OrderBy(x => x.Id))
                    : EntityExtention<Banner>.OrderBy(q => q.OrderByDescending(x => x.Id))
            };
        }

        #endregion

        #region Create

        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Create(int portalId,string language = "vi")
        {
            var model = new BannerModel { Language = language };
            var portals = await _portalRepository.SearchAsync(true, 0, 0);
            model.PortalSelectList = new SelectList(portals, "Id", "Name");
            model.PortalId = portalId;
            model.PortalName = portals.FirstOrDefault(x => x.Id == portalId)?.Name;
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(BannerModel model)
        {
            if (!ModelState.IsValid)
                return ResponseHepper.Warning("Bạn chưa nhập đầy đủ thông tin");

            try
            {
                var exists = await _bannerRepository.SingleOrDefaultAsync(
                    false, m => m.Code == model.Code && m.Language == model.Language && m.PortalId == model.PortalId && m.Type == BannerType.Advertising);

                if (exists != null)
                    return ResponseHepper.Warning("Mã Banner đã tồn tại, vui lòng kiểm tra lại.");

                var banner = new Banner
                {
                    Name = model.Name,
                    Delete = false,
                    Status = model.Status,
                    Language = model.Language,
                    Template = model.Template ?? TokenContent,
                    Type = BannerType.Advertising,
                    ClassActive = model.ClassActive,
                    Code = model.Code,
                    PortalId = model.PortalId
                };

                await _bannerRepository.AddAsync(banner);
                await _bannerRepository.CommitAsync();
                // Sinh nội dung và lưu vào trường Content để tái sử dụng
                banner.Content = await UpdateGroupBanner(banner);
                _bannerRepository.Update(banner);
                await _bannerRepository.CommitAsync();

                CommonFunctions.TriggerCacheModuleClear(
                    banner.Content,
                    ModuleType.AdvertisingBanner,
                    banner.Code,
                    banner.Language, banner.PortalId);

                await AddLog(new LogModel
                {
                    ObjectId = banner.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Thêm mới Banner \"{banner.Name}\".",
                    Type = LogType.Create
                });

                return ResponseHepper.Success("Thêm mới Banner thành công", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
                return ResponseHepper.Error();
            }
        }

        #endregion

        #region Edit

        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Edit(int id)
        {
            var banner = await _bannerRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (banner == null || banner.Delete)
                return View("404");

            var model = MapModel<BannerModel>.Go(banner);
            var portals = await _portalRepository.SearchAsync(true, 0, 0);
            model.PortalSelectList = new SelectList(portals, "Id", "Name");
            model.PortalName = portals.FirstOrDefault(x => x.Id == banner.PortalId)?.Name;
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(BannerModel model, int id)
        {
            if (!ModelState.IsValid)
                return ResponseHepper.Warning("Bạn chưa nhập đầy đủ thông tin.");

            try
            {
                var banner = await _bannerRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (banner == null || banner.Delete)
                    return ResponseHepper.Warning("Dữ liệu không tồn tại, vui lòng thử lại.");

                var exists = await _bannerRepository.SingleOrDefaultAsync(
                    false, m => m.Code == model.Code && m.Language == model.Language && m.PortalId == model.PortalId && m.Type == BannerType.Advertising && m.Id != id);

                if (exists != null)
                    return ResponseHepper.Warning("Mã Banner đã tồn tại, vui lòng kiểm tra lại.");

                banner.Code = model.Code;
                banner.Name = model.Name;
                banner.Status = model.Status;
                banner.Language = model.Language;
                banner.Template = model.Template;
                banner.ClassActive = model.ClassActive;
                banner.PortalId = model.PortalId;
                banner.Content = await UpdateGroupBanner(banner);
                _bannerRepository.Update(banner);
                await _bannerRepository.CommitAsync();


                CommonFunctions.TriggerCacheModuleClear(
                    banner.Content,
                    ModuleType.AdvertisingBanner,
                    banner.Code,
                    banner.Language, banner.PortalId);

                await AddLog(new LogModel
                {
                    ObjectId = banner.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Cập nhật Banner \"{banner.Name}\".",
                    Type = LogType.Edit
                });

                return ResponseHepper.Success("Cập nhật Banner thành công.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
                return ResponseHepper.Error();
            }
        }

        #endregion

        #region Delete

        [HttpPost, ActionName("Delete")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeletePost(int id)
        {
            try
            {
                var banner = await _bannerRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (banner == null || banner.Delete)
                    return ResponseHepper.Warning("Banner không tồn tại, vui lòng thử lại.");

                banner.Delete = true;
                await _bannerRepository.CommitAsync();

                CommonFunctions.TriggerCacheModuleClear(
                    null,
                    ModuleType.AdvertisingBanner,
                    banner.Code,
                    banner.Language, banner.PortalId);

                await AddLog(new LogModel
                {
                    ObjectId = banner.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa Banner \"{banner.Name}\".",
                    Type = LogType.Delete
                });

                return ResponseHepper.Success("Xóa Banner thành công.", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
                return ResponseHepper.Error();
            }
        }

        #endregion

        #region DeleteItem

        [HttpPost, ActionName("DeleteItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeleteItemPost(int id)
        {
            try
            {
                var item = await _bannerItemRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (item == null)
                    return ResponseHepper.Warning("Banner không tồn tại, vui lòng thử lại.");

                _bannerItemRepository.Delete(item);
                var parentBanner = await _bannerRepository.SingleOrDefaultAsync(true, x => x.Id == item.BannerId);
                if (parentBanner != null)
                {
                    parentBanner.Content = await UpdateGroupBanner(parentBanner);
                    _bannerRepository.Update(parentBanner);
                    await _bannerRepository.CommitAsync();
                    CommonFunctions.TriggerCacheModuleClear(parentBanner.Content, ModuleType.AdvertisingBanner, parentBanner.Code, parentBanner?.Language, parentBanner.PortalId);
                }
                await _bannerItemRepository.CommitAsync();

                await AddLog(new LogModel
                {
                    ObjectId = item.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa Banner item \"{item.Name}\".",
                    Type = LogType.Delete
                });

                return ResponseHepper.Success("Xóa Banner item thành công.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
                return ResponseHepper.Error();
            }
        }

        #endregion

        #region Tree/Items

        /// <summary>
        /// Convert tree data to flat list with order and parentId.
        /// </summary>
        private static List<DataSortModel> ConvertData(List<DataSortModel> list, int parentId = 0)
        {
            var order = 1;
            var newList = new List<DataSortModel>();
            foreach (var item in list)
            {
                item.ParentId = parentId;
                item.Order = order++;
                newList.Add(item);
                if (item.Children.Any())
                    newList.AddRange(ConvertData(item.Children, item.Id));
            }
            return newList;
        }

        /// <summary>
        /// Render tree structure as HTML.
        /// </summary>
        private string ShowTree(List<BannerItem> list, int parentId)
        {
            if (!list.Any())
                return "<span>Hiện tại chưa có dữ liệu</span>";

            var sb = new StringBuilder();
            sb.Append("<ol class=\"dd-list\">");
            foreach (var item in list.OrderBy(x => x.Order))
            {
                sb.Append($"<li class=\"dd-item {(item.Status ? "treeTrue" : "")} dd3-item\" data-id=\"{item.Id}\">");
                sb.Append("<div class=\"dd-handle dd3-handle\"></div>");
                sb.Append("<div class=\"dd3-content\">");
                sb.Append(item.Name);
                sb.Append($"<span class=\"button-icon\"><a button-static href='{Url.Action("EditItem", new { id = item.Id })}' title=\"Cập nhật\"><i class=\"material-icons iconcontrol text-primary\">edit</i></a></span>");
                sb.Append("</div>");
                sb.Append("</li>");
            }
            sb.Append("</ol>");
            return sb.ToString();
        }

        [HttpGet]
        public async Task<string> Items(int id)
        {
            var list = await _bannerItemRepository.SearchAsync(true, 0, 0, x => x.BannerId == id);
            return ShowTree(list, 0);
        }

        #endregion

        #region CreateItem

        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult CreateItem(int BannerId = 0) =>
            View(new BannerItemModel { BannerId = BannerId });

        [HttpPost, ActionName("CreateItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreateItemPost(BannerItemModel model)
        {
            if (!ModelState.IsValid)
                return ResponseHepper.Warning("Bạn chưa nhập đầy đủ thông tin");

            try
            {
                var maxOrder = _bannerItemRepository.MaxOrder(x => x.BannerId == model.BannerId);
                var item = new BannerItem
                {
                    Name = model.Name,
                    Status = model.Status,
                    Order = maxOrder + 1,
                    Href = model.Href,
                    BannerId = model.BannerId,
                    Banner = model.Banner,
                    Template = model.Template,
                    Target = model.Target
                };

                await _bannerItemRepository.AddAsync(item);
                await _bannerItemRepository.CommitAsync();

                var parentBanner = await _bannerRepository.SingleOrDefaultAsync(true, x => x.Id == item.BannerId);

                if (parentBanner != null)
                {
                    var content = await UpdateGroupBanner(parentBanner);
                    parentBanner.Content = content;
                    _bannerRepository.Update(parentBanner);
                    await _bannerRepository.CommitAsync();
                    CommonFunctions.TriggerCacheModuleClear(content, ModuleType.AdvertisingBanner, parentBanner.Code, parentBanner?.Language, parentBanner.PortalId);
                }

                await AddLog(new LogModel
                {
                    ObjectId = item.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Thêm Banner item \"{item.Name}\".",
                    Type = LogType.Create
                });

                return ResponseHepper.Success("Thêm mới Banner item thành công", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
                return ResponseHepper.Error();
            }
        }

        #endregion

        #region EditItem

        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> EditItem(int id)
        {
            var item = await _bannerItemRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (item == null)
                return View("404");

            var model = MapModel<BannerItemModel>.Go(item);
            return View(model);
        }

        [HttpPost, ActionName("EditItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditItemPost(BannerItemModel model, int id)
        {
            if (!ModelState.IsValid)
                return ResponseHepper.Warning("Bạn chưa nhập đầy đủ thông tin.");

            try
            {
                var item = await _bannerItemRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (item == null)
                    return ResponseHepper.Warning("Dữ liệu không tồn tại, vui lòng thử lại.");

                item.Name = model.Name;
                item.Status = model.Status;
                item.Href = model.Href;
                item.Template = model.Template;
                item.Target = model.Target;
                item.Banner = model.Banner;

                _bannerItemRepository.Update(item);
                await _bannerItemRepository.CommitAsync();

                var parentBanner = await _bannerRepository.SingleOrDefaultAsync(true, x => x.Id == item.BannerId);

                CommonFunctions.GenModule(
                    _hostingEnvironment.WebRootPath,
                    await UpdateGroupBanner(parentBanner),
                    ModuleType.AdvertisingBanner,
                    parentBanner.Code,
                    parentBanner?.Language);

                await AddLog(new LogModel
                {
                    ObjectId = item.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Cập nhật Banner item \"{item.Name}\".",
                    Type = LogType.Edit
                });

                return ResponseHepper.Success("Cập nhật Banner thành công.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
                return ResponseHepper.Error();
            }
        }

        [HttpPost, ActionName("UpdateOrder")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> UpdateOrderPost([FromBody] string data, int id)
        {
            try
            {
                var listItem = ConvertData(Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataSortModel>>(data));
                var listItemIds = listItem.Select(x => x.Id).ToList();
                var items = await _bannerItemRepository.SearchAsync(false, 0, 0, x => listItemIds.Contains(x.Id));
                foreach (var item in items)
                {
                    var objIn = listItem.FirstOrDefault(x => x.Id == item.Id);
                    if (objIn != null)
                        item.Order = objIn.Order;
                    _bannerItemRepository.Update(item);
                }
                await _bannerItemRepository.CommitAsync();

                await AddLog(new LogModel
                {
                    ObjectId = id,
                    ActionTime = DateTime.Now,
                    Name = $"Cập nhật thứ tự banner quảng cáo #\"{id}\".",
                    Type = LogType.Edit
                });

                var parentBanner = await _bannerRepository.SingleOrDefaultAsync(true, x => x.Id == id);
                CommonFunctions.GenModule(
                    _hostingEnvironment.WebRootPath,
                    await UpdateGroupBanner(parentBanner),
                    ModuleType.AdvertisingBanner,
                    parentBanner.Code,
                    parentBanner?.Language);

                return ResponseHepper.Success("Cập nhật thành công.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
                return ResponseHepper.Error();
            }
        }

        #endregion

        /// <summary>
        /// Generate HTML content for group banner.
        /// </summary>
        private async Task<string> UpdateGroupBanner(Banner group)
        {
            var items = await _bannerItemRepository.SearchAsync(true, 0, 0, x => x.Status && x.BannerId == group.Id);
            var sb = new StringBuilder();
            foreach (var item in items.OrderBy(x => x.Order))
            {
                if (!string.IsNullOrEmpty(item.Template))
                {
                    sb.Append(item.Template
                        .Replace(TokenUrl, item.Href)
                        .Replace(TokenTarget, item.Target)
                        .Replace(TokenImg, item.Banner)
                        .Replace(TokenName, item.Name));
                }
            }
            var output = (group.Template ?? string.Empty).Replace(TokenContent, sb.ToString());
            group.Content = output;
            return output;
        }
    }
}