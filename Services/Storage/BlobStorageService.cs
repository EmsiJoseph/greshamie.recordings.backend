using System.IO;
using System.Threading.Tasks;

namespace backend.Services.Storage
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string containerName, string fileName);
        Task DeleteFileAsync(string containerName, string fileName);
        Task<Stream?> DownloadFileAsync(string containerName, string fileName);
    }
}