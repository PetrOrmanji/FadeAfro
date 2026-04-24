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

        var unavailabilities = await _unavailabilityRepository.GetByMasterProfileIdAsync(query.MasterProfileId);

        var response = unavailabilities
            .Select(u => new UnavailabilityResponse(
                u.Id,
                u.Date,
                u.StartTime,
                u.EndTime))
            .ToList();

        return new GetMasterUnavailabilitiesResponse(response);
    }
}
