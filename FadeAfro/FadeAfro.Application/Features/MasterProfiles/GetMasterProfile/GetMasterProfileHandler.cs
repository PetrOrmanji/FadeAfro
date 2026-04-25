using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetMasterProfile;

public class GetMasterProfileHandler : IRequestHandler<GetMasterProfileQuery, GetMasterProfileResponse>
{
    private readonly IMasterProfileRepository _masterProfileRepository;

    public GetMasterProfileHandler(IMasterProfileRepository masterProfileRepository)
    {
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<GetMasterProfileResponse> Handle(GetMasterProfileQuery query, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(query.MasterProfileId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        return new GetMasterProfileResponse(
            masterProfile.Id,
            masterProfile.MasterId,
            masterProfile.Master.FirstName,
            masterProfile.Master.LastName,
            masterProfile.PhotoUrl,
            masterProfile.Description);
    }
}
