using FadeAfro.Application.Features.Notifications.Common;

namespace FadeAfro.Application.Features.Notifications.GetMyNotifications;

public record GetMyUnreadNotificationsResponse(List<NotificationDto> Notifications);