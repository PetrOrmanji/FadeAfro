namespace FadeAfro.Application.Features.Services.UpdateService;

public record UpdateServiceRequest(
    string Name,
    string? Description,
    int Price,
    TimeSpan Duration);