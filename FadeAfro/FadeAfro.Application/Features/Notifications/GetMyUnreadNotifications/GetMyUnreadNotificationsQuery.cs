using MediatR;

namespace FadeAfro.Application.Features.Notifications.GetMyUnreadNotifications;

public record GetMyUnreadNotificationsQuery(Guid UserId) : IRequest<GetMyUnreadNotificationsResponse>;