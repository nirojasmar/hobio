using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;

namespace hobio.worker.Services;

public interface IStorageService
{
    Task UploadFileAsync(byte[] content, string fileName, string contentType);
}

public class GcsService : IStorageService
{
    private readonly string _bucketName;
    private readonly Google.Apis.Auth.OAuth2.GoogleCredential _credential;
    private StorageClient? _storageClient;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public GcsService(IConfiguration configuration, Google.Apis.Auth.OAuth2.GoogleCredential credential)
    {
        _credential = credential;
        _bucketName = configuration["REPORTS_BUCKET_NAME"] ?? throw new ArgumentNullException("REPORTS_BUCKET_NAME not set");
    }

    private async Task<StorageClient> GetClientAsync()
    {
        if (_storageClient != null)
            return _storageClient;

        await _semaphore.WaitAsync();
        try
        {
            if (_storageClient == null)
            {
                _storageClient = await StorageClient.CreateAsync(_credential);
            }
            return _storageClient;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GcsService] Critical failure in StorageClient.CreateAsync: {ex}");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task UploadFileAsync(byte[] content, string fileName, string contentType)
    {
        var client = await GetClientAsync();
        using var stream = new MemoryStream(content);
        await client.UploadObjectAsync(_bucketName, fileName, contentType, stream);
    }
}