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

        public static decimal GetFullPrice(decimal price)
        {
            return (price * 5 / 4);
        }

        public static string SubStringTitle(string input,int num)
        {
            if(string.IsNullOrEmpty(input))
            {
                return "";
            }
            else if(input.Length <=num)
            {
                return input;
            }
            else
            {
                return input.Substring(0,num - 1);
            }
        }
        public static List<string> StringToListItem(string input,string sp=",")
        {
            try
            {
                if(string.IsNullOrEmpty(input))
                {
                    return new List<string>();
                }
                return input.Split(sp).Where(x=>!string.IsNullOrEmpty(x)).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }
        
        public static string FormatMoney(double input)
        {
            if(input>=1000)
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
            var REGEX_TAGS = new Regex(@">\s+<", RegexOptions.Compiled);
            var REGEX_ALL = new Regex(@"\s+|\t\s+|\n\s+|\r\s+", RegexOptions.Compiled);
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

        public static string SContent(string content)
        {
            if(!string.IsNullOrEmpty(content))
            {
                string newString = Regex.Replace(content, "<.*?>", String.Empty);
                newString = newString.Replace("https://", String.Empty).Replace("http://", String.Empty).Replace("//", String.Empty);
                return newString;
            }
            return "";
        }

        public static bool CheckIPValid(string strIP)
        {
            //  Split string by ".", check that array length is 3
            char chrFullStop = '.';
            string[] arrOctets = strIP.Split(chrFullStop);
            if (arrOctets.Length != 4)
            {
                return false;
            }
            //  Check each substring checking that the int value is less than 255 and that is char[] length is !> 2
            int MAXVALUE = 255;
            int temp; // Parse returns Int32
            foreach (string strOctet in arrOctets)
            {
                if (strOctet.Length > 3)
                {
                    return false;
                }

                temp = int.Parse(strOctet);
                if (temp > MAXVALUE)
                {
                    return false;
                }
            }
            return true;
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
            return (page - 1)* size + cstt;
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
            if(slug=="")
            {
                return $"/{language}";
            }
            return $"/{slug}.html";
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
        
        public static string GetHours(string from,string to)
        {
            if (from == null || to == null) return "04:00";
            if(to=="00:00")
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
                if(!Directory.Exists(Path.GetDirectoryName(mapPath)))
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
            if(!IsFileLocked(new FileInfo(mapPath)))
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
            StringBuilder str = new StringBuilder();
            int totalPage = (totalRow % size > 0) ? (totalRow / size + 1) : (totalRow / size);
            for (int i =1;i <= totalPage;i++)
            {
                str.Append($"<li class=\"{liClass} {(i == curentPage ? classActive : "")}\"><a rel='nofollow' class=\"page-link\" href=\"{url}?page={i}{(query != null ? $"{query}" : "")}\">{i}</a></li>");
            }
            return str.ToString();
        }

        public static string GenPageListAjax(int curentPage, int totalRow, int size, string function, string forcus = null)
        {
            var str = new StringBuilder();
            int totalPage = (totalRow % size > 0) ? (totalRow / size + 1) : (totalRow / size);
            str.Append("<ul class=\"pagination justify-content-center\">");
            for (int i = 1; i <= totalPage; i++)
            {
                str.Append($"<li class=\"page-item {(i == curentPage ? "active" : "")}\"><a { (forcus == null ? "": $"href=\"{forcus}\"") } onclick = \"{function}({i})\" class=\"page-link\">{i}</a></li>");
            }
            str.Append("</ul>");
            return str.ToString();
        }




        public static string GenContent(string data)
        {
            try
            {
                if(string.IsNullOrEmpty(data))
                {
                    return data;
                }
                var splData = data.Split("\n");
                var newData = "";
                newData += "<ul>";
                foreach (var item in splData)
                {
                    if(!string.IsNullOrEmpty(item))
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
            "áàạảãâấầậẩẫăắằặẳẵ",
            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
            "éèẹẻẽêếềệểễ",
            "ÉÈẸẺẼÊẾỀỆỂỄ",
            "óòọỏõôốồộổỗơớờợởỡ",
            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
            "úùụủũưứừựửữ",
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
