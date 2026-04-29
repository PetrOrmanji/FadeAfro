using MediatR;

namespace FadeAfro.Application.Features.MasterUnavailabilities.DeleteUnavailability;

public record DeleteUnavailabilityCommand(
    Guid MasterId,
    Guid UnavailabilityId) : IRequest;
