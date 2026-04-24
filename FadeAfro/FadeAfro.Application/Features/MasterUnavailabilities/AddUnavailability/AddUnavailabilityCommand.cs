using MediatR;

namespace FadeAfro.Application.Features.MasterUnavailabilities.AddUnavailability;

public record AddUnavailabilityCommand(
    Guid MasterProfileId,
    DateOnly Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime) : IRequest<AddUnavailabilityResponse>;
