using FadeAfro.Application.Services;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetMasterProfilePhoto;

public class GetMasterProfilePhotoHandler : IRequestHandler<GetMasterProfilePhotoQuery, GetMasterProfilePhotoResponse>
{
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IFileStorageService _fileStorageService;

    public GetMasterProfilePhotoHandler(
        IMasterProfileRepository masterProfileRepository,
        IFileStorageService fileStorageService)
    {
        _masterProfileRepository = masterProfileRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<GetMasterProfilePhotoResponse> Handle(GetMasterProfilePhotoQuery query, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(query.MasterProfileId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var result = _fileStorageService.GetMasterPhoto(query.MasterProfileId);

        if (result is null)
            throw new MasterProfilePhotoNotFoundException();

        return new GetMasterProfilePhotoResponse(result.Value.Stream, result.Value.ContentType);
    }
}
