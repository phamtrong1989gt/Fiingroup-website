using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using PT.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.UI.Areas.Manager.Controllers
{
    [Area("Manager")]
    [IsSupperAdminAuthorizePermission]
    public class FileManagerController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Allowed text file extensions for editing
        private static readonly HashSet<string> EditableExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".txt", ".css", ".html", ".htm", ".js", ".json", ".xml", 
            ".razor", ".cshtml", ".cs", ".config", ".md", ".sql"
        };

        public FileManagerController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Main view - File Manager UI
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get available drives
        /// GET: /Manager/FileManager/GetDrives
        /// </summary>
        [HttpGet]
        public IActionResult GetDrives()
        {
            try
            {
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.IsReady)
                    .Select(d => new
                    {
                        name = d.Name,
                        label = $"{d.Name} ({d.VolumeLabel})",
                        driveType = d.DriveType.ToString(),
                        totalSize = d.TotalSize,
                        availableSpace = d.AvailableFreeSpace,
                        totalSizeFormatted = FormatFileSize(d.TotalSize),
                        availableSpaceFormatted = FormatFileSize(d.AvailableFreeSpace)
                    })
                    .ToList();

                return Json(new { success = true, drives });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Browse directory contents
        /// GET: /Manager/FileManager/Browse?path=/auto-css
        /// OR: /Manager/FileManager/Browse?path=C:\
        /// </summary>
        [HttpGet]
        public IActionResult Browse(string path = "")
        {
            try
            {
                // If path is empty, return drives list
                if (string.IsNullOrWhiteSpace(path))
                {
                    return GetDrives();
                }

                // Full path from root (e.g., C:\, D:\Projects\)
                var fullPath = path;

                if (!Directory.Exists(fullPath))
                {
                    return Json(new { success = false, message = "Directory not found" });
                }

                // Get directories
                var directories = Directory.GetDirectories(fullPath)
                    .Select(d => new
                    {
                        name = Path.GetFileName(d),
                        path = d,
                        type = "folder",
                        size = 0L,
                        modified = Directory.GetLastWriteTime(d).ToString("yyyy-MM-dd HH:mm:ss")
                    })
                    .OrderBy(d => d.name);

                // Get files
                var files = Directory.GetFiles(fullPath)
                    .Select(f => new FileInfo(f))
                    .Select(fi => new
                    {
                        name = fi.Name,
                        path = fi.FullName,
                        type = "file",
                        extension = fi.Extension,
                        size = fi.Length,
                        sizeFormatted = FormatFileSize(fi.Length),
                        modified = fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        canEdit = IsEditable(fi.Extension)
                    })
                    .OrderBy(f => f.name);

                var items = directories.Concat<object>(files).ToList();

                return Json(new
                {
                    success = true,
                    path = fullPath,
                    items,
                    parentPath = GetParentPath(fullPath)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Download file
        /// GET: /Manager/FileManager/Download?path=C:\Projects\file.css
        /// </summary>
        [HttpGet]
        public IActionResult Download(string path)
        {
            try
            {
                if (!System.IO.File.Exists(path))
                {
                    return NotFound("File not found");
                }

                var fileBytes = System.IO.File.ReadAllBytes(path);
                var fileName = Path.GetFileName(path);
                var contentType = GetContentType(Path.GetExtension(path));

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get file content for editing
        /// GET: /Manager/FileManager/GetContent?path=C:\Projects\file.css
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetContent(string path)
        {
            try
            {
                if (!System.IO.File.Exists(path))
                {
                    return Json(new { success = false, message = "File not found" });
                }

                var extension = Path.GetExtension(path);
                if (!IsEditable(extension))
                {
                    return Json(new { success = false, message = "This file type cannot be edited" });
                }

                var content = await System.IO.File.ReadAllTextAsync(path);
                var fileInfo = new FileInfo(path);

                return Json(new
                {
                    success = true,
                    content,
                    path = path,
                    fileName = fileInfo.Name,
                    extension = fileInfo.Extension,
                    size = fileInfo.Length,
                    modified = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Save file content
        /// POST: /Manager/FileManager/SaveContent
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveContent([FromBody] SaveFileRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Path))
                {
                    return Json(new { success = false, message = "Path is required" });
                }

                if (!System.IO.File.Exists(request.Path))
                {
                    return Json(new { success = false, message = "File not found" });
                }

                var extension = Path.GetExtension(request.Path);
                if (!IsEditable(extension))
                {
                    return Json(new { success = false, message = "This file type cannot be edited" });
                }

                // Backup original file
                var backupPath = request.Path + ".backup";
                System.IO.File.Copy(request.Path, backupPath, true);

                try
                {
                    // Save new content
                    await System.IO.File.WriteAllTextAsync(request.Path, request.Content ?? string.Empty, Encoding.UTF8);

                    return Json(new
                    {
                        success = true,
                        message = "File saved successfully",
                        modified = System.IO.File.GetLastWriteTime(request.Path).ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }
                catch
                {
                    // Restore backup on error
                    System.IO.File.Copy(backupPath, request.Path, true);
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #region Helper Methods

        private string GetParentPath(string fullPath)
        {
            try
            {
                var parentDir = Directory.GetParent(fullPath);
                return parentDir?.FullName;
            }
            catch
            {
                return null;
            }
        }

        private bool IsEditable(string extension)
        {
            return EditableExtensions.Contains(extension);
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".html" or ".htm" => "text/html",
                ".txt" => "text/plain",
                ".xml" => "application/xml",
                ".pdf" => "application/pdf",
                ".zip" => "application/zip",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }

        #endregion
    }

    #region Request Models

    public class SaveFileRequest
    {
        public string Path { get; set; }
        public string Content { get; set; }
    }

    #endregion
}
