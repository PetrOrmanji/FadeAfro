using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterUnavailabilities.AddUnavailability;

public class AddUnavailabilityHandler : IRequestHandler<AddUnavailabilityCommand, AddUnavailabilityResponse>
{
    private readonly IMasterUnavailabilityRepository _unavailabilityRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public AddUnavailabilityHandler(IMasterUnavailabilityRepository unavailabilityRepository, IMasterProfileRepository masterProfileRepository)
    {
        _unavailabilityRepository = unavailabilityRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<AddUnavailabilityResponse> Handle(AddUnavailabilityCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(command.MasterProfileId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var unavailability = new MasterUnavailability(
            command.MasterProfileId,
            command.Date,
            command.StartTime,
            command.EndTime);

        await _unavailabilityRepository.AddAsync(unavailability);

        return new AddUnavailabilityResponse(unavailability.Id);
    }
}
