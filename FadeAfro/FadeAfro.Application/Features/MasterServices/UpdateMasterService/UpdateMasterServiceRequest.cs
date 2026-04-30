namespace FadeAfro.Application.Features.MasterServices.UpdateMasterService;

public record UpdateMasterServiceRequest(
    string Name,
    string? Description,
    int Price,
    TimeSpan Duration);