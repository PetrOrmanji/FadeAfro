using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetMasterAvailability;

public record GetMasterAvailabilityQuery(
    Guid MasterProfileId,
    Guid ServiceId) : IRequest<GetMasterAvailabilityResponse>;
