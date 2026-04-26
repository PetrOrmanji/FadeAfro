namespace FadeAfro.Application.Services;

public interface IFileStorageService
{
    Task<string> SaveMasterPhotoAsync(Guid masterProfileId, Stream stream, string extension);
    void DeleteMasterPhoto(Guid masterProfileId);
}
