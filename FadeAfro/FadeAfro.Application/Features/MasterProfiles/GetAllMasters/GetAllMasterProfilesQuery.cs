using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetAllMasters;

public record GetAllMasterProfilesQuery : IRequest<GetAllMasterProfilesResponse>;
