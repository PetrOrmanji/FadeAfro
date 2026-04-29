using FadeAfro.Application.Features.MasterUnavailabilities.Common;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterUnavailabilities.GetMasterUnavailabilities;

public class GetMasterUnavailabilitiesHandler : IRequestHandler<GetMasterUnavailabilitiesQuery, GetMasterUnavailabilitiesResponse>
{
    private readonly IMasterUnavailabilityRepository _unavailabilityRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public GetMasterUnavailabilitiesHandler(IMasterUnavailabilityRepository unavailabilityRepository, IMasterProfileRepository masterProfileRepository)
    {
        _unavailabilityRepository = unavailabilityRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<GetMasterUnavailabilitiesResponse> Handle(GetMasterUnavailabilitiesQuery query, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(query.MasterProfileId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var unavailabilities = 
            await _unavailabilityRepository.GetByMasterProfileIdAsync(query.MasterProfileId);
        
        var unavailabilityDtos = new List<UnavailabilityDto>();

        foreach (var unavailability in unavailabilities)
        {
            var unavailabilityDto = new UnavailabilityDto(
                unavailability.Id,
                unavailability.Date);
            
            unavailabilityDtos.Add(unavailabilityDto);
        }

        return new GetMasterUnavailabilitiesResponse(unavailabilityDtos);
    }
}
