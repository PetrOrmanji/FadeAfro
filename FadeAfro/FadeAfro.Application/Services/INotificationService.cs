namespace FadeAfro.Application.Services;

public interface INotificationService
{
    Task NotifyAsync(Guid userId, long telegramId, string text);
}