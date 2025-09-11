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
                string[] allowedExtensions = _baseSettings.ImagesType.Split(',');
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
            using var image = System.Drawing.Image.FromStream(file.OpenReadStream());
            var img = ResizeImage(Path.GetFileName(file.FileName), image, maxSideSize, makeItSquare);
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
            int newWidth;
            int newHeight;

            int oldWidth = image.Width;
            int oldHeight = image.Height;
            Bitmap newImage;
            if (makeItSquare)
            {
                int smallerSide = oldWidth >= oldHeight ? oldHeight : oldWidth;
                double coeficient = maxSideSize / (double)smallerSide;
                newWidth = Convert.ToInt32(coeficient * oldWidth);
                newHeight = Convert.ToInt32(coeficient * oldHeight);
                Bitmap tempImage = new Bitmap(image, newWidth, newHeight);
                int cropX = (newWidth - maxSideSize) / 2;
                int cropY = (newHeight - maxSideSize) / 2;
                newImage = new Bitmap(maxSideSize, maxSideSize);
                Graphics tempGraphic = Graphics.FromImage(newImage);
                tempGraphic.SmoothingMode = SmoothingMode.AntiAlias;
                tempGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                tempGraphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                tempGraphic.DrawImage(tempImage, new Rectangle(0, 0, maxSideSize, maxSideSize), cropX, cropY, maxSideSize, maxSideSize, GraphicsUnit.Pixel);
            }
            else
            {
                int maxSide = oldWidth >= oldHeight ? oldWidth : oldHeight;
                if (maxSide > maxSideSize)
                {
                    double coeficient = maxSideSize / (double)maxSide;
                    newWidth = Convert.ToInt32(coeficient * oldWidth);
                    newHeight = Convert.ToInt32(coeficient * oldHeight);
                }
                else
                {
                    newWidth = oldWidth;
                    newHeight = oldHeight;
                }
                newImage = new Bitmap(image, newWidth, newHeight);
            }
            return newImage;
        }

    }
}