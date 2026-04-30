using MediatR;

namespace FadeAfro.Application.Features.MasterUnavailabilities.AddMasterUnavailability;

public record AddMasterUnavailabilityCommand(
    Guid MasterId,
    DateOnly Date) : IRequest;
