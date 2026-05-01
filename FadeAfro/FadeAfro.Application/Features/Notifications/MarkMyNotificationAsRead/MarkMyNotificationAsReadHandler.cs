using FadeAfro.Domain.Exceptions.Notification;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Notifications.MarkMyNotificationAsRead;

public class MarkMyNotificationAsReadHandler : IRequestHandler<MarkMyNotificationAsReadCommand>
{
    private readonly INotificationRepository _notificationRepository;
    
    public MarkMyNotificationAsReadHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task Handle(MarkMyNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = 
            await _notificationRepository.GetNotificationByIdAndUserId(request.NotificationId, request.UserId);

        if (notification is null)
            throw new NotificationNotFoundException();
        
        notification.MarkAsRead();
        
        await _notificationRepository.UpdateAsync(notification);
    }
}