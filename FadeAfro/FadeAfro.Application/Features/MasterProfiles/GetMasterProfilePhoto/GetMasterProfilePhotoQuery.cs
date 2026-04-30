using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetMasterProfilePhoto;

public record GetMasterProfilePhotoQuery(Guid MasterProfileId) : IRequest<GetMasterProfilePhotoResponse>;
