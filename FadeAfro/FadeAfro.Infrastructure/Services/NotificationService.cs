using FadeAfro.Application.Services;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace FadeAfro.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly INotificationRepository _notificationRepository;
    
    public NotificationService(
        ILogger<NotificationService> logger,
        ITelegramBotClient telegramBotClient,
        INotificationRepository notificationRepository)
    {
        _logger = logger;
        _telegramBotClient = telegramBotClient;
        _notificationRepository = notificationRepository;
    }
    
    public async Task NotifyAsync(Guid userId, long telegramId, string text)
    {
        var notification = new Notification(userId, text);
        await _notificationRepository.AddAsync(notification);
    
        await SendTelegramMessageAsync(telegramId, text);
    }
    
    private async Task SendTelegramMessageAsync(long telegramId, string text)
    {
        try
        {
            await _telegramBotClient.SendMessage(telegramId, text);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Failed to send Telegram message to {telegramId}");
        }
    }
}