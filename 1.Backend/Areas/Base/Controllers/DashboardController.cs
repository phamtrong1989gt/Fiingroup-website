using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PT.Base;
using PT.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;

namespace PT.BE.Areas.Base.Controllers
{
    [Area("Base")]
    //[Authorize]
    public class DashboardController : Controller
    {
        // P/Invoke for Windows memory stats
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        [AuthorizePermission]
        [Route("Admin")]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("Admin/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Returns basic server and process metrics for display in admin dashboard
        [HttpGet]
        [Route("Admin/Metrics")]
        public IActionResult Metrics()
        {
            try
            {
                var proc = Process.GetCurrentProcess();
                var hostName = Dns.GetHostName();
                var ips = Dns.GetHostEntry(hostName).AddressList
                    .Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Select(a => a.ToString()).ToArray();

                var uptimeSeconds = (DateTime.UtcNow - proc.StartTime.ToUniversalTime()).TotalSeconds;
                var cpuTotalSeconds = proc.TotalProcessorTime.TotalSeconds;
                var processorCount = Environment.ProcessorCount;

                double processCpuPercent = 0;
                if (uptimeSeconds > 0)
                {
                    // Average CPU usage by this process over its lifetime, relative to total CPU capacity
                    processCpuPercent = (cpuTotalSeconds / (processorCount * uptimeSeconds)) * 100.0;
                }

                // Memory: try platform-specific total physical memory
                long totalPhysical = 0;
                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        var mem = new MEMORYSTATUSEX();
                        mem.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                        if (GlobalMemoryStatusEx(ref mem))
                        {
                            totalPhysical = (long)mem.ullTotalPhys;
                        }
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        var meminfo = System.IO.File.ReadAllLines("/proc/meminfo");
                        foreach (var line in meminfo)
                        {
                            if (line.StartsWith("MemTotal:", StringComparison.OrdinalIgnoreCase))
                            {
                                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length >= 2 && long.TryParse(parts[1], out var kb))
                                {
                                    totalPhysical = kb * 1024; // kB to bytes
                                }
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    totalPhysical = 0;
                }

                double memoryPercent = -1;
                if (totalPhysical > 0)
                {
                    memoryPercent = (double)proc.WorkingSet64 / totalPhysical * 100.0;
                }

                // Drive/disk info
                var drives = new List<object>();
                try
                {
                    foreach (var d in DriveInfo.GetDrives())
                    {
                        if (!d.IsReady) continue;
                        long total = 0;
                        long free = 0;
                        try
                        {
                            total = d.TotalSize;
                            free = d.AvailableFreeSpace;
                        }
                        catch { }
                        long used = Math.Max(0, total - free);
                        double usedPct = total > 0 ? (double)used / total * 100.0 : 0;
                        drives.Add(new
                        {
                            Name = d.Name,
                            DriveType = d.DriveType.ToString(),
                            TotalBytes = total,
                            FreeBytes = free,
                            UsedBytes = used,
                            UsedPercent = Math.Round(usedPct, 2)
                        });
                    }
                }
                catch { }

                var data = new
                {
                    MachineName = Environment.MachineName,
                    OS = RuntimeInformation.OSDescription,
                    Runtime = RuntimeInformation.FrameworkDescription,
                    ProcessorCount = processorCount,
                    ProcessId = proc.Id,
                    ProcessName = proc.ProcessName,
                    ProcessWorkingSet = proc.WorkingSet64,
                    ProcessPrivateMemory = proc.PrivateMemorySize64,
                    ProcessTotalCpuMs = proc.TotalProcessorTime.TotalMilliseconds,
                    ProcessCpuPercent = Math.Round(processCpuPercent, 2),
                    ProcessStartTime = proc.StartTime,
                    UptimeSeconds = uptimeSeconds,
                    GCAllocatedMemory = GC.GetTotalMemory(false),
                    MemoryTotalBytes = totalPhysical,
                    MemoryUsedPercent = totalPhysical > 0 ? Math.Round(memoryPercent, 2) : (double?)null,
                    ServerIps = ips,
                    LocalIp = HttpContext.Connection.LocalIpAddress?.ToString(),
                    LocalPort = HttpContext.Connection.LocalPort,
                    RemoteIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Drives = drives
                };

                return Json(new { output = 1, data });
            }
            catch (Exception ex)
            {
                return Json(new { output = 0, message = ex.Message });
            }
        }
    }
}