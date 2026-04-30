using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.MasterUnavailability;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterUnavailabilities.AddMasterUnavailability;

public class AddMasterUnavailabilityHandler : IRequestHandler<AddMasterUnavailabilityCommand>
{
    private readonly IMasterUnavailabilityRepository _unavailabilityRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public AddMasterUnavailabilityHandler(IMasterUnavailabilityRepository unavailabilityRepository, IMasterProfileRepository masterProfileRepository)
    {
        _unavailabilityRepository = unavailabilityRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task Handle(AddMasterUnavailabilityCommand command, CancellationToken cancellationToken)
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
    }
}
