using FadeAfro.Application.Features.MasterServices.Common;

namespace FadeAfro.Application.Features.MasterServices.GetMasterServices;

public record GetMasterServicesResponse(List<MasterServiceDto> Services);
