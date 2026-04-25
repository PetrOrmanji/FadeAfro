using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetAvailableSlots;

public record GetAvailableSlotsQuery(
    Guid MasterProfileId,
    Guid ServiceId,
    DateOnly Date) : IRequest<GetAvailableSlotsResponse>;
