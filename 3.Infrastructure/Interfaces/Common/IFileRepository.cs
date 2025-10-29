using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using PT.Domain.Model;

namespace PT.Infrastructure.Interfaces
{
    public interface IFileRepository
    {
        //Task ResizeImageAsync(IFormFile file, string pathFile, ImageFormat imgType, int width, int height = 0);
        void SettingsUpdate(string map, object a);
        bool DeleteFile(string path);
        Task<ResponseModel> UploadFile(IFormFileCollection files, BaseSettings _baseSettings, string webPath, string folder);
        string ResizeImage(IFormFile file, string pathSave, int maxSideSize, bool makeItSquare);
    }
}
