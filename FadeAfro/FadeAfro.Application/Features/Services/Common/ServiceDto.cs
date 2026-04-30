namespace FadeAfro.Application.Features.Services.Common;

public record ServiceDto(
    Guid Id,
    string Name,
    string? Description,
    int Price,
    TimeSpan Duration);