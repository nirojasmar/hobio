using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;

namespace hobio.api.Services;

public class StorageService : IStorageService
{
    private readonly string _bucketName;
    private readonly Google.Apis.Auth.OAuth2.GoogleCredential _credential;

    public StorageService(IConfiguration configuration, Google.Apis.Auth.OAuth2.GoogleCredential credential)
    {
        _credential = credential;
        _bucketName = configuration["REPORTS_BUCKET_NAME"] ?? throw new ArgumentNullException("REPORTS_BUCKET_NAME not set");
    }

    public async Task<string> GetSignedDownloadUrlAsync(string fileName, TimeSpan duration)
    {
        var signer = UrlSigner.FromCredential(_credential);
        return await signer.SignAsync(
            _bucketName,
            fileName,
            duration,
            HttpMethod.Get);
    }
}
