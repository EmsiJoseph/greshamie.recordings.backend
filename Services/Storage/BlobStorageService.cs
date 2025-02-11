using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;

namespace backend.Services.Storage
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageService(IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("AzureBlobStorage");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Azure Blob Storage connection string is missing.");
            }

            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string containerName, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, overwrite: true);

            return blobClient.Uri.ToString(); // Return the Blob URL
        }
        
        public async Task<string> StreamingUrlAsync(string containerName, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
    
            // Optional: Check if the blob exists.
            if (!await blobClient.ExistsAsync())
            {
                throw new Exception("Blob does not exist.");
            }
    
            // Create a SAS token that grants read permissions.
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = fileName,
                Resource = "b", // 'b' indicates the resource is a blob.
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
                
                ContentType = "audio/mpeg",
                ContentDisposition = "inline"  // inline forces the browser to stream/display instead of downloading.
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);
    
            // Generate the full URI with SAS token.
            Uri streamingSasUri = blobClient.GenerateSasUri(sasBuilder);
            return streamingSasUri.ToString();
        }
        
        public async Task DeleteFileAsync(string containerName, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<Stream?> DownloadFileAsync(string containerName, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            if (!await blobClient.ExistsAsync())
            {
                return null;
            }

            var downloadInfo = await blobClient.DownloadAsync();
            return downloadInfo.Value.Content;
        }
    }
}
