using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetMasterProfileDayAvailability;

public record GetMasterProfileDayAvailabilityQuery(Guid MasterProfileId, DateOnly Date, TimeSpan ServiceDuration)
    : IRequest<GetMasterProfileDayAvailabilityResponse>;