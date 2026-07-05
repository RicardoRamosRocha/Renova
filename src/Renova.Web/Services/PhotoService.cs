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
            throw new InvalidOperationException("A imagem deve ter no maximo 5 MB.");
        }

        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

        if (!_allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Formato invalido. Use JPG, JPEG, PNG ou WEBP.");
        }

        if (!await HasValidImageSignatureAsync(photo, extension))
        {
            throw new InvalidOperationException("Arquivo invalido. Envie uma imagem JPG, PNG ou WEBP.");
        }

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folderName);
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await photo.CopyToAsync(stream);

        DeletePhoto(oldPhotoPath);

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

    private static async Task<bool> HasValidImageSignatureAsync(IFormFile photo, string extension)
    {
        var header = new byte[12];
        await using var stream = photo.OpenReadStream();
        var read = await stream.ReadAsync(header);

        return extension switch
        {
            ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => read >= 8 &&
                header[0] == 0x89 &&
                header[1] == 0x50 &&
                header[2] == 0x4E &&
                header[3] == 0x47 &&
                header[4] == 0x0D &&
                header[5] == 0x0A &&
                header[6] == 0x1A &&
                header[7] == 0x0A,
            ".webp" => read >= 12 &&
                header[0] == 0x52 &&
                header[1] == 0x49 &&
                header[2] == 0x46 &&
                header[3] == 0x46 &&
                header[8] == 0x57 &&
                header[9] == 0x45 &&
                header[10] == 0x42 &&
                header[11] == 0x50,
            _ => false
        };
    }
}
