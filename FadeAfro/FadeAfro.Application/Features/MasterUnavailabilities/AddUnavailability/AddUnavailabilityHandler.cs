using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.MasterUnavailability;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterUnavailabilities.AddUnavailability;

public class AddUnavailabilityHandler : IRequestHandler<AddUnavailabilityCommand, Unit>
{
    private readonly IMasterUnavailabilityRepository _unavailabilityRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public AddUnavailabilityHandler(IMasterUnavailabilityRepository unavailabilityRepository, IMasterProfileRepository masterProfileRepository)
    {
        _unavailabilityRepository = unavailabilityRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<Unit> Handle(AddUnavailabilityCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(command.MasterId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();
        
        var dayUnavailability = await _unavailabilityRepository.GetByMasterProfileIdAndDateAsync(
            masterProfile.Id, command.Date);

        if (dayUnavailability is not null)
            throw new MasterUnavailabilityAlreadyExistsException();

        dayUnavailability = new MasterUnavailability(
            masterProfile.Id,
            command.Date);

        await _unavailabilityRepository.AddAsync(dayUnavailability);
        return Unit.Value;
    }
}
