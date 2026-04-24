using MediatR;

namespace FadeAfro.Application.Features.MasterUnavailabilities.DeleteUnavailability;

public record DeleteUnavailabilityCommand(Guid UnavailabilityId) : IRequest<Unit>;
