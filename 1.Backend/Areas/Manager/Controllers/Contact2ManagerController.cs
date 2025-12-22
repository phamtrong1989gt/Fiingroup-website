using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;
using PT.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PT.BE.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class Contact2ManagerController : Base.Controllers.BaseController
    {

        private readonly ILogger _logger;
        private readonly IContactRepository _iContactRepository;
        private readonly IPortalRepository _iPortalRepository;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public Contact2ManagerController(
            ILogger<Contact2ManagerController> logger,
            IContactRepository iContactRepository,
            IPortalRepository iPortalRepository,
            IContentPageRepository iContentPageRepository,
            IWebHostEnvironment webHostEnvironment
        )
        {
            controllerName = "Contact2Manager";
            tableName = "Contact";
            _logger = logger;
            _iContactRepository = iContactRepository;
            _iPortalRepository = iPortalRepository;
            _iContentPageRepository = iContentPageRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        #region [Index]
        [AuthorizePermission]
        public async Task<IActionResult> Index(string language = null, int? portalId = 1)
        {
            var portals = await _iPortalRepository.SearchAsync(true, 0, 0);
            ViewData["PortalSelectList"] = new SelectList(portals, "Id", "Name");
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(int? page, int? limit, string key, bool? status,int? serviceId, string startTime, int? portalId, string endTime, string ordertype = "asc", string orderby = "name")
        {
            limit = (limit > 100 || limit < 10) ? 10 : limit;

            // Build shared predicate (no serviceId / portalId passed here)
            var predicate = BuildContactPredicate(key, status, startTime, endTime, serviceId, portalId, Contact.ContactType.Contact);

            var data = await _iContactRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                predicate,
                OrderByExtention(ordertype, orderby));

            var producs = await _iContentPageRepository.SearchAsync(true, 0, 0, x=>x.CategoryType == ECategoryType.ContentPage_Flow);

            foreach (var item in data.Data)
            {
                // Do something with each item
                if(string.IsNullOrEmpty(item.Products))
                    continue;
                try
                {
                    var producids = item.Products.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(id => int.Parse(id)).ToList();
                    item.ProductsList = producs.Where(x => producids.Contains(x.Id)).ToList();
                }
                catch
                {
                }
             
            }
            return View("IndexAjax", data);
        }

        /// <summary>
        /// Build a shared filter predicate for Contact queries used by Index and ExportExcel.
        /// Supports filters: key (search), status, startTime, endTime, serviceId, portalId, type
        /// </summary>
        private Expression<Func<Contact, bool>> BuildContactPredicate(string key, bool? status, string startTime, string endTime, int? serviceId, int? portalId, Contact.ContactType type)
        {
            // Parse start and end time if provided
            DateTime? start = null;
            DateTime? end = null;
            if (!string.IsNullOrWhiteSpace(startTime))
            {
                if (DateTime.TryParseExact(startTime, new[] { "dd/MM/yyyy HH:mm" }, CultureInfo.CurrentCulture, DateTimeStyles.None, out var s))
                {
                    start = s;
                }
                else if (DateTime.TryParse(startTime, out var s2))
                {
                    start = s2;
                }
            }
            if (!string.IsNullOrWhiteSpace(endTime))
            {
                if (DateTime.TryParseExact(endTime, new[] { "dd/MM/yyyy HH:mm"}, CultureInfo.CurrentCulture, DateTimeStyles.None, out var e))
                {
                    end = e;
                }
                else if (DateTime.TryParse(endTime, out var e2))
                {
                    end = e2;
                }
            }

            return m =>
                ((string.IsNullOrEmpty(key) || m.FullName.Contains(key) || m.Email.Contains(key) || m.Phone.Contains(key) || m.Position.Contains(key) || m.ConpanyName.Contains(key))
                 && (status == null || m.Status == status)
                 && (serviceId == null || m.ServiceId == serviceId)
                 && (portalId == null || m.PortalId == portalId)
                 && m.Type == type
                 && (start == null || m.CreatedDate >= start.Value)
                 && (end == null || m.CreatedDate <= end.Value)
                );
        }

        private Func<IQueryable<Contact>, IOrderedQueryable<Contact>> OrderByExtention(string ordertype, string orderby)
        {
            Func<IQueryable<Contact>, IOrderedQueryable<Contact>> functionOrder = null;
            switch (orderby)
            {
                case "name":
                    functionOrder = ordertype == "asc" ? EntityExtention<Contact>.OrderBy(m => m.OrderBy(x => x.FullName)) : EntityExtention<Contact>.OrderBy(m => m.OrderByDescending(x => x.FullName));
                    break;
                default:
                    functionOrder = ordertype == "asc" ? EntityExtention<Contact>.OrderBy(m => m.OrderBy(x => x.Id)) : EntityExtention<Contact>.OrderBy(m => m.OrderByDescending(x => x.Id));
                    break;
            }
            return functionOrder;
        }
        #endregion

        #region [Details]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Details(int id)
        {
            var dl = await _iContactRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null || (dl != null && dl.Delete))
            {
                return View("404");
            }
            if (!dl.Status)
            {
                dl.Status = true;
                _iContactRepository.Update(dl);
                await _iContactRepository.CommitAsync();
            }
            var model = MapModel<ContactModel>.Go(dl);
            return View(model);
        }
        #endregion

        #region [Export Excel]
        [HttpPost]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> ExportExcel(string key, bool? status, string startTime, string endTime, int? serviceId, int? portalId, string ordertype = "desc", string orderby = "id")
        {
            try
            {
                // Set EPPlus License Context
            
                // Use shared predicate that matches Index filters
                var predicate = BuildContactPredicate(key, status, startTime, endTime, serviceId, portalId, Contact.ContactType.Contact);

                // Get data with same filter as IndexPost but without pagination
                var data = await _iContactRepository.SearchAsync(
                    true,
                    0,
                    0,
                    predicate,
                    OrderByExtention(ordertype, orderby));

                // Load related data
                var products = await _iContentPageRepository.SearchAsync(true, 0, 0, x => x.CategoryType == ECategoryType.ContentPage_Flow);

                foreach (var item in data)
                {
                    if (!string.IsNullOrEmpty(item.Products))
                    {
                        try
                        {
                            var producIds = item.Products.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(id => int.Parse(id)).ToList();
                            item.ProductsList = products.Where(x => producIds.Contains(x.Id)).ToList();
                        }
                        catch { }
                    }
                }

                // Create Excel file
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Danh sách liên hệ");

                    // Set up template header
                    worksheet.Cells["A1:J1"].Merge = true;
                    worksheet.Cells["A1"].Value = "DANH SÁCH LIÊN HỆ NHẬN TIN";
                    worksheet.Cells["A1"].Style.Font.Size = 16;
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Row(1).Height = 30;

                    // Export info
                    worksheet.Cells["A2"].Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                    worksheet.Cells["A2"].Style.Font.Italic = true;
                    worksheet.Cells["A2:J2"].Merge = true;

                    // Header row
                    int headerRow = 4;
                    worksheet.Cells[headerRow, 1].Value = "STT";
                    worksheet.Cells[headerRow, 2].Value = "Họ và tên";
                    worksheet.Cells[headerRow, 3].Value = "Số điện thoại";
                    worksheet.Cells[headerRow, 4].Value = "Email";
                    worksheet.Cells[headerRow, 5].Value = "Tên công ty";
                    worksheet.Cells[headerRow, 6].Value = "Chức danh công việc";
                    worksheet.Cells[headerRow, 8].Value = "Lĩnh vực";
                    worksheet.Cells[headerRow, 9].Value = "Mô tả chi tiết";
                    worksheet.Cells[headerRow, 10].Value = "Ngày gửi";

                    // Style header
                    using (var range = worksheet.Cells[headerRow, 1, headerRow, 10])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                        range.Style.Font.Color.SetColor(Color.White);
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }

                    // Data rows
                    int row = headerRow + 1;
                    int stt = 1;
                    foreach (var item in data)
                    {
                        worksheet.Cells[row, 1].Value = stt++;
                        worksheet.Cells[row, 2].Value = Functions.SContent(item.FullName ?? "");
                        worksheet.Cells[row, 3].Value = Functions.SContent(item.Phone ?? "");
                        worksheet.Cells[row, 4].Value = Functions.SContent(item.Email ?? "");
                        worksheet.Cells[row, 5].Value = Functions.SContent(item.ConpanyName ?? "");
                        worksheet.Cells[row, 6].Value = Functions.SContent(item.Position ?? "");
                        worksheet.Cells[row, 8].Value = item.ProductsList != null && item.ProductsList.Any()
                            ? string.Join(", ", item.ProductsList.Select(p => Functions.SContent(p.Name ?? "")))
                            : "";
                        worksheet.Cells[row, 9].Value = Functions.SContent(item.Content ?? "");
                        worksheet.Cells[row, 10].Value = item.CreatedDate.ToString("dd/MM/yyyy HH:mm");

                        // Style data rows
                        using (var range = worksheet.Cells[row, 1, row, 10])
                        {
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            range.Style.WrapText = true;
                        }

                        // Center align STT
                        worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        
                        // Apply alternating row colors
                        if (row % 2 == 0)
                        {
                            using (var range = worksheet.Cells[row, 1, row, 10])
                            {
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));
                            }
                        }

                        row++;
                    }

                    // Auto-fit columns
                    worksheet.Column(1).Width = 6;   // STT
                    worksheet.Column(2).Width = 25;  // Họ và tên
                    worksheet.Column(3).Width = 15;  // SĐT
                    worksheet.Column(4).Width = 30;  // Email
                    worksheet.Column(5).Width = 30;  // Tên công ty
                    worksheet.Column(6).Width = 25;  // Chức danh
                    worksheet.Column(8).Width = 40;  // Sản phẩm
                    worksheet.Column(9).Width = 50;  // Mô tả
                    worksheet.Column(10).Width = 18; // Ngày gửi

                    // Freeze header rows
                    worksheet.View.FreezePanes(headerRow + 1, 1);

                    // Return file
                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    string fileName = $"DanhSachLienHe_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting contacts to Excel");
                return BadRequest(new { success = false, message = "Có lỗi xảy ra khi xuất file Excel. Vui lòng thử lại." });
            }
        }
        #endregion
    }
}