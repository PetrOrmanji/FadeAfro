using FadeAfro.Application.Services;
using Microsoft.AspNetCore.Hosting;

namespace FadeAfro.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadsPath;
    private readonly string _baseUrl;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _uploadsPath = Path.Combine(env.WebRootPath, "uploads", "masters");
        _baseUrl = "/uploads/masters";
    }

    public async Task<string> SaveMasterPhotoAsync(Guid masterProfileId, Stream stream, string extension)
    {
        Directory.CreateDirectory(_uploadsPath);

        // Удаляем старый файл с тем же ID (если было другое расширение)
        foreach (var old in Directory.GetFiles(_uploadsPath, $"{masterProfileId}.*"))
            File.Delete(old);

        var fileName = $"{masterProfileId}{extension}";
        var fullPath = Path.Combine(_uploadsPath, fileName);

        await using var fileStream = File.Create(fullPath);
        await stream.CopyToAsync(fileStream);

        return $"{_baseUrl}/{fileName}";
    }

    public void DeleteMasterPhoto(Guid masterProfileId)
    {
        foreach (var file in Directory.GetFiles(_uploadsPath, $"{masterProfileId}.*"))
            File.Delete(file);
    }
}
