using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Notifications.GetMyUnreadNotificationsCount;

public class GetMyUnreadNotificationsCountHandler 
    : IRequestHandler<GetMyUnreadNotificationsCountQuery, GetMyUnreadNotificationsCountResponse>
{
    private readonly INotificationRepository _notificationRepository;
    
    public GetMyUnreadNotificationsCountHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }
    
    public async Task<GetMyUnreadNotificationsCountResponse> Handle(
        GetMyUnreadNotificationsCountQuery request, CancellationToken cancellationToken)
    {
        var unreadNotificationsCount = 
            await _notificationRepository.GetUnreadNotificationsCountByUserId(request.UserId);

        return new GetMyUnreadNotificationsCountResponse(unreadNotificationsCount);
    }
}