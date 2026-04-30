using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.MasterUnavailability;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterUnavailabilities.DeleteUnavailability;

public class DeleteUnavailabilityHandler : IRequestHandler<DeleteUnavailabilityCommand>
{
    private readonly IMasterUnavailabilityRepository _unavailabilityRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public DeleteUnavailabilityHandler(
        IMasterUnavailabilityRepository unavailabilityRepository,
        IMasterProfileRepository masterProfileRepository)
    {
        _unavailabilityRepository = unavailabilityRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task Handle(DeleteUnavailabilityCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(command.MasterId);
        if (masterProfile == null)
            throw new MasterProfileNotFoundException();
        
        var unavailability = await _unavailabilityRepository.GetByIdAsync(command.UnavailabilityId);

        if (unavailability is null)
            throw new MasterUnavailabilityNotFoundException();

        if (unavailability.MasterProfileId != masterProfile.Id)
            throw new UnavailabilityOfAnotherMasterException();

        await _unavailabilityRepository.DeleteAsync(unavailability);
    }
}
