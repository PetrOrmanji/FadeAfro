using MediatR;

namespace FadeAfro.Application.Features.Notifications.GetMyNotifications;

public record GetMyUnreadNotificationsQuery(Guid UserId) : IRequest<GetMyUnreadNotificationsResponse>;