namespace FadeAfro.Application.Features.Services.GetMasterServices;

public record ServiceResponse(
    Guid Id,
    string Name,
    string? Description,
    int Price,
    TimeSpan Duration);

public record GetMasterServicesResponse(List<ServiceResponse> Services);
