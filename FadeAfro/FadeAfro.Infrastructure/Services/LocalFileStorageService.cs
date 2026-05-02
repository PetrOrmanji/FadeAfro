using FadeAfro.Application.Services;
using Microsoft.AspNetCore.StaticFiles;

namespace FadeAfro.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private const string MastersDirectory = "uploads/masters";

    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    public (Stream Stream, string ContentType)? GetMasterPhoto(Guid masterProfileId)
    {
        if (!Directory.Exists(MastersDirectory)) return null;

        var files = Directory.GetFiles(MastersDirectory, $"{masterProfileId}.*");
        if (files.Length == 0) return null;

        var filePath = files[0];
        _contentTypeProvider.TryGetContentType(filePath, out var contentType);

        return (new FileStream(filePath, FileMode.Open), contentType!);
    }
    
    public async Task<string> SaveMasterPhotoAsync(Guid masterProfileId, Stream stream, string extension)
    {
        if (!Directory.Exists(MastersDirectory))
            Directory.CreateDirectory(MastersDirectory);

        foreach (var old in Directory.GetFiles(MastersDirectory, $"{masterProfileId}.*"))
            File.Delete(old);

        var filePath = GetPhotoPath(masterProfileId, extension);
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fileStream);

        return $"/api/master-profiles/get/{masterProfileId}/photo";
    }

    public void DeleteMasterPhoto(Guid masterProfileId)
    {
        if (!Directory.Exists(MastersDirectory)) return;

        foreach (var file in Directory.GetFiles(MastersDirectory, $"{masterProfileId}.*"))
            File.Delete(file);
    }

    private static string GetPhotoPath(Guid masterProfileId, string extension) =>
        Path.Combine(MastersDirectory, $"{masterProfileId}{extension}");
}
