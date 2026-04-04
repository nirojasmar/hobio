namespace hobio.api.Services;

public interface IStorageService
{
    Task<string> GetSignedDownloadUrlAsync(string objectName, TimeSpan duration);
}
