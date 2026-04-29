using MediatR;

namespace FadeAfro.Application.Features.MasterUnavailabilities.AddUnavailability;

public record AddUnavailabilityCommand(
    Guid MasterId,
    DateOnly Date) : IRequest;
