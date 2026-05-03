using FadeAfro.Application.Features.MasterProfiles.GetMasterProfile;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetMyMasterProfile;

public class GetMyMasterProfileHandler : IRequestHandler<GetMyMasterProfileQuery, GetMasterProfileResponse>
{
    private readonly IMasterProfileRepository _masterProfileRepository;

    public GetMyMasterProfileHandler(IMasterProfileRepository masterProfileRepository)
    {
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<GetMasterProfileResponse> Handle(GetMyMasterProfileQuery query, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(query.UserId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        return new GetMasterProfileResponse(
            masterProfile.Id,
            masterProfile.MasterId,
            masterProfile.Master.FirstName,
            masterProfile.Master.LastName,
            masterProfile.PhotoUrl);
    }
}
