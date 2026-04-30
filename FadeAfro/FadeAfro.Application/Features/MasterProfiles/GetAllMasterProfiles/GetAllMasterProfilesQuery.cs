using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetAllMasterProfiles;

public record GetAllMasterProfilesQuery : IRequest<GetAllMasterProfilesResponse>;
