using Microsoft.AspNetCore.Http;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using PT.Infrastructure.Interfaces;
using PT.Domain.Model;
using System.Net.Http.Headers;
using System.Linq;
using System.Drawing.Drawing2D;

namespace PT.Infrastructure.Repositories
{

    public class FileRepository : IFileRepository
    {
        //public virtual async Task ResizeImageAsync(IFormFile file, string pathFile, ImageFormat imgType, int width, int height = 0)
        //{

        //    int newWidth;
        //    int newHeight;
        //    Image image = Image.FromStream(file.OpenReadStream());
        //    int BaseWidth = image.Width;
        //    int BaseHeight = image.Height;
        //    if (BaseWidth > width && width > 0)
        //    {
        //        var typeSave = imgType;
        //        string FileType = Path.GetExtension(pathFile).ToLower();

        //        double dblCoef = (double)width / (double)BaseWidth;
        //        if (height > 0)
        //        {
        //            newWidth = width;
        //            newHeight = height;
        //        }
        //        else
        //        {
        //            newWidth = Convert.ToInt32(dblCoef * BaseWidth);
        //            newHeight = Convert.ToInt32(dblCoef * BaseHeight);
        //        }

        //        Image ReducedImage;
        //        Image.GetThumbnailImageAbort callb = new Image.GetThumbnailImageAbort(ThumbnailCallback);
        //        ReducedImage = image.GetThumbnailImage(newWidth, newHeight, callb, IntPtr.Zero);
        //        ReducedImage.Save(pathFile, typeSave);
        //    }
        //    else
        //    {
        //        using (FileStream fs = File.Create(pathFile))
        //        {
        //            await file.CopyToAsync(fs);
        //            fs.Flush();
        //        }
        //    }
        //}
        public bool ThumbnailCallback()
        {
            return false;
        }
        public void SettingsUpdate(string map, object a)
        {
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(a, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(map, output);
        }
        public bool DeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ResponseModel> UploadFile(IFormFileCollection files, BaseSettings _baseSettings, string webPath, string folder)
        {
            try
            {
                string[] allowedExtensions = (_baseSettings.ImagesType + ",.webp").Split(',');
                string path = $"{webPath}{folder}";
                string pathServer = folder;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                foreach (var file in files)
                {
                    if (!allowedExtensions.Contains(Path.GetExtension(file.FileName)))
                    {
                        return new ResponseModel() { Output = 2, Message = "Hình ảnh tải lên không đúng định dạng.", Type = ResponseTypeMessage.Warning, Data = "" };
                    }
                    else if (_baseSettings.ImagesMaxSize < file.Length)
                    {
                        return new ResponseModel() { Output = 3, Message = "Hình ảnh tải lên vượt quá kích thước cho phép.", Type = ResponseTypeMessage.Warning, Data = "" };
                    }
                    else
                    {
                        var newFilename = Path.GetFileName(file.FileName);
                        if (System.IO.File.Exists(path + file.Name))
                        {
                            newFilename = DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + Path.GetFileName(file.FileName);
                        }

                        string pathFile = ContentDispositionHeaderValue
                        .Parse(file.ContentDisposition)
                        .FileName
                        .Trim('"');

                        pathFile = $"{path}{newFilename}";
                        pathServer = $"{pathServer}{newFilename}";
                        using (var stream = new FileStream(pathFile, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }
                return new ResponseModel() { Output = 1, Message = "Tải ảnh lên thành công.", Type = ResponseTypeMessage.Success, Data = pathServer, IsClosePopup = false };
            }
            catch (Exception)
            {
                return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
            }
        }

        public string ResizeImage(IFormFile file, string pathSave, int maxSideSize, bool makeItSquare)
        {
            // Mở ảnh từ stream
            using var image = System.Drawing.Image.FromStream(file.OpenReadStream());

            // Nếu không cần resize (kích thước đã nhỏ hơn hoặc bằng yêu cầu) thì ghi file gốc
            if (!makeItSquare)
            {
                // Nếu ảnh ngang: kiểm tra theo chiều rộng
                if (image.Width > image.Height)
                {
                    if (image.Width <= maxSideSize)
                    {
                        // Lưu nguyên bản
                        using var outStream = new FileStream(pathSave, FileMode.Create);
                        file.OpenReadStream().CopyTo(outStream);
                        return pathSave;
                    }
                }
                else // ảnh dọc hoặc vuông: kiểm tra theo chiều cao
                {
                    if (image.Height <= maxSideSize)
                    {
                        using var outStream = new FileStream(pathSave, FileMode.Create);
                        file.OpenReadStream().CopyTo(outStream);
                        return pathSave;
                    }
                }
            }

            // Thực hiện resize (bao gồm cả trường hợp makeItSquare)
            using var img = ResizeImage(Path.GetFileName(file.FileName), image, maxSideSize, makeItSquare);
            var type = GetImageFormat(img);
            img.Save(pathSave, type);
            return pathSave;
        }

        private ImageFormat GetImageFormat(System.Drawing.Image img)
        {
            if (img.RawFormat.Equals(ImageFormat.Png))
                return ImageFormat.Png;
            else if (img.RawFormat.Equals(ImageFormat.Gif))
                return ImageFormat.Gif;
            else if (img.RawFormat.Equals(ImageFormat.Icon))
                return ImageFormat.Icon;
            else
                return ImageFormat.Jpeg;
        }

        private Bitmap ResizeImage(string fileName, System.Drawing.Image image, int maxSideSize, bool makeItSquare)
        {
            int oldWidth = image.Width;
            int oldHeight = image.Height;

            if (makeItSquare)
            {
                // Resize theo tỉ lệ rồi crop để thành square
                int smallerSide = Math.Min(oldWidth, oldHeight);
                double coef = maxSideSize / (double)smallerSide;
                int tempWidth = (int)Math.Round(oldWidth * coef);
                int tempHeight = (int)Math.Round(oldHeight * coef);

                using var tempImage = new Bitmap(tempWidth, tempHeight);
                using (var g = Graphics.FromImage(tempImage))
                {
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(image, 0, 0, tempWidth, tempHeight);
                }

                int cropX = (tempWidth - maxSideSize) / 2;
                int cropY = (tempHeight - maxSideSize) / 2;
                var square = new Bitmap(maxSideSize, maxSideSize);
                using (var g = Graphics.FromImage(square))
                {
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(tempImage, new Rectangle(0, 0, maxSideSize, maxSideSize), cropX, cropY, maxSideSize, maxSideSize, GraphicsUnit.Pixel);
                }

                return square;
            }

            // Không make square: resize theo orientation
            double scale = 1.0;
            if (oldWidth >= oldHeight)
            {
                // landscape or square: constrain by width
                if (oldWidth > maxSideSize) scale = maxSideSize / (double)oldWidth;
            }
            else
            {
                // portrait: constrain by height
                if (oldHeight > maxSideSize) scale = maxSideSize / (double)oldHeight;
            }

            int newWidth = (int)Math.Round(oldWidth * scale);
            int newHeight = (int)Math.Round(oldHeight * scale);

            // Nếu không cần thay đổi kích thước, trả về một bản sao Bitmap của ảnh gốc
            if (scale >= 1.0)
            {
                return new Bitmap(image);
            }

            var dest = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(dest))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return dest;
        }

    }
}