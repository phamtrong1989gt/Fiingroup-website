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
using System.Text;
using PT.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PT.BE.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class MenuManagerController : Base.Controllers.BaseController
    {
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly ILogger _logger;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IMenuRepository _iMenuRepository;
        private readonly IMenuItemRepository _iMenuItemRepository;
        private readonly ILinkRepository _iLinkRepository;

        public MenuManagerController(
            ILogger<MenuManagerController> logger,
            IOptions<BaseSettings> baseSettings,
            IWebHostEnvironment iHostingEnvironment,
            IMenuRepository iMenuRepository,
            IMenuItemRepository iMenuItemRepository,
            ILinkRepository iLinkRepository
        )
        {
            controllerName = "MenuManager";
            tableName = "Menu";
            _logger = logger;
            _baseSettings = baseSettings;
            _iHostingEnvironment = iHostingEnvironment;
            _iMenuRepository = iMenuRepository;
            _iMenuItemRepository = iMenuItemRepository;
            _iLinkRepository = iLinkRepository;
        }

        #region [Index]
        [AuthorizePermission("Index")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> IndexPost(int? id, int? page, int? limit, string key, string code, string language = "vi", string ordertype = "asc", string orderby = "name")
        {

            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iMenuRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                    m => (m.Name.Contains(key) || key == null) 
                    && (m.Code == code || code == null)
                    && (m.Language == language) 
                    && !m.Delete
                    && (m.Id==id || id==null)
                    ,
                OrderByExtention(ordertype, orderby));
            return View("IndexAjax", data);
        }
        private Func<IQueryable<Menu>, IOrderedQueryable<Menu>> OrderByExtention(string ordertype, string orderby)
        {
            Func<IQueryable<Menu>, IOrderedQueryable<Menu>> functionOrder = null;
            switch (orderby)
            {
                case "name":
                    functionOrder = ordertype == "asc" ? EntityExtention<Menu>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<Menu>.OrderBy(m => m.OrderByDescending(x => x.Name));
                    break;
                default:
                    functionOrder = ordertype == "asc" ? EntityExtention<Menu>.OrderBy(m => m.OrderBy(x => x.Id)) : EntityExtention<Menu>.OrderBy(m => m.OrderByDescending(x => x.Id));
                    break;
            }
            return functionOrder;
        }
        #endregion

        #region [Create]
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Create(string language = "vi")
        {
            var dl = new MenuModel
            {
                Language = language
            };
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
            return View(dl);
        }
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(MenuModel use)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var ktCode = await _iMenuRepository.SingleOrDefaultAsync(true, m => m.Code == use.Code);
                    if (ktCode != null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Mã menu đã tồn tại, vui lòng chọn mã khác.", Type = ResponseTypeMessage.Warning };
                    }
                    var data = new Menu
                    {
                        Name = use.Name,
                        Delete = false,
                        Status = use.Status,
                        Language = use.Language,
                        HasChildrentClass1 = use.HasChildrentClass1,
                        HasChildrentClass2 = use.HasChildrentClass2,
                        HasChildrentClass3 = use.HasChildrentClass3,
                        Template = use.Template,
                        Template1 = use.Template1,
                        Template2 = use.Template2,
                        Template3 = use.Template3,
                        Code = use.Code
                    };
                    await _iMenuRepository.AddAsync(data);
                    await _iMenuRepository.CommitAsync();
                     
                    CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupMenu(data), ModuleType.Menu, data.Code, data.Language);

                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới menu \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới menu thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
            var dl = await _iMenuRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null || (dl != null && dl.Delete))
            {
                return View("404");
            }
            var model = MapModel<MenuModel>.Go(dl);
            return View(model);
        }
        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(MenuModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iMenuRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null || (dl != null && dl.Delete))
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    var ktCode = await _iMenuRepository.SingleOrDefaultAsync(true, m => m.Code == use.Code && m.Id != dl.Id);
                    if (ktCode != null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Mã menu đã tồn tại, vui lòng chọn mã khác.", Type = ResponseTypeMessage.Warning };
                    }

                    dl.Name = use.Name;
                    dl.Status = use.Status;
                    dl.Language = use.Language;
                    dl.HasChildrentClass1 = use.HasChildrentClass1;
                    dl.HasChildrentClass2 = use.HasChildrentClass2;
                    dl.HasChildrentClass3 = use.HasChildrentClass3;
                    dl.Template = use.Template;
                    dl.Template1 = use.Template1;
                    dl.Template2 = use.Template2;
                    dl.Template3 = use.Template3;
                    dl.Code = use.Code;
                    _iMenuRepository.Update(dl);
                    await _iMenuRepository.CommitAsync();

                    CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupMenu(dl), ModuleType.Menu, dl.Code, dl.Language);

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật menu \"{dl.Name}\".",
                        Type = LogType.Edit
                    });
                    return new ResponseModel() { Output = 1, Message = "Cập nhật menu thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = false };
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
                var kt = await _iMenuRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null || (kt != null && kt.Delete))
                {
                    return new ResponseModel() { Output = 0, Message = "menu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                kt.Delete = true;
                await _iMenuRepository.CommitAsync();

                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa menu \"{kt.Name}\".",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa menu thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [DeleteItem]
        [HttpPost, ActionName("DeleteItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeleteItemPost(int id)
        {
            try
            {
                var kt = await _iMenuItemRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null)
                {
                    return new ResponseModel() { Output = 0, Message = "menu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                _iMenuItemRepository.Delete(kt);
                await _iMenuItemRepository.CommitAsync();

                var dataParent = await _iMenuRepository.SingleOrDefaultAsync(true, x => x.Id == kt.MenuId);
                CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupMenu(dataParent), ModuleType.Menu, dataParent.Code, dataParent?.Language);

                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa menu item \"{kt.Name}\".",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa menu item thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [ABC]
        private List<DataSortModel> ConverData(List<DataSortModel> list, int parentId = 0)
        {
            int order = 1;
            var newList = new List<DataSortModel>();
            foreach (var item in list)
            {
                item.ParentId = parentId;
                item.Order = order;
                newList.Add(item);
                if (item.Children.Count() > 0)
                {
                    newList.AddRange(ConverData(item.Children, item.Id));
                }
                order++;
            }
            return newList;
        }

        private string ShowTree(List<MenuItem> list, int parrentId)
        {
            if (list.Count() == 0)
            {
                return "<span>Hiện tại chưa có dữ liệu</span>";
            }
            var listCulture = Shared.ListData.ListLanguage;
            System.Text.StringBuilder str = new System.Text.StringBuilder();
            var listitem = list.Where(m => m.ParentId == parrentId).OrderBy(m => m.Order);
            if (listitem.Count() > 0)
            {
                if (parrentId == 0)
                {
                    str.Append($"<ol class=\"dd-list\">");
                }
                else
                {
                    str.Append($"<ol class=\"dd-list\">");
                }
                foreach (var item in listitem)
                {
                    if (item.Id == parrentId)
                    {
                        return "";
                    }
                    str.Append($"<li class=\"dd-item {(item.Status ? "treeTrue" : "")} dd3-item\" data-id=\"{item.Id}\">");
                    str.Append($"<div class=\"dd-handle dd3-handle\"></div>");
                    str.Append($"<div class=\"dd3-content\">");
                    str.Append($"{item.Name}");
                    str.Append($"<span class=\"button-icon\"><a button-static href='{Url.Action("EditItem", new { id = item.Id })}'  title=\"Cập nhật\"><i class=\"material-icons iconcontrol text-primary\">edit</i></a><span>");
                    str.Append("</div>");
                    str.Append(ShowTree(list, item.Id));
                    str.Append("</li>");
                }
                str.Append($"</ol>");
            }
            return str.ToString();
        }

        [HttpGet]
        public async Task<string> Items(int menuId)
        {
            var list = await _iMenuItemRepository.SearchAsync(true, 0, 0, x => x.MenuId == menuId);
            return ShowTree(list, 0);
        }
        #endregion

        #region [CreateItem]
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult CreateItem(int menuId = 0)
        {
            var dl = new MenuItemModel
            {
                MenuId = menuId
            };
            return View(dl);
        }
        [HttpPost, ActionName("CreateItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreateItemPost(MenuItemModel use)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var maxOrder = _iMenuItemRepository.MaxOrder(x => x.ParentId == 0);
                    var data = new MenuItem
                    {
                        Name = use.Name,
                        Status = use.Status,
                        Order = maxOrder + 1,
                        ParentId = 0,
                        Href = use.Href,
                        MenuId = use.MenuId,
                        Class = use.Class,
                        Icon = use.Icon,
                        Target = use.Target,
                        CategoryType = use.CategoryType ?? CategoryType.Tour,
                        IsLinkLocal = use.IsLinkLocal,
                        Language = use.Language,
                        LinkId = use.LinkId
                    };

                    if (data.IsLinkLocal)
                    {
                        var dlLink = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.Id == data.LinkId);
                        data.Language = dlLink?.Language;
                        data.Href = Functions.FormatUrl(dlLink?.Language, dlLink?.Slug);
                    }

                    await _iMenuItemRepository.AddAsync(data);
                    await _iMenuItemRepository.CommitAsync();

                   
                    var dataParent = await _iMenuRepository.SingleOrDefaultAsync(true, x => x.Id == data.MenuId);
                    CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupMenu(dataParent), ModuleType.Menu, dataParent.Code, dataParent?.Language);

                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm menu item \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới menu item thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = false };
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

        #region [EditItem]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> EditItem(int id)
        {
            var dl = await _iMenuItemRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null)
            {
                return View("404");
            }
            var model = MapModel<MenuItemModel>.Go(dl);
            if(model.IsLinkLocal)
            {
                var dataLink = await _iLinkRepository.SearchAsync(true, 0, 1, x => x.Id == model.LinkId, null, x=> new Link { Id = x.Id , Name = $"({x.Language}|{x.Type.GetDisplayName()}) / {x.Name}" });
                model.LinkSelectList = new SelectList(dataLink, "Id", "Name");
            }    
            return View(model);
        }
        [HttpPost, ActionName("EditItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditItemPost(MenuItemModel use, int id)
        {
            try
            {
                if (use.IsLinkLocal)
                {
                    var dlLink = await _iLinkRepository.SingleOrDefaultAsync(true, x => x.Id == use.LinkId);
                    use.Href = Functions.FormatUrl(dlLink?.Language, dlLink?.Slug);
                }

                if (ModelState.IsValid)
                {
                    var dl = await _iMenuItemRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    dl.Name = use.Name;
                    dl.Status = use.Status;
                    dl.Href = use.Href;
                    dl.Class = use.Class;
                    dl.Icon = use.Icon;
                    dl.Target = use.Target;
                    dl.IsLinkLocal = use.IsLinkLocal;

                    _iMenuItemRepository.Update(dl);
                    await _iMenuItemRepository.CommitAsync();

                    var dataParent = await _iMenuRepository.SingleOrDefaultAsync(true, x => x.Id == dl.MenuId);

                    CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupMenu(dataParent), ModuleType.Menu, dataParent.Code, dataParent?.Language);

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật menu item \"{dl.Name}\".",
                        Type = LogType.Edit
                    });
                    return new ResponseModel() { Output = 1, Message = "Cập nhật menu thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = false };
                }
                return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin.", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }

        [HttpPost, ActionName("UpdateOrder")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> UpdateOrderPost([FromBody]string data, int id)
        {
            try
            {
                var listItem = ConverData(Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataSortModel>>(data));
                var listItemIds = listItem.Select(x => x.Id).ToList();
                var listCa = await _iMenuItemRepository.SearchAsync(false, 0, 0, x => listItemIds.Contains(x.Id));
                foreach (var item in listCa)
                {
                    var objIn = listItem.FirstOrDefault(x => x.Id == item.Id);
                    item.ParentId = objIn == null ? 0 : objIn.ParentId;
                    item.Order = objIn.Order;
                    _iMenuItemRepository.Update(item);
                }
                await _iMenuItemRepository.CommitAsync();


                var dataParent = await _iMenuRepository.SingleOrDefaultAsync(true, x => x.Id == id);

                CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupMenu(dataParent), ModuleType.Menu, dataParent.Code, dataParent?.Language);

                return new ResponseModel() { Output = 1, Message = "Cập nhật thành công.", Type = ResponseTypeMessage.Success };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [Static]
        private readonly string tokenUrl = "[Url]";
        private readonly string tokenTarget = "[Target]";
        private readonly string tokenName = "[Name]";
        private readonly string tokenIcon = "[Icon]";
        private readonly string tokenContent = "[Content]";
        private readonly string tokenHasChildrent = "[HasChildrent]";
        private readonly string tokenShowChildren = "[Children]";

        private async Task<string> UpdateGroupMenu(Menu dlGroup)
        {
            if (dlGroup == null || dlGroup.Delete) return "";
            if (dlGroup.Template == null || dlGroup.Template1 == null) return "";
            var list = (await _iMenuItemRepository.SearchAsync(true,0,0, m => m.MenuId == dlGroup.Id && m.Status == true)).OrderBy(m => m.Order).ToList();
            var dlLop = LopUpdateGroupMenu(list, 0, dlGroup, 0);
            return dlGroup.Template.Replace(tokenContent, dlLop);
        }
        private string LopUpdateGroupMenu(List<MenuItem> list, int idParent, Menu dlGroup, int level)
        {
            var str = new StringBuilder();
            // vòng lặp 1
            var ListMenuLevel1 = list.Where(m => m.ParentId == 0).OrderBy(m => m.Order);
            string TokenForLevel1 = Functions.TrimToken(dlGroup.Template1, "For") ?? "";
            string TokenForLevel2 = Functions.TrimToken(dlGroup.Template2, "For") ?? "";
            string TokenForLevel3 = Functions.TrimToken(dlGroup.Template3, "For") ?? "";
            if (string.IsNullOrEmpty(dlGroup.Template1))
            {
                return "";
            }
            var stringlv1 = " ";
            foreach (var item1 in ListMenuLevel1)
            {
                if(TokenForLevel1 != "")
                {
                    var stringlv2 = " ";
                    var ListMenuLevel2 = list.Where(m => m.ParentId == item1.Id).OrderBy(m => m.Order);
                    if (!string.IsNullOrEmpty(dlGroup.Template2))
                    {
                        //
                        foreach (var item2 in ListMenuLevel2)
                        {
                            if (TokenForLevel2 != "")
                            {
                                var stringlv3 = " ";
                                if (item2.Id == item1.Id) return "";
                                var ListMenuLevel3 = list.Where(m => m.ParentId == item2.Id).OrderBy(m => m.Order);
                                if (!string.IsNullOrEmpty(dlGroup.Template3))
                                {

                                    foreach (var item3 in ListMenuLevel3)
                                    {
                                        if (TokenForLevel3 != "")
                                        {
                                            if (item3.Id == item2.Id) return "";
                                            stringlv3 += TokenForLevel3
                                                  .Replace(tokenUrl, item3.Href)
                                                  .Replace(tokenTarget, item3.Target)
                                                  .Replace(tokenIcon, item3.Icon)
                                                  .Replace(tokenName, item3.Name)
                                                  .Replace("[AddClass]", item3.Class)
                                                  .Replace(tokenHasChildrent, "");
                                        }
                                    }
                                }
                                stringlv2 += TokenForLevel2
                                   .Replace(tokenUrl, item2.Href)
                                   .Replace(tokenTarget, item2.Target)
                                   .Replace(tokenIcon, item2.Icon)
                                   .Replace(tokenName, item2.Name)
                                   .Replace("[AddClass]", item2.Class)
                                   .Replace(tokenShowChildren, ListMenuLevel3.Count() > 0 ? dlGroup.Template3.Replace(TokenForLevel3 == "" ? " " : TokenForLevel3, stringlv3) : "")
                                   .Replace(tokenHasChildrent, ListMenuLevel3.Count() > 0 ? dlGroup.HasChildrentClass2 : "");
                            }

                        }
                    }
                    stringlv1 += TokenForLevel1
                        .Replace(tokenUrl, item1.Href)
                        .Replace(tokenTarget, item1.Target)
                        .Replace(tokenIcon, item1.Icon)
                        .Replace(tokenName, item1.Name)
                         .Replace("[AddClass]", item1.Class)
                        .Replace(tokenShowChildren, ListMenuLevel2.Count() > 0 ? dlGroup.Template2.Replace(TokenForLevel2, stringlv2) : "")
                        .Replace(tokenHasChildrent, ListMenuLevel2.Count() > 0 ? dlGroup.HasChildrentClass1 : "");
                }
            }
            stringlv1 = dlGroup.Template1.Replace(TokenForLevel1, stringlv1).Replace("[For]", "").Replace("[/For]", "");
            return stringlv1;
        }
        #endregion


        [HttpPost, Authorize]
        public async Task<List<SelectListItem>> SearchLink(string q, string language = null, CategoryType? categoryType = null)
        {
            if (string.IsNullOrEmpty(q))
            {
                return new List<SelectListItem>();
            }
            return (await _iLinkRepository.SearchAsync(true, 0, 20, x => (x.Name.ToLower() == q.ToLower() || x.Name.ToLower().Contains(q.ToLower()) || q == null) && (x.Language == language || language == null) && (x.Type == categoryType || categoryType == null) && x.Status, x => x.OrderBy(y => y.Name),
                x => new Link { Id = x.Id, Name = x.Name, Status = x.Status, Type = x.Type, Language = x.Language  })).Select(x => new SelectListItem { Text = $"({x.Language}|{x.Type.GetDisplayName()}) / {x.Name}", Value = x.Id.ToString() }).ToList();
        }
    }
}