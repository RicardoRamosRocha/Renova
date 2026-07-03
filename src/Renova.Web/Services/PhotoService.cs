namespace Renova.Web.Services;

public sealed class PhotoService : IPhotoService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string[] _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSize = 5 * 1024 * 1024;

    public PhotoService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> SavePhotoAsync(IFormFile? photo, string folderName, string? oldPhotoPath = null)
    {
        if (photo is null || photo.Length == 0)
        {
            return oldPhotoPath;
        }

        if (photo.Length > MaxFileSize)
        {
            throw new InvalidOperationException("A imagem deve ter no máximo 5 MB.");
        }

        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

        if (!_allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Formato inválido. Use JPG, JPEG, PNG ou WEBP.");
        }

        DeletePhoto(oldPhotoPath);

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folderName);
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await photo.CopyToAsync(stream);

        return $"/uploads/{folderName}/{fileName}";
    }

    public void DeletePhoto(string? photoPath)
    {
        if (string.IsNullOrWhiteSpace(photoPath))
        {
            return;
        }

        var relativePath = photoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}