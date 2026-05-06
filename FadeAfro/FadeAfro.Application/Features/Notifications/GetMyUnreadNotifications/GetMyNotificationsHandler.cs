using FadeAfro.Application.Features.Notifications.Common;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Notifications.GetMyUnreadNotifications;

public class GetMyNotificationsHandler : IRequestHandler<GetMyUnreadNotificationsQuery, GetMyUnreadNotificationsResponse>
{
    private readonly INotificationRepository _notificationRepository;
    
    public GetMyNotificationsHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<GetMyUnreadNotificationsResponse> Handle(GetMyUnreadNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = 
            await _notificationRepository.GetUnreadNotificationsByUserId(request.UserId);

        var notificationDtos = new List<NotificationDto>();

        foreach (var notification in notifications)
        {
            var notificationDto = new NotificationDto(
                notification.Id,
                notification.Text);
            
            notificationDtos.Add(notificationDto);
        }

        return new GetMyUnreadNotificationsResponse(notificationDtos);
    }
}