using FadeAfro.Application.Features.MasterProfiles.Common;
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
        
        var masterProfileDtos = new List<MasterProfileDto>();

        foreach (var masterProfile in masterProfiles)
        {
            var masterProfileDto = new MasterProfileDto(
                masterProfile.Id,
                masterProfile.Master.FirstName,
                masterProfile.Master.LastName,
                masterProfile.PhotoUrl,
                masterProfile.Description);
            
            masterProfileDtos.Add(masterProfileDto);
        }
        
        return new GetAllMasterProfilesResponse(masterProfileDtos);
    }
}
