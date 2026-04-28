using MediatR;

namespace FadeAfro.Application.Features.Services.DeleteService;

public record DeleteServiceCommand(
    Guid MasterId,
    Guid ServiceId) : IRequest<Unit>;
