using FadeAfro.Application.Features.MasterProfiles.GetMasterProfile;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetMyMasterProfile;

public record GetMyMasterProfileQuery(Guid UserId) : IRequest<GetMasterProfileResponse>;
