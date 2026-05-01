using FadeAfro.Application.Features.Notifications.Common;

namespace FadeAfro.Application.Features.Notifications.GetMyNotifications;

public record GetMyNotificationsResponse(List<NotificationDto> Notifications);