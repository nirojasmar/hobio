namespace hobio.api.Services;

public interface IStorageService
{
    Task<string> GetSignedDownloadUrlAsync(string fileName, TimeSpan duration);
}
