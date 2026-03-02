using Google.Cloud.Storage.V1;

namespace hobio.worker.Services;

public interface IStorageService
{
    Task UploadFileAsync(byte[] content, string fileName, string contentType);
}

public class GcsService : IStorageService
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;

    public GcsService(IConfiguration configuration)
    {
        _storageClient = StorageClient.Create();
        
        _bucketName = configuration["REPORTS_BUCKET_NAME"] ?? throw new ArgumentNullException("REPORTS_BUCKET_NAME not set");
    }

    public async Task UploadFileAsync(byte[] content, string fileName, string contentType)
    {
        using var stream = new MemoryStream(content);
        await _storageClient.UploadObjectAsync(_bucketName, fileName, contentType, stream);
    }
}