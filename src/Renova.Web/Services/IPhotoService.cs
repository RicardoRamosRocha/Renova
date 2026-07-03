using Microsoft.AspNetCore.Http;

namespace Renova.Web.Services;

public interface IPhotoService
{
    Task<string?> SavePhotoAsync(IFormFile? photo, string folderName, string? oldPhotoPath = null);
    void DeletePhoto(string? photoPath);
}