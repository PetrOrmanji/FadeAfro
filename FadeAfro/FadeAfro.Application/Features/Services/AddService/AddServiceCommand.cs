using MediatR;

namespace FadeAfro.Application.Features.Services.AddService;

public record AddServiceCommand(
    Guid MasterId,
    string Name,
    string? Description,
    int Price,
    TimeSpan Duration) : IRequest<Unit>;
