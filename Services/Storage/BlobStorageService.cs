using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Storage;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _config;
    public BlobStorageService(IConfiguration configuration, ApplicationDbContext dbContext, IConfiguration config)
    {
        _dbContext = dbContext;
        _config = config;
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

    public async Task<string> StreamingUrlAsync(string? containerName, string fileName)
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
            ContentDisposition = "inline" // inline forces the browser to stream/display instead of downloading.
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        // Generate the full URI with SAS token.
        Uri streamingSasUri = blobClient.GenerateSasUri(sasBuilder);
        return streamingSasUri.ToString();
    }
    
    public async Task<string> UpdateStreamingUrlAsync(string recordingId)
    {
        // Retrieve the SyncedRecording record from the database.
        var record = await _dbContext.SyncedRecordings.FirstOrDefaultAsync(r => r.Id == recordingId);
        if (record == null)
        {
            throw new Exception("Recording not found in DB.");
        }
        var fileName = $"{record.RecordingDate:yyyy/MM/dd}/{record.Id}.mp3";
        var containerClient = _blobServiceClient.GetBlobContainerClient(_config["BlobStorage:ContainerName"]);
        
        // Generate a new SAS URL.
        string newSasUrl = await StreamingUrlAsync(_config["BlobStorage:ContainerName"], fileName);
        
        // Update the record with the new SAS URL.
        record.StreamingUrl = newSasUrl;
        _dbContext.Update(record);
        await _dbContext.SaveChangesAsync();
        return newSasUrl;
    }
}