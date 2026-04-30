namespace FadeAfro.Application.Features.MasterServices.AddMasterService;

public record AddMasterServiceRequest(
    string Name,
    string? Description,
    int Price,
    TimeSpan Duration);