using FadeAfro.Domain.Exceptions.Notification;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Notifications.MarkMyNotificationsAsRead;

public class MarkMyNotificationsAsReadHandler : IRequestHandler<MarkMyNotificationsAsReadCommand>
{
    private readonly INotificationRepository _notificationRepository;
    
    public MarkMyNotificationsAsReadHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task Handle(MarkMyNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        var notifications = 
            await _notificationRepository.GetNotificationsByUserId(request.UserId);

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }
        
        await _notificationRepository.UpdateRangeAsync(notifications);
    }
}