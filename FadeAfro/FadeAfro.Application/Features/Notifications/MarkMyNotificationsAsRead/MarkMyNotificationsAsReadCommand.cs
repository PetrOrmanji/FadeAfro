using MediatR;

namespace FadeAfro.Application.Features.Notifications.MarkMyNotificationsAsRead;

public record MarkMyNotificationsAsReadCommand(Guid UserId) : IRequest;
