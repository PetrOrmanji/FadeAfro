using MediatR;

namespace FadeAfro.Application.Features.Services.UpdateService;

public record UpdateServiceCommand(
    Guid ServiceId,
    string Name,
    string? Description,
    int Price,
    TimeSpan Duration) : IRequest<Unit>;
