using MediatR;

namespace FadeAfro.Application.Features.MasterUnavailabilities.GetMasterUnavailabilities;

public record GetMasterUnavailabilitiesQuery(Guid MasterProfileId) : IRequest<GetMasterUnavailabilitiesResponse>;
