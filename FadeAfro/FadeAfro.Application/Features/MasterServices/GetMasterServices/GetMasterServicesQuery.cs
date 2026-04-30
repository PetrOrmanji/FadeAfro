using MediatR;

namespace FadeAfro.Application.Features.MasterServices.GetMasterServices;

public record GetMasterServicesQuery(Guid MasterProfileId) : IRequest<GetMasterServicesResponse>;
