namespace FadeAfro.Application.Features.Services.AddService;

public record AddServiceRequest(
    string Name,
    string? Description,
    int Price,
    TimeSpan Duration);