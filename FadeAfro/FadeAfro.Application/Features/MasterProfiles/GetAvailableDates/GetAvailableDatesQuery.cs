using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetAvailableDates;

public record GetAvailableDatesQuery(
    Guid MasterProfileId,
    Guid ServiceId,
    int Year,
    int Month) : IRequest<GetAvailableDatesResponse>;
