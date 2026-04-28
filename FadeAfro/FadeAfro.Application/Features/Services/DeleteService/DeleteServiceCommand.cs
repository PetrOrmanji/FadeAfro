using MediatR;

namespace FadeAfro.Application.Features.Services.DeleteService;

public record DeleteServiceCommand(
    Guid UserId,
    Guid ServiceId) : IRequest<Unit>;
