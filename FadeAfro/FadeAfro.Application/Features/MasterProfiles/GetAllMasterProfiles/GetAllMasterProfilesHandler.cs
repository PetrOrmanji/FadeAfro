using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetAllMasterProfiles;

public class GetAllMasterProfilesHandler : IRequestHandler<GetAllMasterProfilesQuery, GetAllMasterProfilesResponse>
{
    private readonly IMasterProfileRepository _masterProfileRepository;

    public GetAllMasterProfilesHandler(IMasterProfileRepository masterProfileRepository)
    {
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<GetAllMasterProfilesResponse> Handle(GetAllMasterProfilesQuery query, CancellationToken cancellationToken)
    {
        var masterProfiles = await _masterProfileRepository.GetAllAsync();

        var masters = masterProfiles
            .Select(mp => new MasterProfileResponse(
                mp.Id,
                mp.MasterId,
                mp.Master.FirstName,
                mp.Master.LastName,
                mp.PhotoUrl,
                mp.Description))
            .ToList();

        return new GetAllMasterProfilesResponse(masters);
    }
}
