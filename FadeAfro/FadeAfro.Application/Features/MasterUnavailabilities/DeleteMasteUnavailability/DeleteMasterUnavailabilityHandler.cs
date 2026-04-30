using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.MasterUnavailability;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterUnavailabilities.DeleteMasteUnavailability;

public class DeleteMasterUnavailabilityHandler : IRequestHandler<DeleteMasterUnavailabilityCommand>
{
    private readonly IMasterUnavailabilityRepository _unavailabilityRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public DeleteMasterUnavailabilityHandler(
        IMasterUnavailabilityRepository unavailabilityRepository,
        IMasterProfileRepository masterProfileRepository)
    {
        _unavailabilityRepository = unavailabilityRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task Handle(DeleteMasterUnavailabilityCommand command, CancellationToken cancellationToken)
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
