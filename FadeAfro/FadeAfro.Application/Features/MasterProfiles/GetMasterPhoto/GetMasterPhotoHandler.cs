using FadeAfro.Application.Services;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetMasterPhoto;

public class GetMasterPhotoHandler : IRequestHandler<GetMasterPhotoQuery, GetMasterPhotoResponse>
{
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IFileStorageService _fileStorageService;

    public GetMasterPhotoHandler(
        IMasterProfileRepository masterProfileRepository,
        IFileStorageService fileStorageService)
    {
        _masterProfileRepository = masterProfileRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<GetMasterPhotoResponse> Handle(GetMasterPhotoQuery query, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(query.MasterProfileId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var result = _fileStorageService.GetMasterPhoto(query.MasterProfileId);

        if (result is null)
            throw new MasterProfilePhotoNotFoundException();

        return new GetMasterPhotoResponse(result.Value.Stream, result.Value.ContentType);
    }
}
