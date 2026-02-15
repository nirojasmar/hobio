using System.IO;
using System.Threading.Tasks;

namespace hobio.worker.Services;

public interface IFileService
{
    Task WriteAllBytesAsync(string path, byte[] bytes);
}

public class FileService : IFileService
{
    public Task WriteAllBytesAsync(string path, byte[] bytes)
    {
        // Ensure directory exists
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        return File.WriteAllBytesAsync(path, bytes);
    }
}
