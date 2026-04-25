using FadeAfro.Application.Services;
using FadeAfro.Domain.Exceptions.File;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.UploadMasterPhoto;

public class UploadMasterPhotoHandler : IRequestHandler<UploadMasterPhotoCommand>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IFileStorageService _fileStorageService;

    public UploadMasterPhotoHandler(
        IMasterProfileRepository masterProfileRepository,
        IFileStorageService fileStorageService)
    {
        _masterProfileRepository = masterProfileRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task Handle(UploadMasterPhotoCommand command, CancellationToken cancellationToken)
    {
        if (!AllowedExtensions.Contains(command.Extension.ToLower()))
            throw new InvalidFileException("Допустимые форматы: JPEG, PNG, WebP.");

        if (command.FileSize > MaxFileSizeBytes)
            throw new InvalidFileException("Размер файла не должен превышать 5 МБ.");

        var masterProfile = await _masterProfileRepository.GetByIdAsync(command.MasterProfileId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var photoUrl = await _fileStorageService.SaveMasterPhotoAsync(
            command.MasterProfileId,
            command.FileStream,
            command.Extension);

        masterProfile.Update(photoUrl, masterProfile.Description);
        await _masterProfileRepository.UpdateAsync(masterProfile);
    }
}
