using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetMasterProfile;

public record GetMasterProfileQuery(Guid MasterProfileId) : IRequest<GetMasterProfileResponse>;
