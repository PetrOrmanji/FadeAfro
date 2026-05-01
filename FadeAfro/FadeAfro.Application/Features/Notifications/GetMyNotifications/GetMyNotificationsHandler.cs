using FadeAfro.Application.Features.Notifications.Common;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Notifications.GetMyNotifications;

public class GetMyNotificationsHandler : IRequestHandler<GetMyNotificationsQuery, GetMyNotificationsResponse>
{
    private readonly INotificationRepository _notificationRepository;
    
    public GetMyNotificationsHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<GetMyNotificationsResponse> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = 
            await _notificationRepository.GetNotificationsByUserId(request.UserId);

        var notificationDtos = new List<NotificationDto>();

        foreach (var notification in notifications)
        {
            var notificationDto = new NotificationDto(
                notification.Id,
                notification.Text,
                notification.IsRead);
            
            notificationDtos.Add(notificationDto);
        }

        return new GetMyNotificationsResponse(notificationDtos);
    }
}