using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PT.Shared
{

    public class Functions
    {
        /// <summary>
        /// Thiết lập và trả về đường dẫn vật lý và đường dẫn public cho thư mục chia sẻ /Data.
        /// - Nếu <paramref name="configuredDataPath"/> được cung cấp sẽ được ưu tiên. Hỗ trợ đường dẫn tuyệt đối và tương đối (so với webRootPath).
        /// - Nếu không có cấu hình sẽ fallback về wwwroot/Data (sử dụng webRootPath nếu có).
        /// - <paramref name="folderByDate"/> có thể truyền vào (ví dụ Functions.GenFolderByDate()) để tạo subfolder theo ngày.
        /// Hàm đảm bảo thư mục vật lý tồn tại (kiểm tra trước khi tạo).
        /// Trả về tuple (PhysicalPath, PublicUrl).
        /// </summary>
        public static (string PhysicalPath, string PublicUrl) SetupSharedDataFolder(string configuredDataPath = null, string folderByDate = null)
        {
            // Lấy folder theo ngày hoặc mặc định
            var folder = string.IsNullOrWhiteSpace(folderByDate) ? GenFolderByDate() : folderByDate;

            string physicalBasePath;
            if (!string.IsNullOrWhiteSpace(configuredDataPath))
            {
                // Nếu là đường dẫn tuyệt đối dùng luôn.
                // Nếu là đường dẫn tương đối thì kết hợp với current directory (không dùng any host paths)
                physicalBasePath = Path.IsPathRooted(configuredDataPath)
                ? configuredDataPath
                : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), configuredDataPath));
            }
            else
            {
                // Mặc định lưu trong Data trong current directory
                physicalBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            }

            // Chuẩn hóa folder (loại bỏ dấu / đầu nếu có)
            var relativeFolder = (folder ?? string.Empty).TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.Combine(physicalBasePath, relativeFolder);

            // Kiểm tra tồn tại thư mục rồi tạo nếu chưa có
            if (!Directory.Exists(physicalPath))
            {
                Directory.CreateDirectory(physicalPath);
            }

            // PublicUrl luôn bắt đầu bằng /Data và theo định dạng URL (dấu /)
            var publicUrl = "/Data" + (folder.StartsWith("/") ? folder : "/" + folder);
            if (!publicUrl.EndsWith("/")) publicUrl += "/";

            return (physicalPath, publicUrl);
        }

        public static decimal GetFullPrice(decimal price)
        {
            return (price * 5 / 4);
        }

        public static string SubStringTitle(string input, int num)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "";
            }
            else if (input.Length <= num)
            {
                return input;
            }
            else
            {
                return input.Substring(0, num - 1);
            }
        }
        public static List<string> StringToListItem(string input, string sp = ",")
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                {
                    return new List<string>();
                }
                return input.Split(sp).Where(x => !string.IsNullOrEmpty(x)).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public static string FormatMoney(double input)
        {
            if (input >= 1000)
            {
                return string.Format("{0:00,0}", input);
            }
            return input.ToString();
        }

        public static string TrimToken(string sInput, string sToken)
        {
            try
            {
                string sStart = "[" + sToken + "]";
                string sEnd = "[/" + sToken + "]";
                if (!sInput.Contains(sStart) || !sInput.Contains(sEnd)) return "";

                int startIndex = sInput.IndexOf(sStart, StringComparison.CurrentCultureIgnoreCase) + sStart.Length;
                int endIndex = sInput.IndexOf(sEnd, startIndex, StringComparison.CurrentCultureIgnoreCase);
                int length = endIndex - startIndex;

                return sInput.Substring(startIndex, length);
            }
            catch { return ""; }
        }

        public static string ZipStringHTML(string value)
        {
            var REGEX_TAGS = new Regex(@">\\s+<", RegexOptions.Compiled);
            var REGEX_ALL = new Regex(@"\\s+|\\t\\s+|\\n\\s+|\\r\\s+", RegexOptions.Compiled);
            if (value != null)
            {
                var html = value.ToString();
                html = REGEX_TAGS.Replace(html, "><");
                html = REGEX_ALL.Replace(html, " ");
                return html;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Sanitizes user input content to prevent XSS, SQL Injection, and other security threats.
        /// Removes dangerous HTML tags, scripts, event handlers, and malicious protocols.
        /// Returns a safe plain text string.
        /// </summary>
        /// <param name="content">The user input content to sanitize</param>
        /// <param name="maxLength">Maximum allowed length (default: 10000 characters)</param>
        /// <returns>Sanitized plain text content</returns>
        public static string SContent(string content, int maxLength = 10000)
        {
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            try
            {
                // 0. Enforce maximum length to prevent DoS attacks
                if (content.Length > maxLength)
                {
                    content = content.Substring(0, maxLength);
                }

                // 1. Remove null bytes and control characters (except newline, carriage return, tab)
                content = Regex.Replace(content, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", string.Empty, RegexOptions.Compiled);

                // 2. Remove script/style/iframe/object/embed/form/svg tags and their content
                content = Regex.Replace(content, @"<(script|style|iframe|object|embed|form|svg|math|applet|base|frame|frameset|noscript|xml)[\s\S]*?</\1\s*>", 
                    string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                
                // Remove self-closing dangerous tags
                content = Regex.Replace(content, @"<(iframe|embed|object|link|meta|base|svg|script|style|img|video|audio|source|track)[^>]*\/?>", 
                    string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);

                // 3. Remove ALL event handler attributes (on* attributes)
                content = Regex.Replace(content, @"\s+on\w+\s*=\s*(?:'[^']*'|""[^""]*""|[^\s>]+)", 
                    string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);

                // 4. Remove dangerous protocols in attributes and standalone
                var dangerousProtocols = new[] { "javascript", "vbscript", "data", "file", "about", "jar", "ms-", "mocha", "livescript" };
                foreach (var protocol in dangerousProtocols)
                {
                    content = Regex.Replace(content, $@"{protocol}\s*:", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }

                // 5. Remove dangerous attribute names (src, href, action, formaction, etc. in remaining tags)
                content = Regex.Replace(content, @"\s+(src|href|action|formaction|background|lowsrc|ping|poster|xlink:href)\s*=\s*(?:'[^']*'|""[^""]*""|[^\s>]+)", 
                    string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);

                // 6. Remove CSS expressions and imports
                content = Regex.Replace(content, @"expression\s*\(", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                content = Regex.Replace(content, @"@import", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);

                // 7. Remove HTML comments (can hide malicious code)
                content = Regex.Replace(content, @"<!--[\s\S]*?-->", string.Empty, RegexOptions.Compiled);

                // 8. Remove CDATA sections
                content = Regex.Replace(content, @"<!\[CDATA\[[\s\S]*?\]\]>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);

                // 9. Remove ALL remaining HTML/XML tags
                content = Regex.Replace(content, @"<[^>]+>", string.Empty, RegexOptions.Compiled);
                
                // 10. Remove potential SQL injection patterns (basic defense in depth)
                // Note: This is NOT a replacement for parameterized queries!
                var sqlPatterns = new[]
                {
                    @"\b(union|select|insert|update|delete|drop|create|alter|exec|execute|script|declare)\b",
                    @"--|;|\/\*|\*\/",
                    @"xp_|sp_|0x[0-9a-f]+",
                    @"char\s*\(|ascii\s*\(|concat\s*\("
                };
                
                foreach (var pattern in sqlPatterns)
                {
                    content = Regex.Replace(content, pattern, string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }

                // 11. Decode HTML entities multiple times to catch nested encoding
                for (int i = 0; i < 3; i++)
                {
                    var previousContent = content;
                    content = System.Net.WebUtility.HtmlDecode(content);
                    
                    // If no change after decode, break to avoid infinite loop
                    if (content == previousContent)
                        break;
                }

                // 12. Remove Unicode directional override characters (can hide malicious content)
                content = Regex.Replace(content, @"[\u202A-\u202E\u2066-\u2069]", string.Empty, RegexOptions.Compiled);

                // 13. Remove zero-width characters
                content = Regex.Replace(content, @"[\u200B-\u200D\uFEFF]", string.Empty, RegexOptions.Compiled);

                // 14. Normalize and collapse whitespace
                content = Regex.Replace(content, @"[ \t]+", " ", RegexOptions.Compiled); // Collapse spaces/tabs
                content = Regex.Replace(content, @"\r?\n\s*\r?\n+", "\n\n", RegexOptions.Compiled); // Collapse multiple newlines
                
                // 15. Trim and final validation
                content = content.Trim();

                // 16. Final sanity check: if content looks suspicious, return empty
                if (IsSuspiciousContent(content))
                {
                    return string.Empty;
                }

                return content;
            }
            catch (Exception)
            {
                // On any error return empty to avoid returning potentially unsafe content
                return string.Empty;
            }
        }

        /// <summary>
        /// Performs additional checks to detect potentially malicious content patterns
        /// </summary>
        private static bool IsSuspiciousContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return false;

            // Check for excessive special characters (may indicate obfuscation)
            int specialCharCount = content.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
            if (content.Length > 0 && (double)specialCharCount / content.Length > 0.5)
                return true;

            // Check for patterns that survived sanitization but are still suspicious
            var suspiciousPatterns = new[]
            {
                @"eval\s*\(",
                @"setTimeout\s*\(",
                @"setInterval\s*\(",
                @"Function\s*\(",
                @"\\x[0-9a-f]{2}",  // Hex encoding
                @"\\u[0-9a-f]{4}",  // Unicode encoding
                @"%[0-9a-f]{2}",    // URL encoding
                @"&#\d+;",          // Numeric HTML entities that survived
                @"&#x[0-9a-f]+;",   // Hex HTML entities
                @"\{.*?\$.*?\}",    // Template injection patterns
                @"\[\[.*?\]\]"      // Template injection patterns
            };

            foreach (var pattern in suspiciousPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }

        public static byte[] StringToByte(string data)
        {
            var chars = Unicode2TCVN3.ToTCVN3(data);
            var bytes = new byte[chars.Length + 2];
            for (int i = 0; i < chars.Length; i++)
            {
                bytes[i] = (byte)chars[i];
            }
            bytes[chars.Length] = 0x0D;
            bytes[chars.Length + 1] = 0x0A;
            return bytes;
        }

        public static string GetBitStr(byte[] data)
        {
            BitArray bits = new BitArray(data);

            string strByte = string.Empty;
            for (int i = 0; i <= bits.Count - 1; i++)
            {
                if (i % 8 == 0)
                {
                    strByte += " ";
                }
                strByte += (bits[i] ? "1" : "0");
            }

            return strByte;
        }
        public static Size GetSizeAdjustedToAspectRatio(int sourceWidth, int sourceHeight, int dWidth, int dHeight)
        {
            bool isLandscape = sourceWidth > sourceHeight ? true : false;
            int fixedSize = dWidth;

            double aspectRatio = (double)sourceWidth / (double)sourceHeight; ;

            if (isLandscape)
                return new Size(fixedSize, (int)((fixedSize / aspectRatio) + 0.5));
            else
                return new Size((int)((fixedSize * aspectRatio) + 0.5), fixedSize);
        }

        public static string ConvertHTMLToRTF(string html)
        {
            try
            {
                //SautinSoft.HtmlToRtf h = new SautinSoft.HtmlToRtf();
                //h.OpenHtml(html);
                //return h.ToRtf();
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
 {
 {".txt", "text/plain"},
 {".pdf", "application/pdf"},
 {".doc", "application/vnd.ms-word"},
 {".docx", "application/vnd.ms-word"},
 {".xls", "application/vnd.ms-excel"},
 {".xlsx", "application/vnd.openxmlformats officedocument.spreadsheetml.sheet"},
 {".png", "image/png"},
 {".jpg", "image/jpeg"},
 {".jpeg", "image/jpeg"},
 {".gif", "image/gif"},
 {".csv", "text/csv"},
 {".mp4","video/mp4"},
 {".avi","video/avi"}
 };
        }
        public static string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }
        public static int DateTimeToUnixTimestamp(DateTime dateTime)
        {
            TimeSpan span = (TimeSpan)(dateTime.ToUniversalTime() - new DateTime(0x7b2, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            return (int)span.TotalSeconds;
        }

        public static double CalcuDistance(double srcLong, double srcLat, double desLong, double desLat)
        {
            double num = srcLong * 0.017453292519943295;
            double d = srcLat * 0.017453292519943295;
            double num3 = desLong * 0.017453292519943295;
            double num4 = desLat * 0.017453292519943295;
            double num5 = num3 - num;
            double num6 = num4 - d;
            double num7 = Math.Pow(Math.Sin(num6 / 2.0), 2.0) + ((Math.Cos(d) * Math.Cos(num4)) * Math.Pow(Math.Sin(num5 / 2.0), 2.0));
            double num8 = 2.0 * Math.Atan2(Math.Sqrt(num7), Math.Sqrt(1.0 - num7));
            return (6378.5 * num8);
        }

        public static bool CheckInLocation(double srcLong, double srcLat, double desLong, double desLat, double Rkm)
        {
            if (CalcuDistance(srcLong, srcLat, desLong, desLat) > Rkm)
            {
                return false;
            }
            return true;
        }

        public bool IsNumber(string pText)
        {
            Regex regex = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");
            return regex.IsMatch(pText);
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime time = new DateTime(0x7b2, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return time.AddSeconds(unixTimeStamp).ToLocalTime();
        }

        public static int GenSTT(int cstt, int page, int size = 10)
        {
            return (page - 1) * size + cstt;
        }

        //private static  string _identString = "";
        //public static string FormatJson(string str)
        //{
        //    var indent = 0;
        //    var quoted = false;
        //    var sb = new StringBuilder();
        //    for (var i = 0; i < str.Length; i++)
        //    {
        //        var ch = str[i];
        //        switch (ch)
        //        {
        //            case '{':
        //            case '[':
        //                sb.Append(ch);
        //                if (!quoted)
        //                {
        //                    sb.AppendLine();
        //                    Enumerable.Range(0, ++indent).ForEach(item => sb.Append(_identString));
        //                }
        //                break;
        //            case '}':
        //            case ']':
        //                if (!quoted)
        //                {
        //                    sb.AppendLine();
        //                    Enumerable.Range(0, --indent).ForEach(item => sb.Append(_identString));
        //                }
        //                sb.Append(ch);
        //                break;
        //            case '"':
        //                sb.Append(ch);
        //                bool escaped = false;
        //                var index = i;
        //                while (index > 0 && str[--index] == '\\')
        //                    escaped = !escaped;
        //                if (!escaped)
        //                    quoted = !quoted;
        //                break;
        //            case ',':
        //                sb.Append(ch);
        //                if (!quoted)
        //                {
        //                    sb.AppendLine();
        //                    Enumerable.Range(0, indent).ForEach(item => sb.Append(_identString));
        //                }
        //                break;
        //            case ':':
        //                sb.Append(ch);
        //                if (!quoted)
        //                    sb.Append(" ");
        //                break;
        //            default:
        //                sb.Append(ch);
        //                break;
        //        }
        //    }
        //    return sb.ToString();
        //}
        public static string FormatReturnUrl(string returnUrl, string urlDefault)
        {
            try
            {
                return string.IsNullOrEmpty(returnUrl) ? urlDefault : returnUrl.ToString();
            }
            catch
            {
                return urlDefault;
            }
        }
        public static string FormatUrl(string language, string slug)
        {
            if (slug == "")
            {
                return $"/{language}";
            }
            return $"/{language}/{slug}.html";
            //if(string.IsNullOrEmpty(language))
            //{
            //    return $"/{slug}.html";
            //}
            //else
            //{
            //    return $"/{language}/{slug}.html";
            //}
        }

        public static string FormatTrips(string data)
        {
            try
            {
                if (data == null)
                {
                    return "";
                }
                var list = data.Split(',');
                return string.Join(" → ", list);
            }
            catch
            {
                return data;
            }
        }

        public static string GenFolderByDate()
        {
            return DateTime.Now.ToString("/yyyy/MM/");
        }

        public static string ToJson(object data)
        {
            return null;
            //var serializerSettings = new JsonSerializerSettings
            //{
            //    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            //};
            //var json = JsonConvert.SerializeObject(data, serializerSettings);
            //return json;
        }
        public static long ConvertToUnixTime(DateTime datetime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return (long)(datetime - sTime).TotalSeconds;
        }

        public static double DistanceInMeter(double lat1, double lon1, double lat2, double lon2)
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(Deg2rad(lat1)) * Math.Sin(Deg2rad(lat2)) + Math.Cos(Deg2rad(lat1)) * Math.Cos(Deg2rad(lat2)) * Math.Cos(Deg2rad(theta));
            dist = Math.Acos(dist);
            dist = Rad2deg(dist);
            dist = dist * 60 * 1.1515;

            // meter
            dist = dist * 1.609344 * 1000;

            return (dist);
        }

        private static double Deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private static double Rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        public static string GetHours(string from, string to)
        {
            if (from == null || to == null) return "04:00";
            if (to == "00:00")
            {
                to = "23:59:59";
            }
            var fromDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd") + " " + from);
            var toDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd") + " " + to);
            TimeSpan myDateResult = new TimeSpan();
            myDateResult = toDate - fromDate;
            return myDateResult.ToString();
        }

        public static void StringToFile(string mapPath, string data)
        {
            if (!File.Exists(mapPath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(mapPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(mapPath));
                }
                System.IO.FileInfo fi = new System.IO.FileInfo(mapPath);
                using (System.IO.FileStream fs = fi.Create())
                {
                    Byte[] txt = new System.Text.UTF8Encoding(true).GetBytes("New file.");
                    fs.Write(txt, 0, txt.Length);
                    Byte[] author = new System.Text.UTF8Encoding(true).GetBytes("Author Mahesh Chand");
                    fs.Write(author, 0, author.Length);
                }
            }
            if (!IsFileLocked(new FileInfo(mapPath)))
            {
                using (FileStream fs = new FileStream(mapPath, FileMode.Truncate, FileAccess.Write))
                {
                    StreamWriter wt = new StreamWriter(fs, Encoding.Unicode);
                    fs.Seek(0, SeekOrigin.End);
                    wt.WriteLine(data);
                    wt.Flush();
                    wt.Close();
                }
            }
        }
        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                try
                {
                    if (stream != null)
                        stream.Close();
                }
                catch
                {

                }
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked 
            return false;
        }
        public static string GenPageList(string url, int curentPage, int totalRow, int size, string query = null, string classActive = "uk-active", string liClass = null)
        {
            size = 1;
            var sb = new StringBuilder();
            int totalPage = (totalRow % size > 0) ? (totalRow / size + 1) : (totalRow / size);
            if (totalPage <= 1)
            {
                return string.Empty;
            }

            curentPage = Math.Max(1, curentPage);
            string q = query ?? string.Empty;

            // previous button
            if (curentPage > 1)
            {
                int prev = curentPage - 1;
                sb.Append($"<li class=\"{liClass}\"><a rel='nofollow' class=\"page-link\" href=\"{url}?page={prev}{q}\">&laquo;</a></li>");
            }

            // If few pages, show all
            if (totalPage <= 9)
            {
                for (int i = 1; i <= totalPage; i++)
                {
                    sb.Append($"<li class=\"{liClass} {(i == curentPage ? classActive : "")}\"><a rel='nofollow' class=\"page-link\" href=\"{url}?page={i}{q}\">{i}</a></li>");
                }
            }
            else
            {
                // always show first page
                sb.Append($"<li class=\"{liClass} {(1 == curentPage ? classActive : "")}\"><a rel='nofollow' class=\"page-link\" href=\"{url}?page=1{q}\">1</a></li>");

                int start = Math.Max(2, curentPage - 2);
                int end = Math.Min(totalPage - 1, curentPage + 2);

                if (start > 2)
                {
                    sb.Append($"<li class=\"{liClass}\"><span class=\"page-ellipsis\">...</span></li>");
                }

                for (int i = start; i <= end; i++)
                {
                    sb.Append($"<li class=\"{liClass} {(i == curentPage ? classActive : "")}\"><a rel='nofollow' class=\"page-link\" href=\"{url}?page={i}{q}\">{i}</a></li>");
                }

                if (end < totalPage - 1)
                {
                    sb.Append($"<li class=\"{liClass}\"><span class=\"page-ellipsis\">...</span></li>");
                }

                // last page
                sb.Append($"<li class=\"{liClass} {(totalPage == curentPage ? classActive : "")}\"><a rel='nofollow' class=\"page-link\" href=\"{url}?page={totalPage}{q}\">{totalPage}</a></li>");
            }

            // next button
            if (curentPage < totalPage)
            {
                int next = curentPage + 1;
                sb.Append($"<li class=\"{liClass}\"><a rel='nofollow' class=\"page-link\" href=\"{url}?page={next}{q}\">&raquo;</a></li>");
            }

            return sb.ToString();
        }

        public static string GenPageListAjax2(string url, int curentPage, int totalRow, int size, string query = null, string classActive = "uk-active", string liClass = null, string function = null)
        {
            var sb = new StringBuilder();
            int totalPage = (totalRow % size > 0) ? (totalRow / size + 1) : (totalRow / size);
            if (totalPage <= 1)
            {
                return string.Empty;
            }

            curentPage = Math.Max(1, curentPage);
            string q = query ?? string.Empty;
            var baseLiClass = liClass ?? string.Empty;

            // previous button - always rendered (disabled when on first page)
            if (curentPage > 1)
            {
                int prev = curentPage - 1;
                if (!string.IsNullOrEmpty(function))
                    sb.Append($"<li class=\"{baseLiClass}\"><a role=\"button\" tabindex=\"0\" aria-pressed=\"false\" rel='nofollow' onclick=\"{function}({prev})\" class=\"page-item\" data-href=\"{url}?page={prev}{q}\">&laquo;</a></li>");
                else
                    sb.Append($"<li class=\"{baseLiClass}\"><a role=\"button\" tabindex=\"0\" aria-pressed=\"false\" rel='nofollow' class=\"page-item\" data-href=\"{url}?page={prev}{q}\">&laquo;</a></li>");
            }
            else
            {
                sb.Append($"<li class=\"{baseLiClass} disabled\"><span class=\"page-item disabled\">&laquo;</span></li>");
            }

            if (totalPage <= 9)
            {
                for (int i = 1; i <= totalPage; i++)
                {
                    var active = i == curentPage ? classActive : string.Empty;
                    if (!string.IsNullOrEmpty(function))
                        sb.Append($"<li class=\"{baseLiClass} {active}\"><a role=\"button\" tabindex=\"0\" aria-pressed=\"false\" rel='nofollow' onclick=\"{function}({i})\" class=\"page-item\" data-href=\"{url}?page={i}{q}\">{i}</a></li>");
                    else
                        sb.Append($"<li class=\"{baseLiClass} {active}\"><a role=\"button\" tabindex=\"0\" aria-pressed=\"false\" rel='nofollow' class=\"page-item\" data-href=\"{url}?page={i}{q}\">{i}</a></li>");
                }
            }
            else
            {
                // first page
                var firstActive = 1 == curentPage ? classActive : string.Empty;
                if (!string.IsNullOrEmpty(function))
                    sb.Append($"<li class=\"{baseLiClass} {firstActive}\"><a role=\"button\" tabindex=\"0\" aria-pressed=\"false\" rel='nofollow' onclick=\"{function}(1)\" class=\"page-item\" data-href=\"{url}?page=1{q}\">1</a></li>");
                else
                    sb.Append($"<li class=\"{baseLiClass} {firstActive}\"><a role=\"button\" tabindex=\"0\" aria-pressed=\"false\" rel='nofollow' class=\"page-item\" data-href=\"{url}?page=1{q}\">1</a></li>");

                int start = Math.Max(2, curentPage - 2);
                int end = Math.Min(totalPage - 1, curentPage + 2);

                if (start > 2)
                {
                    sb.Append($"<li class=\"{baseLiClass}\"><span class=\"page-ellipsis\">...</span></li>");
                }

                for (int i = start; i <= end; i++)
                {
                    var active = i == curentPage ? classActive : string.Empty;
                    if (!string.IsNullOrEmpty(function))
                        sb.Append($"<li class=\"{baseLiClass} {active}\"><a role=\"button\" tabindex=\"0\" aria-pressed=\"false\" rel='nofollow' onclick=\"{function}({i})\" class=\"page-item\" data-href=\"{url}?page={i}{q}\">{i}</a></li>");
                    else
                        sb.Append($"<li class=\"{baseLiClass} {active}\"><a role=\"button\" tabindex=\"0\" aria-pressed=\"false\" rel='nofollow' class=\"page-item\" data-href=\"{url}?page={i}{q}\">{i}</a></li>");
                }

                if (end < totalPage - 1)
                {
                    sb.Append($"<li class=\"{baseLiClass}\"><span class=\"page-ellipsis\">...</span></li>");
                }

                // last page
                var lastActive = totalPage == curentPage ? classActive : string.Empty;
                if (!string.IsNullOrEmpty(function))
                    sb.Append($"<li class=\"{baseLiClass} {lastActive}\"><a role=\"button\" tabindex=\"0\" aria-pressed=\"false\" rel='nofollow' onclick=\"{function}({totalPage})\" class=\"page-item\" data-href=\"{url}?page={totalPage}{q}\">{totalPage}</a></li>");
                else
                    sb.Append($"<li class=\"{baseLiClass} {lastActive}\"><a role=\"button\" tabindex=\"0\" aria-pressed=\"false\" rel='nofollow' class=\"page-item\" data-href=\"{url}?page={totalPage}{q}\">{totalPage}</a></li>");
            }

            // next button - always rendered (disabled when on last page)
            if (curentPage < totalPage)
            {
                int next = curentPage + 1;
                if (!string.IsNullOrEmpty(function))
                    sb.Append($"<li class=\"{baseLiClass}\"><a role=\"button\" tabindex=\"0\" aria-pressed=\"false\" rel='nofollow' onclick=\"{function}({next})\" class=\"page-item\" data-href=\"{url}?page={next}{q}\">&raquo;</a></li>");
                else
                    sb.Append($"<li class=\"{baseLiClass}\"><a role=\"button\" tabindex=\"0\" aria-pressed=\"false\" rel='nofollow' class=\"page-item\" data-href=\"{url}?page={next}{q}\">&raquo;</a></li>");
            }
            else
            {
                sb.Append($"<li class=\"{baseLiClass} disabled\"><span class=\"page-item disabled\">&raquo;</span></li>");
            }

            return sb.ToString();
        }

        public static string GenPageListAjax(int curentPage, int totalRow, int size, string function, string forcus = null)
        {
            var str = new StringBuilder();
            int totalPage = (totalRow % size > 0) ? (totalRow / size + 1) : (totalRow / size);
            str.Append("<ul class=\"pagination justify-content-center\">");
            for (int i = 1; i <= totalPage; i++)
            {
                str.Append($"<li class=\"page-item {(i == curentPage ? "active" : "")}\"><a {(forcus == null ? "" : $"href=\"{forcus}\"")} onclick = \"{function}({i})\" class=\"page-link\">{i}</a></li>");
            }
            str.Append("</ul>");
            return str.ToString();
        }

        public static string GenContent(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                {
                    return data;
                }
                var splData = data.Split("\n");
                var newData = "";
                newData += "<ul>";
                foreach (var item in splData)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        newData += $"<li>{item}</li>";
                    }
                }
                newData += "</ul>";
                return newData;
            }
            catch
            {
                return data;
            }
        }

        public static string ToUrlSlug(string str)
        {
            string ChuoiMoi = "";
            string[] mang = str.Split(' ', '/', ',', '*', '-');
            for (int i = 0; i < mang.Count(); i++)
            {
                if (i == mang.Count() - 1)
                {
                    ChuoiMoi += mang[i];
                }
                else
                {
                    ChuoiMoi += mang[i] + "-";
                }
            }

            for (int i = 1; i < vietNamChar.Length; i++)
            {
                for (int j = 0; j < vietNamChar[i].Length; j++)
                    ChuoiMoi = ChuoiMoi.Replace(vietNamChar[i][j], vietNamChar[0][i - 1]);
            }
            return ChuoiMoi.ToLower();
        }

        private static readonly string[] vietNamChar = new string[]
        {
 "aAeEoOuUiIdDyY",
 "áàṭảãâấầuậẩmẫăắằặẳẵ",
 "ÁẠ̀ẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
 "éèẹẻẽêếềệểễ",
 "ÉÈẸẺẼÊẾỜệỂỄ",
 "óòọỏõôốồộổỗơớờợởỡ",
 "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỬ",
 "úùụủũưứừựửूस",
 "ÚÙỤỦŨƯỨỪỰỬỮ",
 "íìịỉĩ",
 "ÍÌỊỈĨ",
 "đ",
 "Đ",
 "ýỳỵỷỹ",
 "ÝỲỴỶỸ"
        };
    }
}
