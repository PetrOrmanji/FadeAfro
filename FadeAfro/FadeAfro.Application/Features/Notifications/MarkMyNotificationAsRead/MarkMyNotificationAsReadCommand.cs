using MediatR;

namespace FadeAfro.Application.Features.Notifications.MarkMyNotificationAsRead;

public record MarkMyNotificationAsReadCommand(Guid UserId, Guid NotificationId) : IRequest;
