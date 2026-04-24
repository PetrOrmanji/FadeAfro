using FadeAfro.Domain.Exceptions.MasterUnavailability;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterUnavailabilities.DeleteUnavailability;

public class DeleteUnavailabilityHandler : IRequestHandler<DeleteUnavailabilityCommand, Unit>
{
    private readonly IMasterUnavailabilityRepository _unavailabilityRepository;

    public DeleteUnavailabilityHandler(IMasterUnavailabilityRepository unavailabilityRepository)
    {
        _unavailabilityRepository = unavailabilityRepository;
    }

    public async Task<Unit> Handle(DeleteUnavailabilityCommand command, CancellationToken cancellationToken)
    {
        var unavailability = await _unavailabilityRepository.GetByIdAsync(command.UnavailabilityId);

        if (unavailability is null)
            throw new MasterUnavailabilityNotFoundException();

        await _unavailabilityRepository.DeleteAsync(command.UnavailabilityId);

        return Unit.Value;
    }
}
