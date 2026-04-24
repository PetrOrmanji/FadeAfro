using MediatR;

namespace FadeAfro.Application.Features.Services.GetMasterServices;

public record GetMasterServicesQuery(Guid MasterProfileId) : IRequest<GetMasterServicesResponse>;
