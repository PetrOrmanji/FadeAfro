using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetMasterPhoto;

public record GetMasterPhotoQuery(Guid MasterProfileId) : IRequest<GetMasterPhotoResponse>;
