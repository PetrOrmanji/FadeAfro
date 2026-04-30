namespace FadeAfro.Application.Features.MasterServices.Common;

public record MasterServiceDto(
    Guid Id,
    string Name,
    string? Description,
    int Price,
    TimeSpan Duration);