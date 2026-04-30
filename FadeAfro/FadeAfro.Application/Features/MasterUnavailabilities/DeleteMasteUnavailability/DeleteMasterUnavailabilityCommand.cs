using MediatR;

namespace FadeAfro.Application.Features.MasterUnavailabilities.DeleteMasteUnavailability;

public record DeleteMasterUnavailabilityCommand(
    Guid MasterId,
    Guid UnavailabilityId) : IRequest;
