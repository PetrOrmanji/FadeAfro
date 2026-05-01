namespace FadeAfro.Application.Features.Notifications.Common;

public record NotificationDto(
    Guid Id,
    string Text,
    bool IsRead);