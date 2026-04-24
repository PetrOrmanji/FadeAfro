using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetAllMasters;

public record GetAllMastersQuery : IRequest<GetAllMastersResponse>;
