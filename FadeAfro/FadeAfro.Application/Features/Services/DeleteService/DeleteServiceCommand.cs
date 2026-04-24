using MediatR;

namespace FadeAfro.Application.Features.Services.DeleteService;

public record DeleteServiceCommand(Guid ServiceId) : IRequest<Unit>;
