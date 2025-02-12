namespace backend.Services.Storage;

public interface IBlobStorageService
{
    /// <summary>
    /// Uploads a file to Azure Blob Storage.
    /// This is used to store recording files in the cloud.
    /// </summary>
    Task<string> UploadFileAsync(Stream fileStream, string containerName, string fileName);

    /// <summary>
    /// Gets a temporary URL for streaming a file.
    /// The URL will work for 1 hour and is configured for audio streaming.
    /// </summary>
    Task<string> StreamingUrlAsync(string containerName, string fileName);
}
