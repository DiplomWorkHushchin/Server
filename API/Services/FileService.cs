using API.Entities;
using API.Interfaces;

namespace API.Services;

public class FileService(IWebHostEnvironment _env) : IFileService
{
    public void DeleteFile(string filePath)
    {
        var uploadsPath = Path.Combine(_env.WebRootPath, "UploadsUserPictures");
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        if (!string.IsNullOrEmpty(filePath))
        {
            var oldPhotoPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
            if (System.IO.File.Exists(oldPhotoPath))
            {
                System.IO.File.Delete(oldPhotoPath);
            }
        }
    }

    public async Task<string> SaveFileAsync(IFormFile file, string targetFolder)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file uploaded.");

        var uploadsPath = Path.Combine(_env.WebRootPath, targetFolder);

        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/{targetFolder}/{fileName}";
    }
}
