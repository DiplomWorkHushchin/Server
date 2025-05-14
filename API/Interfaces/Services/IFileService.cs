namespace API.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string targetFolder);
    void DeleteFile(string filePath);
}
