using FadeAfro.Application.Features.Notifications.Common;

namespace FadeAfro.Application.Features.Notifications.GetMyUnreadNotifications;

public record GetMyUnreadNotificationsResponse(List<NotificationDto> Notifications);